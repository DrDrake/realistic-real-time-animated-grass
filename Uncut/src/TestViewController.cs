using System.Drawing;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.D3DCompiler;
using SlimDX.DXGI;
using SlimDX.DirectInput;

using Uncut.Rendering;
using Uncut.Rendering.UI;
using Uncut.Utility;
using Uncut.UI.Binding;
using Uncut.UI;


namespace Uncut
{
    /// <summary>
    /// Demonstrates how to render a simple colored triangle with Direct3D10.
    /// </summary>
    class TestViewController : SlimSceneController
    {
        #region Implementation Detail

        private RenderTargetView renderTargetView;
        private DepthStencilState depthStencilState;
        private DepthStencilView depthStencilView;
        private Camera m_camera;
        private float m_movementSpeed = 64.0f;
        private InputController m_input;
        private Soundmanager m_soundManager;
        private Clock clock;
        private readonly Bindable<float> output = new Bindable<float>();
        private SimpleCube cube;
        private SimplePlane plane;
        private SimpleGrass straw;
        private SimpleRoot root;
        private float[,] strawSize;

        private Matrix m_proj;
        private Matrix m_view;

        #endregion

        /// <summary>
        /// In a derived class, implements logic to initialize the sample.
        /// </summary>
        protected override void OnInitialize()
        {
            var hudText = new Element();
            hudText.SetBinding("Label", output);
            UserInterface.Container.Add(hudText);

            m_camera = new Camera(
                new Vector3(0, 3, 10), // position
                new Vector3(0, 0, 0), // lookat
                Vector3.UnitZ, // direction
                Vector3.UnitY, // up
                0.3f, // moveSpeedMouse
                0.3f, // moveSpeedKeys
                1.0f, // near
                1000.0f, // far
                45.0f, // fov
                WindowWidth / WindowHeight //aspect ratio
            );
            m_soundManager = new Soundmanager();
            m_input = new InputController(m_form);

            clock = new Clock();
            clock.Start();

            strawSize = new float[100,100];
            System.Random r = new System.Random();

            for (int col = 0; col < 100; ++col)
            {
                for (int row = 0; row < 100; ++row)
                {
                    strawSize.SetValue((float)r.NextDouble() * 0.1f, col, row);
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
            using (Texture2D texture = Texture2D.FromSwapChain<Texture2D>(Context10.SwapChain, 0))
            {
                renderTargetView = new RenderTargetView(Context10.Device, texture);
            }

            CreateDepthBuffer();
            var dssd = new DepthStencilStateDescription
            {
                IsDepthEnabled = true,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less
            };
            depthStencilState = DepthStencilState.FromDescription(Context10.Device, dssd);

            cube = new SimpleCube(Context10.Device, Asset.File("DefaultCamera.fx"), null);
            plane = new SimplePlane(Context10.Device, Asset.File("DefaultCamera.fx"), null);
            straw = new SimpleGrass(Context10.Device, Asset.File("DefaultCamera.fx"), null);
            //Geometry Shader Test
            {
                root = new SimpleRoot(Context10.Device, Asset.Dir("shader").file("GeometryShaderExperiment.fx"), null);
            }
        }

        protected override void OnResourceUnload()
        {
            renderTargetView.Dispose();
            depthStencilView.Dispose();
            depthStencilState.Dispose();
        }
        
        protected void processInput()
        {
            KeyboardState keyState = m_input.ReadKeyboard();
            MouseState mouseState = m_input.ReadMouse();
            if (keyState != null)
            {
                foreach (Key key in keyState.ReleasedKeys)
                {
                    switch (key)
                    {
                        case (Key.LeftShift):
                            if (m_camera.isSlowMoving == true)
                            {
                                m_movementSpeed += 50.0f;
                            }
                            m_camera.isSlowMoving = false;
                            break;
                    }
                }

                foreach (Key key in keyState.PressedKeys)
                {
                    switch (key)
                    {
                        case (Key.P):
                            m_soundManager.playSingle("assets/music/10 ft. Ganja Plant - Set Me Free.wav");
                            break;
                        case (Key.W):
                            m_camera.AddToCamera(0f, 0f, FrameDelta * m_movementSpeed, out m_proj, out m_view);
                            break;
                        case (Key.S):
                            m_camera.AddToCamera(0f, 0f, -FrameDelta * m_movementSpeed, out m_proj, out m_view);
                            break;
                        case (Key.A):
                            m_camera.AddToCamera(FrameDelta * m_movementSpeed, 0f, 0f, out m_proj, out m_view);
                            break;
                        case (Key.D):
                            m_camera.AddToCamera(-FrameDelta * m_movementSpeed, 0f, 0f, out m_proj, out m_view);
                            break;
                        case (Key.Space):
                            m_camera.AddToCamera(0f, FrameDelta * m_movementSpeed, 0f, out m_proj, out m_view);
                            break;
                        case (Key.LeftControl):
                            break;
                        case (Key.C):
                            m_camera.AddToCamera(0f, -FrameDelta * m_movementSpeed, 0f, out m_proj, out m_view);
                            break;
                        case (Key.LeftShift):
                            if (m_camera.isSlowMoving == false)
                            {
                                m_movementSpeed -= 50.0f;
                            }
                            m_camera.isSlowMoving = true;
                            break;
                        case (Key.Escape):
                            m_isFormClosed = true;
                            Quit();
                            break;
                        case (Key.LeftAlt | Key.Return):
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
            Context10.Device.OutputMerger.DepthStencilState = depthStencilState;
            Context10.Device.OutputMerger.SetTargets(depthStencilView, renderTargetView);

            Context10.Device.Rasterizer.SetViewports(new Viewport(0, 0, WindowWidth, WindowHeight, 0.0f, 1.0f));
            Context10.Device.ClearRenderTargetView(renderTargetView, new Color4(0.3f, 0.3f, 0.3f));
            Context10.Device.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

            processInput();

            m_camera.Update(out m_proj, out m_view);
        }

        protected override void OnRender()
        {
            //For anything with time use 'FrameDelta', don't use anything else
            double a = clock.Check();
            output.Value = 5*(float)System.Math.Sin(a);
            //m_camera.Location = new Vector3(5 * (float)System.Math.Sin(a), 3 * (float)System.Math.Sin(a) + 4, 5 * (float)System.Math.Cos(a)); //Orbit around the target.

            Matrix world = Matrix.Identity;
            cube.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
            cube.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
            cube.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
            cube.Draw();

            Matrix.Translation(2.0f, 0.0f, 0.0f, out world);
            cube.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
            cube.Draw();

            world = Matrix.Identity;
            Matrix.Translation(0.0f, -0.5f, 0.0f, out world);
            Matrix tempMatrix;
            Matrix.Scaling(5.0f, 5.0f, 5.0f, out tempMatrix);
            Matrix.Multiply(ref tempMatrix, ref world, out world);
            plane.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
            plane.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
            plane.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
            plane.Draw();

            for (int col = -50; col < 0; ++col)
            {
                for (int row = -50; row < 0; ++row)
                {
                    world = Matrix.Identity;
                    float randomHight = (float)strawSize.GetValue(col+50, row+50);
                    //Matrix.Scaling(0.01f, 0.1f+(float)strawSize.GetValue(col+50, row+50), 0.01f, out world);
                    //Matrix.Translation(row*10, -4.0f, col*10, out tempMatrix);
                    Matrix.Scaling(0.01f, 0.01f + randomHight, 0.01f, out tempMatrix);
                    Matrix.Translation(row * 0.1f, -0.5f, col * 0.1f, out world);
                    Matrix rotationTemp;
                    Matrix.RotationY(col + row + randomHight, out rotationTemp);
                    Matrix.Multiply(ref rotationTemp, ref world, out world);
                    Matrix.Multiply(ref tempMatrix, ref world, out world);
                    straw.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                    straw.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                    straw.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                    straw.Draw();
                }
            }

            // Geometry Shader Test
            for (int col = 0; col < 50; ++col)
            {
                for (int row = 0; row < 50; ++row)
                {
                    /*
                     *  Alle Wurzeln werden mit einer zufälligen Höhe und Ausrichtung erstellt.
                     */
                    world = Matrix.Identity;
                    float randomHight = (float)strawSize.GetValue(col, row) * 0.3f;
                    Matrix.Scaling(0.01f, 0.01f + randomHight, 0.01f, out tempMatrix);
                    Matrix.Translation(row * 0.1f, -0.5f, col * 0.1f, out world);
                    Matrix rotationTemp;
                    Matrix.RotationY(col + row + randomHight, out rotationTemp);
                    Matrix.Multiply(ref rotationTemp, ref tempMatrix, out tempMatrix);
                    Matrix.Multiply(ref tempMatrix, ref world, out world);

                    root.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                    root.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                    root.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                    root.Draw();
                }
            }//*/
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

            using (var depthBuffer = new Texture2D(Context10.Device, depthBufferDesc))
            {
                depthStencilView = new DepthStencilView(Context10.Device, depthBuffer);
            }
        }
    }
}
