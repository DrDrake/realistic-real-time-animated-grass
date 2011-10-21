using System.Collections.Generic;
using Uncut.UI.Binding;

namespace Uncut.UI
{
    /// <summary>
    /// Provides basic logical UI component functionality.
    /// </summary>
    public class Element
    {
        #region Public Interface

        /// <summary>
        /// Gets or sets the element's label.
        /// </summary>
        public string Label
        {
            get;
            set;
        }

        public void SetBinding(string targetName, object source)
        {
            bindings[targetName] = new Uncut.UI.Binding.Binding(targetName, source);
        }

        public void Update()
        {
            foreach (Uncut.UI.Binding.Binding binding in bindings.Values)
            {
                binding.Update(this);
            }
        }

        #endregion
        #region Implementation Detail

        Dictionary<string, Uncut.UI.Binding.Binding> bindings = new Dictionary<string, Uncut.UI.Binding.Binding>();

        #endregion
    }
}
