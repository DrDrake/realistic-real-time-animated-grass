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
        private Point m_dimension;
        private Vector3[] m_roots;
        public Vector3[] Roots { get { return m_roots; } set { m_roots = value; } }

        public Heightmap(float ambient, float diffuse, float specular, float shininess, string heightMapName)
            :base(ambient, diffuse, specular, shininess)
        {
            m_heightmap = new Bitmap(heightMapName);
            m_dimension.X = m_heightmap.Width;
            m_dimension.Y = m_heightmap.Height;
        }

        public override void CreateVertexBuffer(){

            Color i;
            float xf = 0;
            float zf = 0;
            float interspace = 2f;
            float start = 0;

            m_numberOfElements = m_dimension.X * m_dimension.Y;
            m_vertexBuffer = InitVertexBuffer();
            SVertex3P3N2T[] vertices = new SVertex3P3N2T[m_numberOfElements];
            m_roots = new Vector3[m_numberOfElements];

            for (int y = 0; y < m_dimension.Y; y++)
            {
                zf = y * interspace;
                for (int x = 0; x < m_dimension.X; x++)
                {
                    xf = x * interspace;
                    i = m_heightmap.GetPixel(x, y);

                    float b = i.GetBrightness();

                    int index = (y * m_dimension.X) + x;
                    Vector3 pos = new Vector3(
                        start + xf,
                        b * (m_dimension.X/2) - (m_dimension.X/4), 
                        start + zf
                    );

                    vertices[index] = new SVertex3P3N2T(
                        pos, 
                        new Vector3(0.0f, 1.0f, 0.0f),
                        new Vector2(x / (float)m_dimension.X, y / (float)m_dimension.Y)
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

            m_indexCount = ((m_dimension.X - 1)*(m_dimension.Y - 1)) * 6; // -1 because points --> quads ; 6 indices per quad
            m_indexBuffer = InitIndexBuffer();
            UInt32[] indices = new UInt32[m_indexCount];

            int count = 0;
            for (UInt32 y = 0; y < m_dimension.Y - 1; y++)
            {

                for (UInt32 x = 0; x < m_dimension.X - 1; x++)
                {

                    indices[count] = y * (UInt32)m_dimension.X + x;
                    count++;
                    indices[count] = y * (UInt32)m_dimension.X + x + (UInt32)m_dimension.Y + 1;
                    count++;
                    indices[count] = y * (UInt32)m_dimension.X + x + 1;
                    count++;
                    indices[count] = y * (UInt32)m_dimension.X + x;
                    count++;
                    indices[count] = (y + 1) * (UInt32)m_dimension.X + x;
                    count++;
                    indices[count] = y * (UInt32)m_dimension.X + x + (UInt32)m_dimension.Y + 1;
                    count++;
                }
            }

            DataStream stream = m_indexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<UInt32>(indices);
            m_indexBuffer.Unmap();
        }
    }
}
