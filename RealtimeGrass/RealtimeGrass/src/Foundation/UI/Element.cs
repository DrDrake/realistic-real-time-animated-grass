using System.Collections.Generic;

using RealtimeGrass.UI.Binding;

namespace RealtimeGrass.UI
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
            bindings[targetName] = new RealtimeGrass.UI.Binding.Binding(targetName, source);
        }

        public void Update()
        {
            foreach (RealtimeGrass.UI.Binding.Binding binding in bindings.Values)
            {
                binding.Update(this);
            }
        }

        #endregion
        #region Implementation Detail

        Dictionary<string, RealtimeGrass.UI.Binding.Binding> bindings = new Dictionary<string, RealtimeGrass.UI.Binding.Binding>();

        #endregion
    }
}
