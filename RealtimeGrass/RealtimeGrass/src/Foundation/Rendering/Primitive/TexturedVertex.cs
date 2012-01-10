using System;
using System.Globalization;
using System.Runtime.InteropServices;

using SlimDX;

namespace RealtimeGrass.Rendering.Primitive
{
    /// <summary>
    /// Represents a vertex with a position and a texture coordinate.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TexturedVertex : IEquatable<TexturedVertex>
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
        /// Gets or sets the texture coordinate for the vertex.
        /// </summary>
        public Vector2 TextureCoordinate
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturedVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="color">The color.</param>
        public TexturedVertex(Vector3 position, Vector2 textureCoordinate)
            : this()
        {
            Position = position;
            TextureCoordinate = textureCoordinate;
        }

        /// <summary>
        /// Implements operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(TexturedVertex left, TexturedVertex right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements operator !=.
        /// </summary>
        /// <param name="left">The left side of the operator.</param>
        /// <param name="right">The right side of the operator.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(TexturedVertex left, TexturedVertex right)
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
            return Position.GetHashCode() + TextureCoordinate.GetHashCode();
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

            return Equals((TexturedVertex)obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(TexturedVertex other)
        {
            return (Position == other.Position && TextureCoordinate == other.TextureCoordinate);
        }
    }
}
