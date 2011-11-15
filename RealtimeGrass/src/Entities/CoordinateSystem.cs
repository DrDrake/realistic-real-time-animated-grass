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

namespace RealtimeGrass.Entities
{
    class CoordinateSystem : Entity
    {
        public CoordinateSystem()
        {
        }

        public override void CreateVertexBuffer()
        {
            m_numberOfElements = 7;

            //Create Vertex Buffer
            m_vertexBuffer = InitVertexBuffer();

            float length = 10.0f;

            SVertex3P3N2T[] vertices = new SVertex3P3N2T[m_numberOfElements];
            //[Position(float3), Normal(float3), TexCoord(float2)]
            vertices[0] = new SVertex3P3N2T(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f));
            //X-Achse
            vertices[1] = new SVertex3P3N2T(new Vector3(length, 0.0f, -0.2f), new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f));
            vertices[2] = new SVertex3P3N2T(new Vector3(length, 0.0f,  0.2f), new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f));
            //Y-Achse
            vertices[3] = new SVertex3P3N2T(new Vector3(-0.2f, length, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f));
            vertices[4] = new SVertex3P3N2T(new Vector3( 0.2f, length, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f));
            //Z-Achse
            vertices[5] = new SVertex3P3N2T(new Vector3(-0.2f, 0.0f, length), new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f));
            vertices[6] = new SVertex3P3N2T(new Vector3( 0.2f, 0.0f, length), new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f));//*/

            DataStream stream = m_vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<SVertex3P3N2T>(vertices);
            m_vertexBuffer.Unmap();
        }

        public override void CreateIndexBuffer()
        {
            //Default: Draw each Element once
            m_indexCount = 9;

            //Create Vertex Buffer
            m_indexBuffer = InitIndexBuffer();

            //Create Default indices
            UInt32[] indices = new UInt32[m_indexCount];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 0;
            indices[4] = 3;
            indices[5] = 4;

            indices[6] = 0;
            indices[7] = 5;
            indices[8] = 6;

            //Write Vertices to Buffer
            DataStream stream = m_indexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<UInt32>(indices);
            m_indexBuffer.Unmap();
        }

        public override void Draw()
        {
            ChangeRasterizerState(CullMode.None);

            //Texturing
            if (m_textureFormats != null)
            {
                foreach (TextureFormat textureFormat in m_textureFormats)
                {
                    m_effect.GetVariableByName(textureFormat.ShaderName).AsResource().SetResource(textureFormat.ShaderResource);
                }
            }
            //Set Layout
            m_device.InputAssembler.SetInputLayout(m_layout);
            //Draw a List of Triangles, 3 Vertices make up 1 Triangle
            m_device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            //Set Buffers
            m_device.InputAssembler.SetIndexBuffer(m_indexBuffer, Format.R32_UInt, 0);
            m_device.InputAssembler.SetVertexBuffers(0, m_binding);
            //Apply Shader
            m_effect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply();

            //Draw Indices
            m_device.DrawIndexed(m_indexCount, 0, 0);

            //Unset Buffers
            m_device.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);
            m_device.InputAssembler.SetVertexBuffers(0, m_nullBinding);
        }
    }
}
