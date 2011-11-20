using System;
using System.Reflection;
using System.Drawing;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using SlimDX.D3DCompiler;
using Buffer = SlimDX.Direct3D10.Buffer;
using Device = SlimDX.Direct3D10.Device;
using PrimitiveTopology = SlimDX.Direct3D10.PrimitiveTopology;
using MapFlags = SlimDX.Direct3D10.MapFlags;

using RealtimeGrass.Utility;

namespace RealtimeGrass.Entities
{
    class Heightmap : Entity
    {
        private Bitmap m_heightmap;
        private int m_dimension;
        private Vector3[] m_roots;
        public Vector3[] Roots { get { return m_roots; } set { m_roots = value; } }

        public Heightmap(string heightMapName, int dimension)
        {
            m_heightmap = new Bitmap(heightMapName);
            m_dimension = dimension;
        }

        public override void CreateVertexBuffer(){

            Color i;
            float xf = 0;
            float zf = 0;
            float interspace = 2f;
            float start = 0;
            float y_shift = 127;

            m_numberOfElements = m_dimension * m_dimension;
            m_vertexBuffer = InitVertexBuffer();
            SVertex3P3N2T[] vertices = new SVertex3P3N2T[m_numberOfElements];
            m_roots = new Vector3[m_numberOfElements];

            for (int y = 0; y < m_dimension; y++)
            {
                zf = y * interspace;
                for (int x = 0; x < m_dimension; x++)
                {
                    xf = x * interspace;
                    i = m_heightmap.GetPixel(x, y);

                    float b = i.GetBrightness();

                    int index = (y * m_dimension) + x;
                    Vector3 pos = new Vector3(
                        start + xf,
                        b * 255 - 255 + y_shift, 
                        start + zf
                    );

                    vertices[index] = new SVertex3P3N2T(
                        pos, 
                        new Vector3(0.0f, 1.0f, 0.0f),
                        new Vector2(x / (float)m_dimension, y / (float)m_dimension)
                    );

                    m_roots[index] = pos;
                }
            }

            DataStream stream = m_vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<SVertex3P3N2T>(vertices);
            m_vertexBuffer.Unmap();
                                                                                                                           
        }

        public override void CreateIndexBuffer()
        {

            m_indexCount = ((m_dimension - 1)*(m_dimension - 1)) * 6; // -1 because points --> quads ; 6 indices per quad
            m_indexBuffer = InitIndexBuffer();
            UInt32[] indices = new UInt32[m_indexCount];

            int count = 0;
            for (UInt32 y = 0; y < m_dimension-1; y++)
            {

                for (UInt32 x = 0; x < m_dimension-1; x++)
                {

                    indices[count] = y * (UInt32)m_dimension + x;
                    count++;
                    indices[count] = y * (UInt32)m_dimension + x + (UInt32)m_dimension + 1;
                    count++;
                    indices[count] = y * (UInt32)m_dimension + x + 1;
                    count++;
                    indices[count] = y * (UInt32)m_dimension + x;
                    count++;
                    indices[count] = (y + 1) * (UInt32)m_dimension + x;
                    count++;
                    indices[count] = y * (UInt32)m_dimension + x + (UInt32)m_dimension + 1;
                    count++;
                }
            }

            DataStream stream = m_indexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<UInt32>(indices);
            m_indexBuffer.Unmap();
        }
    }
}
