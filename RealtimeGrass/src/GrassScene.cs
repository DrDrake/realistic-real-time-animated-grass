using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.D3DCompiler;
using SlimDX.DXGI;
using SlimDX.DirectInput;


using RealtimeGrass.Utility;
using RealtimeGrass.Rendering;
using RealtimeGrass.Rendering.UI;
using RealtimeGrass.Rendering.Mesh;
using RealtimeGrass.UI.Binding;
using RealtimeGrass.UI;
using RealtimeGrass.Entities;
using Plane = RealtimeGrass.Entities.Plane;


namespace RealtimeGrass
{
    /// <summary>
    /// Demonstrates how to render a simple colored triangle with Direct3D10.
    /// </summary>
    class GrassScene : SlimScene
    {
        #region Implementation Detail

        private RenderTargetView                m_mainRTView;
        private RenderTargetView                m_postProcessRTView;
        private DepthStencilState               m_depthStencilState;
        private DepthStencilView                m_depthStencilView;

        private Camera                          m_camera;
        private InputController                 m_input;
        private Soundmanager                    m_soundManager;
        private Clock                           m_clock;
        private readonly Bindable<float>        m_output = new Bindable<float>();
        private CoordinateSystem                m_coordSys;
        private Plane                           m_plane;
        private Skybox                          m_skybox;
        private Model                           m_Jupiter;
        private Heightmap                       m_heightmap;
        private Heightmap                       m_heightmapLOW;
        private Grass                           m_grass;
        private Grass                           m_grassLOW;
        public Model                            m_butterfly { get; set; }
        public Model                            m_LightDir;
        private Light                           m_light;

        private Matrix                          m_proj;
        private Matrix                          m_view;
        //Sound testing
        private bool                            m_played = false;
        private ImageLoadInformation            m_defaultLoadInfo;


        #endregion

        /// <summary>
        /// In a derived class, implements logic to initialize the sample.
        /// </summary>
        protected override void OnInitialize()
        {
            var hudText = new Element();
            hudText.SetBinding("Label", m_output);
            UserInterface.Container.Add(hudText);

            m_camera = new RealtimeGrass.Utility.Camera(
                new Vector3(0, 3, -10), // position
                new Vector3(0, 0, 0), // lookat
                Vector3.UnitZ, // direction
                Vector3.UnitY, // up
                1.0f, // moveSpeedMouse
                80.0f, // moveSpeedKeys
                1.0f, // near
                3000.0f, // far
                45.0f, // fov
                WindowWidth / WindowHeight //aspect ratio
            );
            m_soundManager = new Soundmanager();
            m_input = new InputController(m_form);

            m_clock = new Clock();
            m_clock.Start();

            DeviceSettings10 settings = new DeviceSettings10
            {
                AdapterOrdinal = 0,
                CreationFlags = DeviceCreationFlags.None,
                Width = WindowWidth,
                Height = WindowHeight
            };

            InitializeDevice(settings);
        }

        protected override void OnResourceLoad()
        {
            try
            {
                Texture2D texture = Texture2D.FromSwapChain<Texture2D>(Context10.SwapChain, 0);
                m_mainRTView = new RenderTargetView(Context10.Device, texture);
            
                //Setting up a float Rendertarget
                Texture2DDescription texturePostDesc = new Texture2DDescription();
                texturePostDesc.Format = Format.R16G16B16A16_Float;
                texturePostDesc.Width = texture.Description.Width;
                texturePostDesc.Height = texture.Description.Height;
                texturePostDesc.ArraySize = 1;
                texturePostDesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
                texturePostDesc.Usage = ResourceUsage.Default;
                texturePostDesc.MipLevels = 1;
                texturePostDesc.SampleDescription = new SampleDescription(1, 0);
                texture.Dispose();

                Texture2D texturePost = new Texture2D(Context10.Device, texturePostDesc);
                m_postProcessRTView = new RenderTargetView(Context10.Device, texturePost);
                texturePost.Dispose();            

                CreateDepthBuffer();
                var dssd = new DepthStencilStateDescription
                {
                    IsDepthEnabled = true,
                    IsStencilEnabled = false,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthComparison = Comparison.Less
                };
                m_depthStencilState = DepthStencilState.FromDescription(Context10.Device, dssd);

                m_defaultLoadInfo = ImageLoadInformation.FromDefaults();

                //a Light-------------------------------------------------------------------
                m_light = new Light(new Vector3(1.0f, 1.0f, 1.0f), new Vector3(-1.0f, -1.0f, 1.0f));

                //a symplistic Coordsystem---------------------------------------------------
                m_coordSys = new CoordinateSystem(0.1f, 0.9f, 0.8f, 64);
                m_coordSys.Init(Context10.Device, "Resources/shader/CoordinateSystem.fx", null);
                
                //the water plane------------------------------------------------------------
                TextureFormat texFormat1 = new TextureFormat(
                    "Resources/texture/wasser.jpg",
                    m_defaultLoadInfo,
                    TextureType.TextureTypeDiffuse,
                    "model_texture"
                );
                //For storing Info about used Textures
                List<TextureFormat> textureFormats1 = new List<TextureFormat>();
                textureFormats1.Add(texFormat1);

                //Water ScaleX, ScaleY
                m_plane = new Plane(0.1f, 0.9f, 0.8f, 64, 10000.0f, 10000.0f);
                m_plane.Init(Context10.Device, "Resources/shader/Water.fx", textureFormats1);
            
                //a fancy skybox--------------------------------------------------------
                ImageLoadInformation loadInfoCube = ImageLoadInformation.FromDefaults();
                loadInfoCube.OptionFlags = ResourceOptionFlags.TextureCube;

                TextureFormat texFormat2 = new TextureFormat(
                    "Resources/texture/Sky_Miramar.dds",
                    loadInfoCube,
                    TextureType.TextureTypeCube,
                    "model_texture02"
                );
                
                TextureFormat texFormat21 = new TextureFormat(
                    "Resources/texture/Sky_Stormydays.dds",
                    loadInfoCube,
                    TextureType.TextureTypeCube,
                    "model_texture01"
                );
                
                TextureFormat texFormat22 = new TextureFormat(
                    "Resources/texture/Sky_Grimmnight.dds",
                    loadInfoCube,
                    TextureType.TextureTypeCube,
                    "model_texture04"
                );
                
                TextureFormat texFormat23 = new TextureFormat(
                    "Resources/texture/Sky_Violentdays.dds",
                    loadInfoCube,
                    TextureType.TextureTypeCube,
                    "model_texture03"
                );

                List<TextureFormat> textureFormats2 = new List<TextureFormat>();
                textureFormats2.Add(texFormat2);
                textureFormats2.Add(texFormat21);
                textureFormats2.Add(texFormat22);
                textureFormats2.Add(texFormat23);

                m_skybox = new Skybox(0.1f, 0.9f, 0.8f, 64);
                m_skybox.Init(Context10.Device, "Resources/shader/Skybox.fx", textureFormats2);

                //Jupiter----------------------------------------------------------

                TextureFormat texFormat3 = new TextureFormat(
                    "Resources/texture/jupiter1024x512.jpg",
                    m_defaultLoadInfo,
                    TextureType.TextureTypeDiffuse,
                    "model_texture"
                );
                List<TextureFormat> textureFormats3 = new List<TextureFormat>();
                textureFormats3.Add(texFormat3);

                m_Jupiter = new Model(0.8f, 0.9f, 0.8f, 64, "Resources/mesh/Jupiter.smd");
                m_Jupiter.Init(Context10.Device, "Resources/shader/ModelTextured.fx", textureFormats3);

                //Heightmap--------------------------------------------------------------

                TextureFormat texFormat4 = new TextureFormat(
                    "Resources/texture/boden01.jpg",
                    m_defaultLoadInfo,
                    TextureType.TextureTypeDiffuse,
                    "model_texture_high"
                );

                TextureFormat texFormat4_2 = new TextureFormat(
                    "Resources/texture/grass.png",
                    m_defaultLoadInfo,
                    TextureType.TextureTypeDiffuse,
                    "model_texture_low"
                );
                List<TextureFormat> textureFormats4 = new List<TextureFormat>();
                textureFormats4.Add(texFormat4);
                textureFormats4.Add(texFormat4_2);

                m_heightmap = new Heightmap(0.1f, 0.9f, 0.8f, 64, "Resources/texture/huegel500x500.jpg", 1f, -128f, 0, 60);
                m_heightmap.Init(Context10.Device, "Resources/shader/ModelTexturedLOD.fx", textureFormats4);

                m_heightmapLOW = new Heightmap(0.1f, 0.9f, 0.8f, 64, "Resources/texture/huegelLOW128x128.jpg", 128f, -8192f,150,80);
                m_heightmapLOW.Init(Context10.Device, "Resources/shader/ModelTexturedLOD.fx", textureFormats4);

                //Grass---------------------------------------------------------------------------------

                TextureFormat texFormat5 = new TextureFormat(
                    "Resources/texture/GrassDiffuse01.jpg",
                    m_defaultLoadInfo,
                    TextureType.TextureTypeDiffuse,
                    "grass_diffuse01"
                );
                TextureFormat texFormat6 = new TextureFormat(
                    "Resources/texture/GrassDiffuse02.jpg",
                    m_defaultLoadInfo,
                    TextureType.TextureTypeDiffuse,
                    "grass_diffuse02"
                );
                TextureFormat texFormat7 = new TextureFormat(
                    "Resources/texture/GrassAlpha.jpg",
                    m_defaultLoadInfo,
                    TextureType.TextureTypeDiffuse,
                    "grass_alpha"
                );
                
                TextureFormat texFormat8 = new TextureFormat(
                    "Resources/texture/noise1024x773.jpg",
                    m_defaultLoadInfo,
                    TextureType.TextureTypeDiffuse,
                    "grass_noise"
                );
                
                TextureFormat texFormat9 = new TextureFormat(
                    "Resources/texture/phasenverschiebung.jpg",
                    m_defaultLoadInfo,
                    TextureType.TextureTypeDiffuse,
                    "grass_shift"
                );

                List<TextureFormat> textureFormats5 = new List<TextureFormat>();
                textureFormats5.Add(texFormat5);
                textureFormats5.Add(texFormat6);
                textureFormats5.Add(texFormat7);
                textureFormats5.Add(texFormat8);
                textureFormats5.Add(texFormat9);

                m_grass = new Grass(0.1f, 0.9f, 0.8f, 100, m_heightmap.Roots, m_heightmap.NumberOfElements);
                m_grass.Init(Context10.Device, "Resources/shader/GrassTextured.fx", textureFormats5);

//                m_grassLOW = new Grass(0.1f, 0.9f, 1.0f, 128, m_heightmapLOW.Roots, m_heightmapLOW.NumberOfElements);
//                m_grassLOW.Init(Context10.Device, "Resources/shader/GrassTextured.fx", textureFormats5);

                //Butterfly----------------------------------------------------------

                TextureFormat texFormat10 = new TextureFormat(
                    "Resources/texture/butterfly.png",
                    m_defaultLoadInfo,
                    TextureType.TextureTypeDiffuse,
                    "model_texture"
                );
                List<TextureFormat> textureFormats10 = new List<TextureFormat>();
                textureFormats10.Add(texFormat10);

                m_butterfly = new Model(0.3f, 0.9f, 0.8f, 64, "Resources/mesh/butterfly.smd");
                m_butterfly.Init(Context10.Device, "Resources/shader/ButterflyTextured.fx", textureFormats10);



               //LightDir Cylinder--------------------------------------------------------
               TextureFormat texFormat11 = new TextureFormat(
                   "Resources/texture/LightDir.png",
                   m_defaultLoadInfo,
                   TextureType.TextureTypeDiffuse,
                   "model_texture"
               );

                List<TextureFormat> textureFormats11 = new List<TextureFormat>();
                textureFormats11.Add(texFormat11);

                m_LightDir = new Model(0.3f, 0.9f, 0.8f, 64, "Resources/mesh/LightDir.smd");
                m_LightDir.Init(Context10.Device, "Resources/shader/ModelTextured.fx", textureFormats11);

                //-----------------------------------------
                //Sounds
               // m_soundManager.playSingle("Resources/sounds/rustleWindwithBirds.wav");
            }
            catch(Exception e)
            {
                Console.WriteLine("Catched Exception in Class GrassScene in Method OnResourceLoad: " + e.Message);
                OnResourceUnload();
            }
        }

        protected void processInput()
        {
            KeyboardState keyState = m_input.ReadKeyboard();
            MouseState mouseState = m_input.ReadMouse();
            if (keyState != null)
            {
                //Keys released
                foreach (Key key in keyState.ReleasedKeys)
                {
                    switch (key)
                    {
                        case (Key.LeftShift):
                            if (m_camera.isSlowMoving == true)
                            {
                                m_camera.m_MoveSpeedKeys += 30.0f;
                            }
                            m_camera.isSlowMoving = false;
                            break;
                    }
                }

                //Keys pressed
                foreach (Key key in keyState.PressedKeys)
                {
                    switch (key)
                    {
                        case (Key.P):
                            if (! m_played)
                            {
                                m_played = true;
                                m_soundManager.playSingle("resources/music/C_C_Red_Alert_2_music_Hell_March_2.wav");
                            }
                            break;
                        case (Key.W):
                            m_camera.AddToCamera(0f, 0f, FrameDelta, out m_proj, out m_view);
                            break;
                        case (Key.S):
                            m_camera.AddToCamera(0f, 0f, -FrameDelta, out m_proj, out m_view);
                            break;
                        case (Key.A):
                            m_camera.AddToCamera(FrameDelta, 0f, 0f, out m_proj, out m_view);
                            break;
                        case (Key.D):
                            m_camera.AddToCamera(-FrameDelta, 0f, 0f, out m_proj, out m_view);
                            break;
                        case (Key.Space):
                            m_camera.AddToCamera(0f, FrameDelta, 0f, out m_proj, out m_view);
                            break;
                        /* This syntax means that each 'C' as well as 'LeftControll' trigger the case. 
                         * This is intentional. (There seems to be no boolean operators in C# switch-cases)*/
                        case (Key.LeftControl): 
                        case (Key.C):
                            m_camera.AddToCamera(0f, -FrameDelta, 0f, out m_proj, out m_view);
                            break;
                        case (Key.LeftShift):
                            if (m_camera.isSlowMoving == false)
                            {
                                m_camera.m_MoveSpeedKeys -= 30.0f;
                            }
                            m_camera.isSlowMoving = true;
                            break;
                        case (Key.Escape):
                            m_isFormClosed = true;
                            Quit();
                            break;
                        case (Key.Return & Key.LeftAlt):
                            OnResourceUnload();
                            isFullScreen = !isFullScreen;

                            if (Context9 != null)
                            {
                                userInterfaceRenderer.Dispose();

                                Context9.PresentParameters.BackBufferWidth = m_configuration.WindowWidth;
                                Context9.PresentParameters.BackBufferHeight = m_configuration.WindowHeight;
                                Context9.PresentParameters.Windowed = !isFullScreen;

                                if (!isFullScreen)
                                    m_form.MaximizeBox = true;

                                Context9.Device.Reset(Context9.PresentParameters);

                                userInterfaceRenderer = new UserInterfaceRenderer9(Context9.Device, m_form.ClientSize.Width, m_form.ClientSize.Height);
                            }
                            else if (Context10 != null)
                            {
                                userInterfaceRenderer.Dispose();

                                Context10.SwapChain.ResizeBuffers(1, WindowWidth, WindowHeight, Context10.SwapChain.Description.ModeDescription.Format, SwapChainFlags.AllowModeSwitch);
                                Context10.SwapChain.SetFullScreenState(isFullScreen, null);

                                userInterfaceRenderer = new UserInterfaceRenderer10(Context10.Device, WindowWidth, WindowHeight);
                            }

                            OnResourceLoad();
                            break;
                    }
                }
            }
            if (mouseState != null)
            {
                m_camera.RotateAroundPosition(FrameDelta * mouseState.X, -FrameDelta * mouseState.Y, out m_proj, out m_view);
            }
        }

        protected override void OnRenderBegin()
        {
            Context10.Device.OutputMerger.DepthStencilState = m_depthStencilState;
            //Render to offscreen RenderTarget
            Context10.Device.OutputMerger.SetTargets(m_depthStencilView, m_mainRTView);

            Context10.Device.Rasterizer.SetViewports(new Viewport(0, 0, WindowWidth, WindowHeight, 0.0f, 1.0f));
            Context10.Device.ClearRenderTargetView(m_mainRTView, new Color4(0.3f, 0.3f, 0.3f));
            Context10.Device.ClearDepthStencilView(m_depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

            processInput();

            m_camera.Update(out m_proj, out m_view);
        }

        protected override void OnRender()
        {
            try
            {
                //For anything with time use 'FrameDelta', don't use anything else
                double a = m_clock.Check();
                m_output.Value = 5*(float)System.Math.Sin(a);

                //Not needed anymore, shader does depthtest-trick
                //SetDepthTest(false);

                Matrix world = Matrix.Identity;
                Matrix.Scaling(1.0f, 1.0f, 1.0f, out world);
                Matrix temp;
                Matrix.Translation(m_camera.m_Position.X, m_camera.m_Position.Y, m_camera.m_Position.Z, out temp);
                Matrix.Multiply(ref temp, ref world, out world);
                m_skybox.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                m_skybox.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                m_skybox.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                m_skybox.Effect.GetVariableByName("time").AsScalar().Set(m_clock.Check());
                m_skybox.Draw();
            
                //SetDepthTest(true);

                world = Matrix.Identity;
                Matrix.Translation(0.0f, 50f, 0.0f, out world);
                m_coordSys.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                m_coordSys.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                m_coordSys.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                m_coordSys.Draw();


                world = Matrix.Identity;

                // Butterflies
                Place_butterfly(30, 40, 90);
                Place_butterfly(470, 480, 100);
                Place_butterfly(0, 540, 100);
                Place_butterfly(500, 250, 100);
                Place_butterfly(270, 60, 80);
                Place_butterfly(60, 350, 80);



                Matrix tempMatrix;
                world = Matrix.Identity;
                Matrix.Translation(0.0f, -0.001f, 0.0f, out world);
                Matrix.Scaling(1.0f, 1.0f, 1.0f, out tempMatrix);
                Matrix.Multiply(ref tempMatrix, ref world, out world);

                //+X
                m_heightmapLOW.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                m_heightmapLOW.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                m_heightmapLOW.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                m_heightmapLOW.Effect.GetVariableByName("time").AsScalar().Set(m_clock.Check());
                m_heightmapLOW.Effect.GetVariableByName("cTexScal").AsScalar().Set(36);
                m_heightmapLOW.Effect.GetVariableByName("cam_Pos").AsVector().Set(m_camera.m_Position);
                m_heightmapLOW.SetShaderMaterial();
                m_heightmapLOW.Draw();//*


                world = Matrix.Identity;
                m_heightmap.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                m_heightmap.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                m_heightmap.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                m_heightmap.Effect.GetVariableByName("time").AsScalar().Set(m_clock.Check());
                m_heightmap.Effect.GetVariableByName("cTexScal").AsScalar().Set(6);
                m_heightmap.Effect.GetVariableByName("cam_Pos").AsVector().Set(m_camera.m_Position);
                m_heightmap.SetShaderMaterial();
                m_heightmap.Draw();//



                world = Matrix.Identity;

                m_plane.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                m_plane.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                m_plane.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                m_plane.SetShaderMaterial();
                m_plane.Draw();

                world = Matrix.Identity;

                m_grass.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                m_grass.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                m_grass.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                m_grass.Effect.GetVariableByName("time").AsScalar().Set(m_clock.Check());
                m_grass.SetShaderMaterial();
                //AHHH : m_grass.Effect.GetVariableByName("ambientLight").AsVector().Set(Vector4(1.0f,1.0f,1.0f,1.0f));
                m_grass.Effect.GetVariableByName("halfwayWS").AsVector().Set(m_camera.CalcHalfWay(m_light.Direction));
                //m_grass.Effect.GetVariableByName("l_color").AsScalar().Set(l_light.Color());
                m_grass.Effect.GetVariableByName("l_dirWS").AsVector().Set(m_light.Direction);
                m_grass.Effect.GetVariableByName("cam_Pos").AsVector().Set(m_camera.m_Position);
                m_grass.Draw();//

                world = Matrix.Identity;

                //m_grassLOW.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                //m_grassLOW.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                //m_grassLOW.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                //m_grassLOW.Effect.GetVariableByName("time").AsScalar().Set(m_clock.Check());
                //m_grassLOW.SetShaderMaterial();
                ////m_grass.Effect.GetVariableByName("ambientLight").AsVector().Set(Vector4(1.0f,1.0f,1.0f,1.0f));
                //m_grassLOW.Effect.GetVariableByName("halfwayWS").AsVector().Set(m_camera.CalcHalfWay(m_light.Direction));
                //m_grassLOW.Effect.GetVariableByName("cam_Pos").AsVector().Set(m_camera.m_Position);
                ////m_grass.Effect.GetVariableByName("l_color").AsScalar().Set(l_light.Color());
                ////m_grass.Effect.GetVariableByName("l_dir").AsScalar().Set(l_light.Dir());
                //m_grassLOW.Draw();//*/

                world = Matrix.Identity;
                Matrix.Translation(0, 0, 100, out world);

                m_Jupiter.m_Rotation.Y = m_Jupiter.m_Rotation.Y + (FrameDelta * 0.1f) % 360;
                m_Jupiter.m_SelfRotation.Y = m_Jupiter.m_SelfRotation.Y + (FrameDelta * 0.5f) % 360;

                world = Matrix.Identity;
                Matrix rotationTemp;
                Matrix translationTemp;
            
                Matrix.RotationY(m_Jupiter.m_Rotation.Y, out rotationTemp);
                Matrix.Multiply(ref rotationTemp, ref world, out world);

                Matrix.Translation(0, 300, 800, out translationTemp);
                Matrix.Multiply(ref translationTemp, ref world, out world);

                Matrix.RotationY(m_Jupiter.m_SelfRotation.Y, out rotationTemp);
                Matrix.Multiply(ref rotationTemp, ref world, out world);

                //To compensate blender coord system y==z
                Matrix.RotationX((float) Math.PI / 2, out rotationTemp);
                Matrix.Multiply(ref rotationTemp, ref world, out world);

                m_Jupiter.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                m_Jupiter.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                m_Jupiter.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                m_Jupiter.SetShaderMaterial();
                m_Jupiter.Draw();

                world = Matrix.Identity;
                //To compensate blender coord system y==z

                Matrix.RotationX((float)Math.PI / 2, out rotationTemp);
                Matrix.Multiply(ref rotationTemp, ref world, out world);
                Matrix.Translation(0, 80, 0, out world);

                m_LightDir.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                m_LightDir.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                m_LightDir.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                m_LightDir.SetShaderMaterial();
                m_LightDir.Draw();

                /*
                //Final Pass 
                Context10.Device.OutputMerger.DepthStencilState = m_depthStencilState;
                //Render to backbuffer 
                Context10.Device.OutputMerger.SetTargets(m_depthStencilView, m_mainRTView);

                Context10.Device.Rasterizer.SetViewports(new Viewport(0, 0, WindowWidth, WindowHeight, 0.0f, 1.0f));
                Context10.Device.ClearRenderTargetView(m_mainRTView, new Color4(0.3f, 0.3f, 0.3f));
                Context10.Device.ClearDepthStencilView(m_depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

                //Draw Fullscreen Quad
                /*svQuad[0].pos = D3DXVECTOR4(-1.0f, 1.0f, 0.5f, 1.0f);
                svQuad[0].tex = D3DXVECTOR2(0.0f, 0.0f);
                svQuad[1].pos = D3DXVECTOR4(1.0f, 1.0f, 0.5f, 1.0f);
                svQuad[1].tex = D3DXVECTOR2(1.0f, 0.0f);
                svQuad[2].pos = D3DXVECTOR4(-1.0f, -1.0f, 0.5f, 1.0f);
                svQuad[2].tex = D3DXVECTOR2(0.0f, 1.0f);
                svQuad[3].pos = D3DXVECTOR4(1.0f, -1.0f, 0.5f, 1.0f);
                svQuad[3].tex = D3DXVECTOR2(1.0f, 1.0f);//*/
            }
            catch (Direct3D10Exception e)
            {
                Console.WriteLine("Catched Exception in main: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Catched Exception in main: " + e.Message);
            }
        }

        protected void SetDepthTest(bool isUsingDepthTest)
        {
            DepthStencilStateDescription dsStateDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = isUsingDepthTest,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
            };
            DepthStencilState depthState = DepthStencilState.FromDescription(Context10.Device, dsStateDesc);
            Context10.Device.OutputMerger.DepthStencilState = depthState;
        }

        protected override void OnRenderEnd()
        {
            Context10.SwapChain.Present(0, SlimDX.DXGI.PresentFlags.None);
        }

        private void CreateDepthBuffer()
        {
            var depthBufferDesc = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.D32_Float,
                Height = WindowHeight,
                Width = WindowWidth,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default
            };

            Texture2D depthBuffer = new Texture2D(Context10.Device, depthBufferDesc);
            m_depthStencilView = new DepthStencilView(Context10.Device, depthBuffer);
            depthBuffer.Dispose();
        }

        protected override void OnResourceUnload()
        {
            m_mainRTView.Dispose();
            m_postProcessRTView.Dispose();
            m_depthStencilView.Dispose();
            m_depthStencilState.Dispose();

            m_soundManager.Dispose();
            m_coordSys.Dispose();
            m_plane.Dispose();
            m_skybox.Dispose();
            m_Jupiter.Dispose();
            m_grass.Dispose();
            m_heightmap.Dispose();
            m_heightmapLOW.Dispose();
            m_butterfly.Dispose();
            m_input.Dispose();

            //Something still alive, but what?
        }
        protected void Place_butterfly(float pos_X, float pos_Y, float shift_Y) {
            float m_speed_X;
            float m_speed_Y;
            float m_speed_Z;
            float m_shift_X;
            float m_shift_Y;
            float m_shift_Ya = shift_Y;
            float m_shift_Z;
            float m_direction;
            float i = pos_X;
            float j = pos_Y;

            Matrix world = Matrix.Identity;
            Matrix tempMatrix;

                        m_speed_X = 5;
                        m_speed_Y = 5;
                        m_speed_Z = 10;
                        m_shift_X = 20+ i;
                        m_shift_Y = m_shift_Ya + m_shift_Ya/8 * (float)(Math.Sin((double)m_clock.Check()));
                        m_shift_Z = 10 + j;
                        m_direction = -1;

                        Matrix.Translation(m_speed_X * (float)(Math.Sin((double)m_clock.Check() * m_direction)) + m_shift_X, m_speed_Y * (float)(Math.Cos((double)m_clock.Check() * m_direction)) + m_shift_Y, m_speed_Z * (float)(Math.Cos((double)m_clock.Check() * m_direction)) + m_shift_Z, out world);

                        Matrix.RotationY(-30 * m_direction + m_clock.Check() * m_direction % 360, out tempMatrix);
                        Matrix.Multiply(ref tempMatrix, ref world, out world);

                        Matrix.Scaling(3.5f, 3.5f, 3.5f, out tempMatrix);
                        Matrix.Multiply(ref tempMatrix, ref world, out world);

                        m_butterfly.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                        m_butterfly.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                        m_butterfly.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                        m_butterfly.Effect.GetVariableByName("time").AsScalar().Set(m_clock.Check());
                        m_butterfly.Effect.GetVariableByName("halfwayWS").AsVector().Set(m_camera.CalcHalfWay(m_light.Direction));
                        m_butterfly.Effect.GetVariableByName("l_dirWS").AsVector().Set(m_light.Direction);
                        m_butterfly.SetShaderMaterial();
                        m_butterfly.Draw();//*/

                        world = Matrix.Identity;
                        m_speed_X = 10;
                        m_speed_Y = 5;
                        m_speed_Z = 5;
                        m_shift_X = 30+i;
                        m_shift_Y = m_shift_Ya + m_shift_Ya / 8 * (float)(Math.Sin((double)m_clock.Check() + j / 125)) + 15;
                        m_shift_Z = 30 + j;
                        m_direction = -1;

                        Matrix.Translation(m_speed_X * (float)(Math.Sin((double)m_clock.Check() * m_direction + Math.PI / 2)) + m_shift_X, m_speed_Y * (float)(Math.Cos((double)m_clock.Check() * m_direction + Math.PI / 2)) + m_shift_Y, m_speed_Z * (float)(Math.Cos((double)m_clock.Check() * m_direction + Math.PI / 2)) + m_shift_Z, out world);

                        Matrix.RotationY(-120 * m_direction + m_clock.Check() * m_direction % 360, out tempMatrix);
                        Matrix.Multiply(ref tempMatrix, ref world, out world);

                        Matrix.Scaling(3.2f, 3.2f, 3.2f, out tempMatrix);
                        Matrix.Multiply(ref tempMatrix, ref world, out world);

                        m_butterfly.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                        m_butterfly.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                        m_butterfly.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                        m_butterfly.Effect.GetVariableByName("time").AsScalar().Set(m_clock.Check());
                        m_butterfly.Effect.GetVariableByName("wingdimension").AsScalar().Set(1.2f);
                        m_butterfly.Effect.GetVariableByName("speed").AsScalar().Set(7.5f);
                        m_butterfly.Effect.GetVariableByName("halfwayWS").AsVector().Set(m_camera.CalcHalfWay(m_light.Direction));
                        m_butterfly.Effect.GetVariableByName("l_dirWS").AsVector().Set(m_light.Direction);
                        m_butterfly.SetShaderMaterial();
                        m_butterfly.Draw();//*/

                        world = Matrix.Identity;
                        m_speed_X = 8;
                        m_speed_Y = 2;
                        m_speed_Z = 8;
                        m_shift_X = 25+i;
                        m_shift_Y = m_shift_Ya + m_shift_Ya / 8 * (float)(Math.Sin((double)m_clock.Check() + j / 125)) + 10;
                        m_shift_Z = 25 + j;
                        m_direction = 1;

                        Matrix.Translation(m_speed_X * (float)(Math.Sin((double)m_clock.Check() * m_direction)) + m_shift_X, m_speed_Y * (float)(Math.Cos((double)m_clock.Check() * m_direction)) + m_shift_Y, m_speed_Z * (float)(Math.Cos((double)m_clock.Check() * m_direction)) + m_shift_Z, out world);

                        Matrix.RotationY(-30 * m_direction + m_clock.Check() * m_direction % 360, out tempMatrix);
                        Matrix.Multiply(ref tempMatrix, ref world, out world);

                        Matrix.Scaling(3.4f, 3.4f, 3.4f, out tempMatrix);
                        Matrix.Multiply(ref tempMatrix, ref world, out world);

                        m_butterfly.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                        m_butterfly.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                        m_butterfly.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                        m_butterfly.Effect.GetVariableByName("time").AsScalar().Set(m_clock.Check());
                        m_butterfly.Effect.GetVariableByName("wingdimension").AsScalar().Set(1.8f);
                        m_butterfly.Effect.GetVariableByName("speed").AsScalar().Set(8f);

                        m_butterfly.SetShaderMaterial();
                        m_butterfly.Draw();//*/
      
                }

    }
}
