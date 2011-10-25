using System;
using SlimDX.DXGI;
using SlimDX;

namespace Uncut.Rendering
{
    /// <summary>
    /// Provides creation and management functionality for a Direct3D10 rendering device and related objects.
    /// </summary>
    public class DeviceContext10 : IDisposable
    {
        #region Public Interface
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceContext10"/> class.
        /// </summary>
        /// <param name="handle">The window handle to associate with the device.</param>
        /// <param name="settings">The settings used to configure the device.</param>
        internal DeviceContext10(IntPtr handle, DeviceSettings10 settings)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Value must be a valid window handle.", "handle");
            if (settings == null)
                throw new ArgumentNullException("settings");

            this.settings = settings;

            factory = new Factory();
            device = new SlimDX.Direct3D10.Device(factory.GetAdapter(settings.AdapterOrdinal), SlimDX.Direct3D10.DriverType.Hardware, settings.CreationFlags);

            swapChain = new SwapChain(factory, device, new SwapChainDescription
            {
                BufferCount = 1,
                Flags = SwapChainFlags.None,
                IsWindowed = true,
                ModeDescription = new ModeDescription(settings.Width, settings.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                OutputHandle = handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            });

            factory.SetWindowAssociation(handle, WindowAssociationFlags.IgnoreAll | WindowAssociationFlags.IgnoreAltEnter);
        }

        /// <summary>
        /// Performs object finalization.
        /// </summary>
        ~DeviceContext10()
        {
            Dispose(false);
        }

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
                if (swapChain.IsFullScreen)
                    swapChain.IsFullScreen = false;

                swapChain.Dispose();
                device.Dispose();
                factory.Dispose();
            }
        }

        /// <summary>
        /// Gets the underlying DXGI factory.
        /// </summary>
        public SlimDX.DXGI.Factory Factory
        {
            get
            {
                return factory;
            }
        }

        /// <summary>
        /// Gets the underlying Direct3D10 device.
        /// </summary>
        public SlimDX.Direct3D10.Device Device
        {
            get
            {
                return device;
            }
        }

        /// <summary>
        /// Gets the underlying DXGI swap chain.
        /// </summary>
        public SlimDX.DXGI.SwapChain SwapChain
        {
            get
            {
                return swapChain;
            }
        }

        #endregion
        #region Implementation Detail

        DeviceSettings10 settings;

        SlimDX.DXGI.Factory factory;
        SlimDX.Direct3D10.Device device;
        SlimDX.DXGI.SwapChain swapChain;

        #endregion
    }
}
