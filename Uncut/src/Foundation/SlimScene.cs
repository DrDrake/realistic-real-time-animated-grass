using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

using SlimDX.Direct3D9;
using SlimDX.DXGI;
using SlimDX.Windows;

using Uncut.Utility;
using Uncut.UI;
using Uncut.Rendering;
using Uncut.Rendering.UI;
using Uncut.UI.Binding;

namespace Uncut
{
    /// <summary>
    /// Implements core application logic of a SlimDX sample.
    /// 
    /// The Sample class provides a minimal wrapper around window setup, user
    /// interaction, and OS-level details, but provides very little abstraction
    /// of the underlying DirectX functionality. The reason for this is that the 
    /// purpose of a SlimDX sample is to illustrate how a particular technique 
    /// might be implemented using SlimDX; providing high level rendering abstractions
    /// in the sample framework simplify obfuscates that.
    /// 
    /// A sample is implemented by overriding various base class methods (those prefixed
    /// with "on"). 
    /// </summary>
    public class SlimScene : IDisposable
    {
        #region Public Interface

        public bool m_isFormClosed { get; set; }

        /// <summary>
        /// Gets the width of the renderable area of the sample window.
        /// </summary>
        public int WindowWidth
        {
            get { return m_configuration.WindowWidth; }
        }

        /// <summary>
        /// Gets the number of seconds passed since the last frame.
        /// </summary>
        public float FrameDelta { get; private set; }

        /// <summary>
        /// Gets the height of the renderable area of the sample window.
        /// </summary>
        public int WindowHeight
        {
            get { return m_configuration.WindowHeight; }
        }

        public UserInterface UserInterface
        {
            get { return userInterface; }
        }

        /// <summary>
        /// Represents a Direct3D9 Context, only valid after calling InitializeDevice(DeviceSettings9)
        /// </summary>
        public DeviceContext9 Context9 { get; private set; }

        /// <summary>
        /// Represents a Direct3D10 Context, only valid after calling InitializeDevice(DeviceSettings10)
        /// </summary>
        public DeviceContext10 Context10 { get; private set; }

        /// <summary>
        /// Represents a Direct2D Context, only valid after calling InitializeDevice(DeviceSettings2D)
        /// </summary>
        public DeviceContext2D Context2D { get; private set; }

        /// <summary>
        /// Disposes of object resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of object resources.
        /// </summary>
        /// <param name="disposeManagedResources">If true, managed resources should be
        /// disposed of in addition to unmanaged resources.</param>
        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                if (userInterfaceRenderer != null)
                {
                    userInterfaceRenderer.Dispose();
                }

                apiContext.Dispose();
                m_form.Dispose();
            }
        }

        protected virtual Form CreateForm(Configuration config)
        {
            return new RenderForm(config.WindowTitle)
            {
                ClientSize = new Size(config.WindowWidth, config.WindowHeight)
            };
        }

        /// <summary>
        /// Runs the sample.
        /// </summary>
        public void Run()
        {
            m_configuration = OnConfigure();
            m_form = CreateForm(m_configuration);

            currentFormWindowState = m_form.WindowState;

            m_isFormClosed = false;
            bool formIsResizing = false;

            m_form.MouseClick += HandleMouseClick;
            m_form.KeyDown += HandleKeyDown;
            m_form.KeyUp += HandleKeyUp;
            m_form.Resize += (o, args) =>
            {
                if (m_form.WindowState != currentFormWindowState)
                {
                    HandleResize(o, args);
                }

                currentFormWindowState = m_form.WindowState;
            };

            m_form.ResizeBegin += (o, args) => { formIsResizing = true; };
            m_form.ResizeEnd += (o, args) =>
            {
                formIsResizing = false;
                HandleResize(o, args);
            };

            m_form.Closed += (o, args) => { m_isFormClosed = true; };

            userInterface = new UserInterface();
            var stats = new Element();
            stats.SetBinding("Label", framesPerSecond);
            userInterface.Container.Add(stats);

            OnInitialize();
            OnResourceLoad();

            clock.Start();
            MessagePump.Run(m_form, () =>
            {
                if (m_isFormClosed)
                {
                    return;
                }

                Update();
                if (!formIsResizing)
                    Render();
            });

            OnResourceUnload();
        }

        /// <summary>
        /// In a derived class, implements logic to control the configuration of the sample
        /// via a <see cref="SampleConfiguration"/> object.
        /// </summary>
        /// <returns>A <see cref="SampleConfiguration"/> object describing the desired configuration of the sample.</returns>
        protected virtual Configuration OnConfigure()
        {
            return new Configuration();
        }

        /// <summary>
        /// In a derived class, implements logic to initialize the sample.
        /// </summary>
        protected virtual void OnInitialize() { }

        protected virtual void OnResourceLoad() { }

        protected virtual void OnResourceUnload() { }

        /// <summary>
        /// In a derived class, implements logic to update any relevant sample state.
        /// </summary>
        protected virtual void OnUpdate() { }

        /// <summary>
        /// In a derived class, implements logic to render the sample.
        /// </summary>
        protected virtual void OnRender() { }

        /// <summary>
        /// In a derived class, implements logic that should occur before all
        /// other rendering.
        /// </summary>
        protected virtual void OnRenderBegin() { }

        /// <summary>
        /// In a derived class, implements logic that should occur after all
        /// other rendering.
        /// </summary>
        protected virtual void OnRenderEnd() { }

        /// <summary>
        /// Initializes a <see cref="DeviceContext2D">Direct2D device context</see> according to the specified settings.
        /// The base class retains ownership of the context and will dispose of it when appropriate.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The initialized device context.</returns>
        protected void InitializeDevice(DeviceSettings2D settings)
        {
            var result = new DeviceContext2D(m_form.Handle, settings);
            //userInterfaceRenderer = new UserInterfaceRenderer9( result.Device, settings.Width, settings.Height );
            apiContext = result;
            Context2D = result;
        }

        /// <summary>
        /// Initializes a <see cref="DeviceContext9">Direct3D9 device context</see> according to the specified settings.
        /// The base class retains ownership of the context and will dispose of it when appropriate.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The initialized device context.</returns>
        protected void InitializeDevice(DeviceSettings9 settings)
        {
            var result = new DeviceContext9(m_form.Handle, settings);
            userInterfaceRenderer = new UserInterfaceRenderer9(result.Device, settings.Width, settings.Height);
            apiContext = result;
            Context9 = result;
        }

        /// <summary>
        /// Initializes a <see cref="DeviceContext10">Direct3D10 device context</see> according to the specified settings.
        /// The base class retains ownership of the context and will dispose of it when appropriate.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The initialized device context.</returns>
        protected void InitializeDevice(DeviceSettings10 settings)
        {
            var result = new DeviceContext10(m_form.Handle, settings);
            userInterfaceRenderer = new UserInterfaceRenderer10(result.Device, settings.Width, settings.Height);
            apiContext = result;
            Context10 = result;
        }

        /// <summary>
        /// Quits the sample.
        /// </summary>
        protected void Quit()
        {
            m_form.Close();
        }

        #endregion

        #region Implementation Detail

        private readonly Clock clock = new Clock();
        private readonly Bindable<float> framesPerSecond = new Bindable<float>();
        private IDisposable apiContext;
        protected Configuration m_configuration;
        private FormWindowState currentFormWindowState;
        protected Form m_form;
        private float frameAccumulator;
        private int frameCount;
        private bool deviceLost = false;
        private UserInterface userInterface;
        protected UserInterfaceRenderer userInterfaceRenderer;

        /// <summary>
        /// Performs object finalization.
        /// </summary>
        ~SlimScene()
        {
            Dispose(false);
        }

        /// <summary>
        /// Updates sample state.
        /// </summary>
        private void Update()
        {
            FrameDelta = clock.Update();
            userInterface.Container.Update();
            OnUpdate();
        }

        /// <summary>
        /// Renders the sample.
        /// </summary>
        private void Render()
        {
            if (deviceLost)
            {
                // This should only become true if we're using D3D9, so we can assume the
                // D3D9 context is valid at this point.
                if (Context9.Device.TestCooperativeLevel() == SlimDX.Direct3D9.ResultCode.DeviceNotReset)
                {
                    Context9.Device.Reset(Context9.PresentParameters);
                    deviceLost = false;
                    userInterfaceRenderer = new UserInterfaceRenderer9(Context9.Device, WindowWidth, WindowHeight);
                    OnResourceLoad();
                }
                else
                {
                    Thread.Sleep(100);
                    return;
                }
            }

            frameAccumulator += FrameDelta;
            ++frameCount;
            if (frameAccumulator >= 1.0f)
            {
                framesPerSecond.Value = frameCount / frameAccumulator;

                frameAccumulator = 0.0f;
                frameCount = 0;
            }

            try
            {
                OnRenderBegin();
                OnRender();
                if (userInterfaceRenderer != null)
                {
                    userInterfaceRenderer.Render(userInterface);
                }
                OnRenderEnd();
            }
            catch (SlimDX.Direct3D9.Direct3D9Exception e)
            {
                if (e.ResultCode == SlimDX.Direct3D9.ResultCode.DeviceLost)
                {
                    OnResourceUnload();
                    userInterfaceRenderer.Dispose();
                    deviceLost = true;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Handles a mouse click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void HandleMouseClick(object sender, MouseEventArgs e) { }

        /// <summary>
        /// Handles a key down event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        private void HandleKeyDown(object sender, KeyEventArgs e) { }

        /// <summary>
        /// Handles a key up event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.Enter)
            {
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
            }
        }

        protected bool isFullScreen = false;
        private void HandleResize(object sender, EventArgs e)
        {
            if (m_form.WindowState == FormWindowState.Minimized)
            {
                return;
            }

            OnResourceUnload();

            if (Context9 != null)
            {
                userInterfaceRenderer.Dispose();

                Context9.PresentParameters.BackBufferWidth = 0;
                Context9.PresentParameters.BackBufferHeight = 0;

                Context9.Device.Reset(Context9.PresentParameters);

                userInterfaceRenderer = new UserInterfaceRenderer9(Context9.Device, m_form.ClientSize.Width, m_form.ClientSize.Height);
            }
            else if (Context10 != null)
            {
                userInterfaceRenderer.Dispose();

                Context10.SwapChain.ResizeBuffers(1, WindowWidth, WindowHeight, Context10.SwapChain.Description.ModeDescription.Format, Context10.SwapChain.Description.Flags);


                userInterfaceRenderer = new UserInterfaceRenderer10(Context10.Device, m_form.ClientSize.Width, m_form.ClientSize.Height);
            }

            OnResourceLoad();
        }

        #endregion
    }
}