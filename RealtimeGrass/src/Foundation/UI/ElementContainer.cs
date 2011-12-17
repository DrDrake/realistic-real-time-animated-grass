using System;
using System.Collections.Generic;
using System.Drawing;
using System.Collections;

namespace RealtimeGrass.UI
{
    /// <summary>
    /// A container for UI elements.
    /// </summary>
    public class ElementContainer : IEnumerable<Element>
    {
        #region Public Interface

        /// <summary>
        /// Adds the specified element to the container.
        /// </summary>
        /// <param name="element">The element.</param>
        public void Add(Element element)
        {
            elements.Add(element);
        }

        public void Update()
        {
            foreach (Element element in elements)
            {
                element.Update();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<Element> IEnumerable<Element>.GetEnumerator()
        {
            foreach (Element element in elements)
                yield return element;
        }

        #endregion
        #region Implementation Detail

        List<Element> elements = new List<Element>();

        #endregion
    }
}
