using System;
using System.Collections.Generic;
using System.Text;

using SlimDX;

namespace RealtimeGrass.Utility
{
    public struct SVertex3P3N2T
    {
        private readonly Vector3 m_position;
        private readonly Vector3 m_normal;
        private readonly Vector2 m_texCoord;

        public SVertex3P3N2T(
            Vector3 position,
            Vector3 normal,
            Vector2 texCoord
        )
        {
            m_position = position;
            m_normal = normal;
            m_texCoord = texCoord;
        }

        public Vector3 Position { get { return m_position; } private set { } }
        public Vector3 Normal { get { return m_normal; } private set { } }
        public Vector2 TexCoord { get { return m_texCoord; } private set { } }
    }
}
