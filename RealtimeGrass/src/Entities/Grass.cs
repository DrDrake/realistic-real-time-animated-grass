using System;
using System.Collections.Generic;
using System.Text;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using MapFlags = SlimDX.Direct3D10.MapFlags;

namespace RealtimeGrass.Entities
{
    class Grass : Entity
    {
        private Vector3[]       m_roots;

        public Grass(float ambient, float diffuse, float specular, float shininess, Vector3[] roots, int numberOfElements)
            : base(ambient, diffuse, specular, shininess)
        {
            m_roots = roots;
            m_numberOfElements = numberOfElements;
        }

        public override void CreateVertexBuffer()
        {
            m_vertexBuffer = InitVertexBuffer();
            DataStream stream = m_vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<Vector3>(m_roots);
            m_vertexBuffer.Unmap();

        }

        public override InputElement[] InitElementsLayout()
        {
            m_bytesPerElement = 12;

            return new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0) //3 * 4 Byte(float) = 12 Bytes
            };
        }
    }
}
