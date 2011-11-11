using System.Collections.Generic;

using SlimDX;

using RealtimeGrass.Utility;

namespace RealtimeGrass.Rendering.Mesh
{
    /// <summary>
    /// A container for mesh data.
    /// </summary>
    public class MeshData
    {
        List<int>               m_indices = new List<int>();
        List<SVertex3P3N2T>     m_vertices = new List<SVertex3P3N2T>();

        /// <summary>
        /// Gets position data.
        /// </summary>
        public List<SVertex3P3N2T> Vertices
        {
            get
            {
                return m_vertices;
            }
        }

        /// <summary>
        /// Gets mesh index data.
        /// </summary>
        public List<int> Indices
        {
            get
            {
                return m_indices;
            }
        }
    }
}
