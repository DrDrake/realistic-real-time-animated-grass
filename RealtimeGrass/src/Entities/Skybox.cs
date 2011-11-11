using System;
using System.Collections.Generic;
using System.Text;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D10.Buffer;
using Device = SlimDX.Direct3D10.Device;
using MapFlags = SlimDX.Direct3D10.MapFlags;

using RealtimeGrass.Utility;

namespace RealtimeGrass
{
    class Skybox : Entity
    {
        public Skybox()
        {
        }

        public override void CreateVertexBuffer()
        {
            m_numberOfElements = 24;

            //Create Vertex Buffer
            m_vertexBuffer = InitVertexBuffer();

            SVertex3P3N2T[] vertices = new SVertex3P3N2T[m_numberOfElements];
            //[Position(float3), Normal(float3), TexCoord(float2)]
            //Front Quad
            vertices[0] = new SVertex3P3N2T(new Vector3( 0.5f, -0.5f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 0.0f));
            vertices[1] = new SVertex3P3N2T(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(1.0f, 0.0f));
            vertices[2] = new SVertex3P3N2T(new Vector3(-0.5f,  0.5f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(1.0f, 1.0f));
            vertices[3] = new SVertex3P3N2T(new Vector3( 0.5f,  0.5f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 1.0f));

            //Back Quad
            vertices[4] = new SVertex3P3N2T(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(0.0f, 1.0f));
            vertices[5] = new SVertex3P3N2T(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(1.0f, 1.0f));
            vertices[6] = new SVertex3P3N2T(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(1.0f, 1.0f));
            vertices[7] = new SVertex3P3N2T(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(1.0f, 1.0f));

            //Right Quad
            vertices[8]  = new SVertex3P3N2T(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1.0f, 1.0f));
            vertices[9]  = new SVertex3P3N2T(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(0.0f, 1.0f));
            vertices[10] = new SVertex3P3N2T(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(0.0f, 1.0f));
            vertices[11] = new SVertex3P3N2T(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1.0f, 1.0f));

            //Left Quad
            vertices[12] = new SVertex3P3N2T(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(1.0f, 1.0f));
            vertices[13] = new SVertex3P3N2T(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(0.0f, 1.0f));
            vertices[14] = new SVertex3P3N2T(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(0.0f, 1.0f));
            vertices[15] = new SVertex3P3N2T(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(1.0f, 1.0f));

            //Top Quad
            vertices[16] = new SVertex3P3N2T(new Vector3( 0.5f, 0.5f,  0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f));
            vertices[17] = new SVertex3P3N2T(new Vector3(-0.5f, 0.5f,  0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.0f));
            vertices[18] = new SVertex3P3N2T(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.0f));
            vertices[19] = new SVertex3P3N2T(new Vector3( 0.5f, 0.5f, -0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f));

            //Bottom Quad
            vertices[20] = new SVertex3P3N2T(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f));
            vertices[21] = new SVertex3P3N2T(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.0f));
            vertices[22] = new SVertex3P3N2T(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.0f));
            vertices[23] = new SVertex3P3N2T(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f));

            DataStream stream = m_vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<SVertex3P3N2T>(vertices);
            m_vertexBuffer.Unmap();
        }

        public override void CreateIndexBuffer()
        {
            //Default: Draw each Element once
            m_indexCount = 36;

            //Create Vertex Buffer
            m_indexBuffer = InitIndexBuffer();

            //Create Default indices
            UInt32[] indices = new UInt32[m_indexCount];

            //Front Quad
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;

            //Back Quad
            indices[6] = 4;
            indices[7] = 5;
            indices[8] = 6;

            indices[9] = 4;
            indices[10] = 6;
            indices[11] = 7;

            //Right Quad
            indices[12] = 8;
            indices[13] = 9;
            indices[14] = 10;

            indices[15] = 8;
            indices[16] = 10;
            indices[17] = 11;

            //Left Quad
            indices[18] = 12;
            indices[19] = 13;
            indices[20] = 14;

            indices[21] = 12;
            indices[22] = 14;
            indices[23] = 15;

            //Top Quad
            indices[24] = 16;
            indices[25] = 17;
            indices[26] = 18;

            indices[27] = 16;
            indices[28] = 18;
            indices[29] = 19;

            //Bottom Quad
            indices[30] = 20;
            indices[31] = 21;
            indices[32] = 22;

            indices[33] = 20;
            indices[34] = 22;
            indices[35] = 23;

            //Write Vertices to Buffer
            DataStream stream = m_indexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<UInt32>(indices);
            m_indexBuffer.Unmap();
        }
    }
}
