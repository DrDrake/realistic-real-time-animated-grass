/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using SlimDX.SampleFramework;
using SlimDX.DirectInput;
using Resource = SlimDX.Direct3D10.Resource;
using System.Windows.Forms;

namespace Gras.src 
{
    internal class GrasSample : Sample 
    {
        #region Members

        ///
        /// Disposable resources
        ///
        private DepthStencilState depthStencilState;
        private DepthStencilView depthStencilView;
        private SimpleModel jupiterMesh;
        private BlendState solidBlendState;
        private BlendState transBlendState;
        private RenderTargetView renderTargetView;
        private CCamera m_camera;
        /// 
        /// Non-Disposable resources
        ///
        private Matrix m_proj;
        private Matrix m_view;
        private float m_rotation;
        
        private CInputController m_input;

        #endregion

        #region Methods
        
        /// In a derived class, implements logic to control the configuration of the sample
        override protected SampleConfiguration OnConfigure()
        {
            return new SampleConfiguration("Countless Blades of Grass", 1000, 800);
        }

        /// In a derived class, implements logic to initialize the sample.
        protected override void OnInitialize() 
        {
            var settings10 = new DeviceSettings10 
            {
                AdapterOrdinal = 0,
                CreationFlags = DeviceCreationFlags.None,
                Width = WindowWidth,
                Height = WindowHeight
            };

            InitializeDevice( settings10 );

        }
        /// In a derived class, implements logic to initialize the sample.
        protected override void OnResourceLoad() 
        {
            //Create Buffer for Rendering onto
            CreatePrimaryRenderTarget();
            CreateDepthBuffer();

            var dssd = new DepthStencilStateDescription 
            {
                IsDepthEnabled = true,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less
            };

            var solidParentOp = new BlendStateDescription();
            solidParentOp.SetBlendEnable( 0, false );
            solidParentOp.SetWriteMask( 0, ColorWriteMaskFlags.All );

            var transParentOp = new BlendStateDescription 
            {
                AlphaBlendOperation = BlendOperation.Add,
                BlendOperation = BlendOperation.Add,
                DestinationAlphaBlend = BlendOption.Zero,
                DestinationBlend = BlendOption.One,
                IsAlphaToCoverageEnabled = false,
                SourceAlphaBlend = BlendOption.Zero,
                SourceBlend = BlendOption.One,
            };

            transParentOp.SetBlendEnable( 0, true );
            transParentOp.SetWriteMask( 0, ColorWriteMaskFlags.All );

            transBlendState = BlendState.FromDescription( Context10.Device, transParentOp );
            solidBlendState = BlendState.FromDescription( Context10.Device, solidParentOp );

            depthStencilState = DepthStencilState.FromDescription( Context10.Device, dssd );

            //Load Mesh
            jupiterMesh = new SimpleModel(
                Context10.Device, 
                "assets/shader/ModelTextured.fx", 
                "assets/mesh/jupiter.SMD", 
                "assets/texture/jupiter.jpg"
            );

            m_input = new CInputController(m_form);

            //Camera
            m_camera = new CCamera(
                new Vector3(0, 0, 300),
                new Vector3(0, 0, 0),
                Vector3.UnitZ,
                Vector3.UnitY,
                0.001f,
                1f,
                10.0f,
                1.0f,
                1000.0f,
                45.0f,
                WindowWidth / WindowHeight
            );

            //Make view-matrix and connect them to the shader
            //view = Matrix.LookAtLH( new Vector3( 100, 220, 0 ), new Vector3( 100, 128.0f, 0 ), Vector3.UnitZ );
            m_camera.Update(out m_proj, out m_view);
            //m_camera.UpdateView(out m_view);
            jupiterMesh.Effect.GetVariableByName( "view" ).AsMatrix().SetMatrix( m_view );

            //Camera
            //Make projection-matrix and connect them to the shader
            //proj = Matrix.PerspectiveFovLH( 45.0f, (float) WindowWidth / (float)WindowHeight, 1.0f, 1000.0f );
           // m_camera.UpdateProj(out m_proj, WindowWidth, WindowHeight);
            jupiterMesh.Effect.GetVariableByName( "proj" ).AsMatrix().SetMatrix( m_proj );
        }

        protected override void OnResourceUnload() 
        {
            Context10.Device.ClearState();
            renderTargetView.Dispose();
            depthStencilView.Dispose();
            
            jupiterMesh.Dispose();
            depthStencilState.Dispose();
            solidBlendState.Dispose();
            transBlendState.Dispose();
        }

        protected void processInput()
        {
            KeyboardState keyState = m_input.ReadKeyboard();
            MouseState mouseState = m_input.ReadMouse();

            if(keyState != null)
            {
                float Move = 64.0f;

                foreach (Key key in keyState.PressedKeys)
                {
                    switch (key)
                    {
                        case (Key.W):
                            m_camera.AddToCamera(0f, 0f, FrameDelta * Move, out m_proj, out m_view);
                            break;
                        case (Key.S):
                            m_camera.AddToCamera(0f, 0f, -FrameDelta * Move, out m_proj, out m_view);
                            break;
                        case (Key.A):
                            m_camera.AddToCamera(FrameDelta * Move, 0f, 0f, out m_proj, out m_view);
                            break;
                        case (Key.D):
                            m_camera.AddToCamera(-FrameDelta * Move, 0f, 0f, out m_proj, out m_view);
                            break;
                        case (Key.Space):
                            m_camera.AddToCamera(0f, FrameDelta * Move, 0f, out m_proj, out m_view);
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
                /*if (mouseState.Y > 0)
                {
                    m_camera.moveCamera(mouseState.Y, EMoveType.MOVE_LOOK_UP);
                }
                else
                {
                    m_camera.moveCamera(mouseState.Y, EMoveType.MOVE_LOOK_DOWN);
                }

                m_camera.moveCamera(mouseState.X, EMoveType.MOVE_LOOK_SIDE);//*/

                m_camera.RotateAroundPosition(FrameDelta * mouseState.X, -FrameDelta * mouseState.Y, out m_proj, out m_view);
            }
        }

        /// In a derived class, implements logic that should occur before all
        /// other rendering.
        protected override void OnRenderBegin() 
        {
            Context10.Device.OutputMerger.DepthStencilState = depthStencilState;
            Context10.Device.OutputMerger.SetTargets( depthStencilView, renderTargetView );

            Context10.Device.Rasterizer.SetViewports( new Viewport( 0, 0, WindowWidth, WindowHeight, 0.0f, 1.0f ) );
            Context10.Device.ClearRenderTargetView( renderTargetView, new Color4( 0.0f, 0.0f, 0.0f ) );
            Context10.Device.ClearDepthStencilView( depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0 );

            //Get mouse and keyboard input
            processInput();
            //Update view and projection matrix
            m_camera.Update(out m_proj, out m_view);

            jupiterMesh.Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
        }
        /// In a derived class, implements logic to render the sample.
        /// Make Rotation and Translation
        protected override void OnRender() 
        {
            var world = Matrix.Identity;
            //TODO Class Member
            
            m_rotation += (float)( Math.PI / 4.0f * FrameDelta );

            if( m_rotation > 2 * Math.PI )
                m_rotation = 0;

            Matrix rotationMatrix;
            Matrix.RotationX(80, out rotationMatrix);
            Matrix.Multiply(ref world, ref rotationMatrix, out world);

            /// First we'll setup the primary planet that we want to render. This is the big jupiter planet.
            /// We're not doing anything overly fancy here, just setting it up to rotate on its axis at a fixed rate.
            Matrix.RotationY( m_rotation, out rotationMatrix );
            Matrix.Multiply( ref world, ref rotationMatrix, out world );//*/

            jupiterMesh.Effect.GetVariableByName("camPosWS").AsVector().Set(new Vector4(m_camera.m_Position, 1.0f));
            jupiterMesh.Effect.GetVariableByName("world").AsMatrix().SetMatrix( world );
            Context10.Device.OutputMerger.BlendState = solidBlendState;
            //Draw big jupiter
            jupiterMesh.Draw();

            /// At this point we want to render a small moon that will orbit the planet (and rotate as well). 
            /// Again, we're not doing anything overly fancy, however we will set the transparency blend state and then
            /// set the transparency of the pixels (a fixed amount).
            /// 
            Matrix.Translation(0, 0, 100, out world);
            Matrix tempMatrix;
            Matrix.Scaling( .125f, .125f, .125f, out tempMatrix );
            Matrix.Multiply( ref tempMatrix, ref world, out world );
            Matrix.Multiply( ref rotationMatrix, 1.25f, out rotationMatrix );
            Matrix.Multiply( ref rotationMatrix, ref world, out world );
            Matrix orbitMatrix;
            Matrix.RotationZ( -m_rotation * 2f, out orbitMatrix );
            Matrix.Multiply( ref world, ref orbitMatrix, out world );

            //Pass changed parameters to shaders
            jupiterMesh.Effect.GetVariableByName( "world" ).AsMatrix().SetMatrix( world );
            jupiterMesh.Effect.GetVariableByName("camPosWS").AsVector().Set(new Vector4(m_camera.m_Position, 1.0f));
            Context10.Device.OutputMerger.BlendState = solidBlendState;

            //Draw Jupiter as moon
            jupiterMesh.Draw();
        }
        /// In a derived class, implements logic that should occur after all
        /// other rendering.
        protected override void OnRenderEnd() 
        {
            Context10.SwapChain.Present( 0, PresentFlags.None );
        }

        private void CreatePrimaryRenderTarget() 
        {
            using( var swapChainBuffer = Resource.FromSwapChain<Texture2D>( Context10.SwapChain, 0 ) ) 
            {
                renderTargetView = new RenderTargetView( Context10.Device, swapChainBuffer );
            }
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
                SampleDescription = new SampleDescription( 1, 0 ),
                Usage = ResourceUsage.Default
            };

            using( var depthBuffer = new Texture2D( Context10.Device, depthBufferDesc ) ) 
            {
                depthStencilView = new DepthStencilView( Context10.Device, depthBuffer );
            }
        }

        #endregion
    }
}