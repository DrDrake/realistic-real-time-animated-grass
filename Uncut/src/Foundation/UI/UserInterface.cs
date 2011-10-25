
namespace Uncut.UI
{
    /// <summary>
    /// Encapsulates logical user interface state.
    /// </summary>
    public class UserInterface
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserInterface"/> class.
        /// </summary>
        public UserInterface()
        {
            Container = new ElementContainer();
        }

        /// <summary>
        /// Gets or sets the interface's element container.
        /// </summary>
        public ElementContainer Container
        {
            get;
            set;
        }
    }
}
