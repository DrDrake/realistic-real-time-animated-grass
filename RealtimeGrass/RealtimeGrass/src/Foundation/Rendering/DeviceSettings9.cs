using SlimDX.Direct3D9;

namespace RealtimeGrass.Rendering
{
    /// <summary>
    /// Settings used to initialize a Direct3D9 device.
    /// </summary>
    public class DeviceSettings9
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
        public CreateFlags CreationFlags
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
