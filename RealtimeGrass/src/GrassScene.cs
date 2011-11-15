using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.D3DCompiler;
using SlimDX.DXGI;
using SlimDX.DirectInput;

using RealtimeGrass.Rendering;
using RealtimeGrass.Rendering.UI;
using RealtimeGrass.Utility;
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
        private SimpleGrass                     m_straw;
        private Skybox                          m_skybox;
        private float[,]                        m_strawSize;
        private Model                           m_Jupiter;
        private Heightmap                       m_heightmap;
        private Grass                           m_grass;

        private Matrix                          m_proj;
        private Matrix                          m_view;
        //Sound testing
        private bool                            m_played = false;

        #endregion

        /// <summary>
        /// In a derived class, implements logic to initialize the sample.
        /// </summary>
        protected override void OnInitialize()
        {
            var hudText = new Element();
            hudText.SetBinding("Label", m_output);
            UserInterface.Container.Add(hudText);

            m_camera = new Camera(
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

            m_strawSize = new float[100,100];
            System.Random r = new System.Random();

            for (int col = 0; col < 100; ++col)
            {
                for (int row = 0; row < 100; ++row)
                {
                    m_strawSize.SetValue((float)r.NextDouble() * 0.1f, col, row);
                }
            }

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
            
                //a symplistic Coordsystem---------------------------------------------------
                m_coordSys = new CoordinateSystem();
                m_coordSys.Init(Context10.Device, "Resources/shader/CoordinateSystem.fx", null);

                //the grass plane------------------------------------------------------------
                //Use FromDefaults() for correct init of ImageLoadInformation
                ImageLoadInformation loadInfo1 = ImageLoadInformation.FromDefaults();

                TextureFormat texFormat1 = new TextureFormat(
                    "Resources/texture/grass4096x4096.jpg",
                    loadInfo1,
                    TextureType.TextureTypeDiffuse,
                    "model_texture"
                );
                //For storing Info about used Textures
                List<TextureFormat> textureFormats1 = new List<TextureFormat>();
                textureFormats1.Add(texFormat1);

                //ScaleX, ScaleY
                m_plane = new Plane(100.0f, 100.0f);
                m_plane.Init(Context10.Device, "Resources/shader/ModelTextured.fx", textureFormats1);
                //a single grass straw----------------------------------------------------
                m_straw = new SimpleGrass(Context10.Device, "Resources/shader/DefaultCamera.fx", null);
            
                //a fancy skybox--------------------------------------------------------
                ImageLoadInformation loadInfo2 = ImageLoadInformation.FromDefaults();
                loadInfo2.OptionFlags = ResourceOptionFlags.TextureCube;

                TextureFormat texFormat2 = new TextureFormat(
                    "Resources/texture/Sky_Interstellar.dds",
                    loadInfo2,
                    TextureType.TextureTypeCube,
                    "model_texture"
                );
                List<TextureFormat> textureFormats2 = new List<TextureFormat>();
                textureFormats2.Add(texFormat2);
            
                m_skybox = new Skybox();
                m_skybox.Init(Context10.Device, "Resources/shader/Skybox.fx", textureFormats2);

                //Jupiter----------------------------------------------------------
                ImageLoadInformation loadInfo3 = ImageLoadInformation.FromDefaults();

                TextureFormat texFormat3 = new TextureFormat(
                    "Resources/texture/jupiter1024x512.jpg",
                    loadInfo3,
                    TextureType.TextureTypeDiffuse,
                    "model_texture"
                );
                List<TextureFormat> textureFormats3 = new List<TextureFormat>();
                textureFormats3.Add(texFormat3);

                m_Jupiter = new Model("Resources/mesh/Jupiter.smd");
                m_Jupiter.Init(Context10.Device, "Resources/shader/ModelTextured.fx", textureFormats3);

                //Heightmap--------------------------------------------------------------
                ImageLoadInformation loadInfo4 = ImageLoadInformation.FromDefaults();

                TextureFormat texFormat4 = new TextureFormat(
                    "Resources/texture/boden2048x2048.jpg",
                    loadInfo4,
                    TextureType.TextureTypeDiffuse,
                    "model_texture"
                );
                List<TextureFormat> textureFormats4 = new List<TextureFormat>();
                textureFormats4.Add(texFormat4);

                m_heightmap = new Heightmap("Resources/texture/huegel1000x1000.jpg");
                m_heightmap.Init(Context10.Device, "Resources/shader/ModelTextured.fx", textureFormats4);

                //Grass---------------------------------------------------------------------------------
                ImageLoadInformation loadInfo5 = ImageLoadInformation.FromDefaults();

                TextureFormat texFormat5 = new TextureFormat(
                    "Resources/texture/GrassDiffuse.bmp",
                    loadInfo5,
                    TextureType.TextureTypeDiffuse,
                    "grass_texture"
                );
                List<TextureFormat> textureFormats5 = new List<TextureFormat>();
                textureFormats5.Add(texFormat5);

                m_grass = new Grass(m_heightmap.Roots, m_heightmap.NumberOfElements);
                m_grass.Init(Context10.Device, "Resources/shader/GrassTextured.fx", textureFormats5);
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
            m_skybox.Draw();
            
            //SetDepthTest(true);

            world = Matrix.Identity;
            m_coordSys.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
            m_coordSys.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
            m_coordSys.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
            m_coordSys.Draw();

            Matrix tempMatrix;

            world = Matrix.Identity;
            Matrix.Translation(0.0f, -0.001f, 0.0f, out world);
            Matrix.Scaling(1.0f, 1.0f, 1.0f, out tempMatrix);
            Matrix.Multiply(ref tempMatrix, ref world, out world);
            //+X
            m_heightmap.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
            m_heightmap.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
            m_heightmap.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
            m_heightmap.Draw();//*/

            m_grass.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
            m_grass.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
            m_grass.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
            m_grass.Draw();//*/
            

            /*for (int col = -50; col < 0; ++col)
            {
                for (int row = -100; row < 0; ++row)
                {
                    world = Matrix.Identity;
                    float randomHight = (float)m_strawSize.GetValue(col+50, row+100);
                    Matrix.Scaling(0.01f, 0.01f + randomHight, 0.01f, out tempMatrix);
                    Matrix.Translation(row * 0.1f, -0.5f, col * 0.1f, out world);
                    Matrix rotationTemp;
                    Matrix.RotationY(col + row + randomHight, out rotationTemp);
                    Matrix.Multiply(ref rotationTemp, ref world, out world);
                    Matrix.Multiply(ref tempMatrix, ref world, out world);
                    m_straw.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                    m_straw.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                    m_straw.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                    m_straw.Draw();
                }
            }*/
            world = Matrix.Identity;
            Matrix.Translation(0, 0, 100, out world);
            m_straw.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
            m_straw.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
            m_straw.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
            m_straw.Draw();

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
            m_Jupiter.Draw();//*/

            // Geometry Shader Test
            for (int col = 0; col < 50; ++col)
            {
                for (int row = 0; row < 50; ++row)
                {
                    /*
                     *  Alle Wurzeln werden mit einer zufälligen Höhe und Ausrichtung erstellt.
                     */
                    world = Matrix.Identity;
                    float randomHight = (float)m_strawSize.GetValue(col, row) * 0.3f;
                    Matrix.Scaling(0.01f, 0.01f + randomHight, 0.01f, out tempMatrix);
                    Matrix.Translation(row * 0.1f, -0.5f, col * 0.1f, out world);
                    Matrix.RotationY(col + row + randomHight, out rotationTemp);
                    Matrix.Multiply(ref rotationTemp, ref tempMatrix, out tempMatrix);
                    Matrix.Multiply(ref tempMatrix, ref world, out world);

                    /*root.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                    root.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                    root.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                    root.Draw();//*/
                }
            }
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

            //world = Matrix.Identity;
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
            m_input.Dispose();
            m_straw.Dispose();
        }

        public Heightmap m_gras { get; set; }
    }
}
