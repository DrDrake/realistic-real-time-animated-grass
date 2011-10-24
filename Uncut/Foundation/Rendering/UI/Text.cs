using SlimDX;

namespace Uncut.Rendering.UI
{
    /// <summary>
    /// Defines the interface required to specify an element's visual representation.
    /// </summary>
    public class Text
    {
        #region Public Interface

        public int X
        {
            get;
            set;
        }

        public int Y
        {
            get;
            set;
        }

        public string String
        {
            get;
            set;
        }

        public Color4 Color
        {
            get;
            set;
        }

        public Text(int x, int y, string text, Color4 color)
        {
            X = x;
            Y = y;
            String = text;
            Color = color;
        }

        #endregion
    }
}