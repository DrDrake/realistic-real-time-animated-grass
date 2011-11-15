using System.Collections.Generic;
using SlimDX;

namespace Uncut.Rendering.Mesh
{
    /// <summary>
    /// A container for mesh data.
    /// </summary>
    public class MeshData
    {
        /// <summary>
        /// Gets position data.
        /// </summary>
        public List<Vector3> Positions
        {
            get
            {
                return positions;
            }
        }

        /// <summary>
        /// Gets normal data.
        /// </summary>
        public List<Vector3> Normals
        {
            get
            {
                return normals;
            }
        }

        /// <summary>
        /// Gets texture coordinate data.
        /// </summary>
        public List<Vector2> TextureCoordinates
        {
            get
            {
                return textureCoordinates;
            }
        }

        /// <summary>
        /// Gets mesh index data.
        /// </summary>
        public List<int> Indices
        {
            get
            {
                return indices;
            }
        }

        List<Vector3> positions = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> textureCoordinates = new List<Vector2>();
        List<int> indices = new List<int>();
    }
}
