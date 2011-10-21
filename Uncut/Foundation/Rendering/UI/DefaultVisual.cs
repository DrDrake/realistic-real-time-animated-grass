using System.Windows.Forms;
using System.Drawing;

using SlimDX;
using Uncut.UI;

namespace Uncut.Rendering.UI
{
    /// <summary>
    /// Provides a default visual representation of an element, to be used
    /// when no element-specific visual representation is available.
    /// </summary>
    public class DefaultVisual : IElementVisual
    {
        #region Public Interface

        /// <summary>
        /// Measures the element, returning the size (in pixels) it would occupy if
        /// rendered with the specified renderer.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        /// <param name="element">The element.</param>
        /// <returns>The size of the element (in pixels).</returns>
        public virtual Vector2 Measure(UserInterfaceRenderer renderer, Element element)
        {
            return renderer.MeasureString(element.Label);
        }

        /// <summary>
        /// Renders the element using the specified renderer.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        /// <param name="element">The element.</param>
        /// <param name="x">The X coordinate (in pixels) of the upper left corner of the region the element should be rendered to.</param>
        /// <param name="y">The Y coordinate (in pixels) of the upper left corner of the region the element should be rendered to.</param>
        /// <param name="width">The width (in pixels) of the region the element should be rendered to.</param>
        /// <param name="height">The height (in pixels) of the region the element should be rendered to.</param>
        public virtual void Render(UserInterfaceRenderer renderer, Element element, int x, int y, int width, int height)
        {
            Color4 color = new Color4(1.0f, 1.0f, 1.0f);
            renderer.RenderRectangle(x, y, width, height, color);
            renderer.RenderString(element.Label, 0, 0, new Color4(1.0f, 0.0f, 0.0f));
        }

        #endregion
    }
}
