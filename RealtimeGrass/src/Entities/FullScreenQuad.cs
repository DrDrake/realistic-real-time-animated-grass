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
    class FullScreenQuad : Entity
    {
        ShaderResourceView m_shaderResource;

        public FullScreenQuad(float ambient, float diffuse, float specular, float shininess)
            : base(ambient, diffuse, specular, shininess)
        {
        }

        public override void CreateVertexBuffer()
        {
            m_numberOfElements = 4;

            //Create Vertex Buffer
            m_vertexBuffer = InitVertexBuffer();

            SVertex3P3N2T[] vertices = new SVertex3P3N2T[m_numberOfElements];
            //[Position(float3), Normal(float3), TexCoord(float2)]
            vertices[0] = new SVertex3P3N2T(new Vector3(-1.0f,  1.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f));
            vertices[1] = new SVertex3P3N2T(new Vector3( 1.0f,  1.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector2(1.0f, 0.0f));
            vertices[2] = new SVertex3P3N2T(new Vector3( 1.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector2(1.0f, 1.0f));
            vertices[3] = new SVertex3P3N2T(new Vector3(-1.0f, -1.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.0f));
            
            DataStream stream = m_vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<SVertex3P3N2T>(vertices);
            m_vertexBuffer.Unmap();
        }

        public override void CreateIndexBuffer()
        {
            //Default: Draw each Element once
            m_indexCount = 6;

            //Create Vertex Buffer
            m_indexBuffer = InitIndexBuffer();

            //Create Default indices
            UInt32[] indices = new UInt32[m_indexCount];

            //Screen Quad
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 3;

            indices[3] = 1;
            indices[4] = 2;
            indices[5] = 3;

            //Write Vertices to Buffer
            DataStream stream = m_indexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<UInt32>(indices);
            m_indexBuffer.Unmap();
        }

        public void setTexture(Device device, Texture2D tex, string effectName)
        {
            m_shaderResource = new ShaderResourceView(device, tex);

            m_device = device;
            //Shader
            m_effect = Effect.FromFile(device, effectName, "fx_4_0");

            //VertexBuffer-Layout for Shader
            m_elements = InitElementsLayout();
            m_layout = new InputLayout(
                device,
                m_effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature,
                m_elements
            );

            //Create Vertexbuffer & fill with data
            CreateVertexBuffer();

            //Create Indexbuffer & fill with data (Default: Draw each Vertex from 'CreateVertexBuffer()' once)
            CreateIndexBuffer();

            //Get a binding to render with
            m_binding = new[] { new VertexBufferBinding(m_vertexBuffer, m_bytesPerElement, 0) };
        }

        public override void Draw()
        {
            ChangeRasterizerState(CullMode.Back);

            m_effect.GetVariableByName("model_texture").AsResource().SetResource(m_shaderResource);
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

