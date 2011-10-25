using System.Drawing;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.D3DCompiler;
using Uncut.Rendering;
using Uncut.Utility;
using Uncut.UI.Binding;
using Uncut.UI;
using SlimDX.DXGI;

namespace Uncut
{
    /// <summary>
    /// Demonstrates how to render a simple colored triangle with Direct3D10.
    /// </summary>
    class TestViewController : SlimSceneController
    {
        /// <summary>
        /// In a derived class, implements logic to initialize the sample.
        /// </summary>
        protected override void OnInitialize()
        {
            var hudText = new Element();
            hudText.SetBinding("Label", output);
            UserInterface.Container.Add(hudText);

            camera = new Camera();
            camera.FieldOfView = (float)System.Math.PI / 2;
            camera.AspectRatio = WindowWidth / WindowHeight;
            camera.NearPlane = 0.1f;
            camera.FarPlane = 110f;
            camera.Location = new Vector3(0.0f, 0.0f, -5.0f);
            camera.Target = new Vector3(0.0f, 0.0f, 0.0f);
            camera.Up = new Vector3(0.0f, 1.0f, 0.0f);

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
        }

        protected override void OnResourceUnload()
        {
            renderTargetView.Dispose();
            depthStencilView.Dispose();
            depthStencilState.Dispose();
        }

        protected override void OnRenderBegin()
        {
            Context10.Device.OutputMerger.DepthStencilState = depthStencilState;
            Context10.Device.OutputMerger.SetTargets(depthStencilView, renderTargetView);

            Context10.Device.Rasterizer.SetViewports(new Viewport(0, 0, WindowWidth, WindowHeight, 0.0f, 1.0f));
            Context10.Device.ClearRenderTargetView(renderTargetView, new Color4(0.3f, 0.3f, 0.3f));
            Context10.Device.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        protected override void OnRender()
        {
            double a = clock.Check();
            output.Value = 5*(float)System.Math.Sin(a);
            camera.Location = new Vector3(5 * (float)System.Math.Sin(a), 3 * (float)System.Math.Sin(a) + 4, 5 * (float)System.Math.Cos(a)); //Orbit around the target.
            //camera.Location = new Vector3(0.0f, 3.0f, 4.0f);
            //camera.MoveLeft(0.8f);

            Matrix world = Matrix.Identity;
            cube.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
            cube.Effect.GetVariableByName("view").AsMatrix().SetMatrix(camera.ViewMatrix);
            cube.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(camera.ProjectionMatrix);
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
            plane.Effect.GetVariableByName("view").AsMatrix().SetMatrix(camera.ViewMatrix);
            plane.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(camera.ProjectionMatrix);
            plane.Draw();

            for (int col = -50; col < 50; ++col)
            {
                for (int row = -50; row < 50; ++row)
                {
                    world = Matrix.Identity;
                    Matrix.Scaling(0.01f, 0.1f+(float)strawSize.GetValue(col+50, row+50), 0.01f, out world);
                    Matrix.Translation(row*10, -5.0f, col*10, out tempMatrix);
                    Matrix temp2;
                    Matrix.RotationY(col+row, out temp2);
                    Matrix.Multiply(ref tempMatrix, ref world, out world);
                    Matrix.Multiply(ref temp2, ref world, out world);
                    straw.Effect.GetVariableByName("world").AsMatrix().SetMatrix(world);
                    straw.Effect.GetVariableByName("view").AsMatrix().SetMatrix(camera.ViewMatrix);
                    straw.Effect.GetVariableByName("proj").AsMatrix().SetMatrix(camera.ProjectionMatrix);
                    straw.Draw();
                }
            }
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

        #region Implementation Detail

        private RenderTargetView renderTargetView;
        private DepthStencilState depthStencilState;
        private DepthStencilView depthStencilView;
        private Camera camera;
        private Clock clock;
        private readonly Bindable<float> output = new Bindable<float>();
        private SimpleCube cube;
        private SimplePlane plane;
        private SimpleGrass straw;
        private float[,] strawSize;

        #endregion
    }
}
