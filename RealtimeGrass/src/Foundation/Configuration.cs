namespace RealtimeGrass
{
    /// <summary>
    /// Describes the desired application configuration of a <see cref="Sample">SlimDX sample</see>. 
    /// </summary>
    public class Configuration
    {
        #region Public Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleConfiguration"/> class.
        /// </summary>
        public Configuration()
        {
            WindowTitle = "Countless Blades of Grass";
            WindowWidth = 1000;
            WindowHeight = 800;

            GrasRootsCount = 42; // <- Example
        }

        /// <summary>
        /// Just an Example.
        /// </summary>
        public int GrasRootsCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the window title.
        /// </summary>
        public string WindowTitle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the width of the window.
        /// </summary>
        public int WindowWidth
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the height of the window.
        /// </summary>
        public int WindowHeight
        {
            get;
            set;
        }

        #endregion
    }
}
