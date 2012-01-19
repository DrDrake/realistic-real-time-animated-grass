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
            WindowTitle = "Realtime Grass";
            WindowWidth = 1680;
            WindowHeight = 1050;
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
