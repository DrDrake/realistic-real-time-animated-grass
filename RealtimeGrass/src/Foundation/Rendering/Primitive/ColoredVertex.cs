using System;
using System.Globalization;
using System.Runtime.InteropServices;

using SlimDX;

namespace RealtimeGrass.Rendering.Primitive
{
    /// <summary>
    /// Represents a vertex with a position and a color.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ColoredVertex : IEquatable<ColoredVertex>
    {
        /// <summary>
        /// Gets or sets the position of the vertex.
        /// </summary>
        public Vector3 Position
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the color of the vertex.
        /// </summary>
        public int Color
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="color">The color.</param>
        public ColoredVertex(Vector3 position, int color)
            : this()
        {
            Position = position;
            Color = color;
        }

        /// <summary>
        /// Implements operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(ColoredVertex left, ColoredVertex right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements operator !=.
        /// </summary>
        /// <param name="left">The left side of the operator.</param>
        /// <param name="right">The right side of the operator.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(ColoredVertex left, ColoredVertex right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return Position.GetHashCode() + Color.GetHashCode();
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            return Equals((ColoredVertex)obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(ColoredVertex other)
        {
            return (Position == other.Position && Color == other.Color);
        }
    }
}
