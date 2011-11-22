using System;
using System.Reflection;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using SlimDX.D3DCompiler;
using Buffer = SlimDX.Direct3D10.Buffer;
using Device = SlimDX.Direct3D10.Device;
using PrimitiveTopology = SlimDX.Direct3D10.PrimitiveTopology;
using MapFlags = SlimDX.Direct3D10.MapFlags;

using RealtimeGrass.Rendering.Mesh;
using RealtimeGrass.Utility;

namespace RealtimeGrass.Entities
{
    class Plane : Entity
    {
        float m_scaleX;
        float m_scaleY;

        public Plane(float scaleX, float scaleY)
        {
            m_scaleX = scaleX;
            m_scaleY = scaleY;
        }

        public override void CreateVertexBuffer()
        {
            m_numberOfElements = 6;

            float y_shift = 0.0f;

            //Create Vertex Buffer
            m_vertexBuffer = InitVertexBuffer();

            SVertex3P3N2T[] vertices = new SVertex3P3N2T[m_numberOfElements];
            //[Position(float3), Normal(float3), TexCoord(float2)]
            vertices[0] = new SVertex3P3N2T(new Vector3( 1.0f * m_scaleX, y_shift,  1.0f * m_scaleY), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f));
            vertices[1] = new SVertex3P3N2T(new Vector3( 1.0f * m_scaleX, y_shift, -1.0f * m_scaleY), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 0.0f));
            vertices[2] = new SVertex3P3N2T(new Vector3(-1.0f * m_scaleX, y_shift, -1.0f * m_scaleY), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 0.0f));
                                                                                      
            vertices[3] = new SVertex3P3N2T(new Vector3(-1.0f * m_scaleX, y_shift, -1.0f * m_scaleY), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 0.0f));
            vertices[4] = new SVertex3P3N2T(new Vector3(-1.0f * m_scaleX, y_shift,  1.0f * m_scaleY), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.0f));
            vertices[5] = new SVertex3P3N2T(new Vector3( 1.0f * m_scaleX, y_shift,  1.0f * m_scaleY), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f));
            
            DataStream stream = m_vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<SVertex3P3N2T>(vertices);
            m_vertexBuffer.Unmap();
        }
    }
}
