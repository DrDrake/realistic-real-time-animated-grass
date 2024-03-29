﻿using System;

using SlimDX.Direct3D9;

namespace RealtimeGrass.Rendering
{
    /// <summary>
    /// Provides creation and management functionality for a Direct3D9 rendering device and related objects.
    /// </summary>
    public class DeviceContext9 : IDisposable
    {
        #region Public Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceContext9"/> class.
        /// </summary>
        /// <param name="handle">The window handle to associate with the device.</param>
        /// <param name="settings">The settings used to configure the device.</param>
        internal DeviceContext9(IntPtr handle, DeviceSettings9 settings)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Value must be a valid window handle.", "handle");
            if (settings == null)
                throw new ArgumentNullException("settings");

            this.settings = settings;

            PresentParameters = new PresentParameters();
            PresentParameters.BackBufferFormat = Format.X8R8G8B8;
            PresentParameters.BackBufferCount = 1;
            PresentParameters.BackBufferWidth = settings.Width;
            PresentParameters.BackBufferHeight = settings.Height;
            PresentParameters.Multisample = MultisampleType.None;
            PresentParameters.SwapEffect = SwapEffect.Discard;
            PresentParameters.EnableAutoDepthStencil = true;
            PresentParameters.AutoDepthStencilFormat = Format.D16;
            PresentParameters.PresentFlags = PresentFlags.DiscardDepthStencil;
            PresentParameters.PresentationInterval = PresentInterval.Default;
            PresentParameters.Windowed = true;
            PresentParameters.DeviceWindowHandle = handle;

            direct3D = new Direct3D();
            Device = new Device(direct3D, settings.AdapterOrdinal, DeviceType.Hardware, handle, settings.CreationFlags, PresentParameters);
        }

        /// <summary>
        /// Performs object finalization.
        /// </summary>
        ~DeviceContext9()
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
                Device.Dispose();
                direct3D.Dispose();
            }
        }

        /// <summary>
        /// Gets the underlying Direct3D9 device.
        /// </summary>
        public Device Device
        {
            get;
            private set;
        }

        public PresentParameters PresentParameters
        {
            get;
            private set;
        }

        #endregion
        #region Implementation Detail

        DeviceSettings9 settings;
        Direct3D direct3D;

        #endregion
    }
}
