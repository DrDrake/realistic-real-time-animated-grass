using SlimDX.Direct3D10;

namespace Uncut.Rendering
{
    /// <summary>
    /// Settings used to initialize a Direct3D10 device.
    /// </summary>
    public class DeviceSettings10
    {
        /// <summary>
        /// Gets or sets the adapter ordinal.
        /// </summary>
        public int AdapterOrdinal
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the creation flags.
        /// </summary>
        public DeviceCreationFlags CreationFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the width of the renderable area.
        /// </summary>
        public int Width
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the height of the renderable area.
        /// </summary>
        public int Height
        {
            get;
            set;
        }
    }
}