using SlimDX.Direct3D9;

namespace Uncut.Rendering
{
    /// <summary>
    /// Settings used to initialize a Direct2D context.
    /// </summary>
    public class DeviceSettings2D
    {
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
