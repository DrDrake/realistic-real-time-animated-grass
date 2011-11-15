using System;
using System.Reflection;
using System.Collections.Generic;

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
    public interface IEntity : IDisposable
    {
        #region Public Interface

        void                CreateVertexBuffer();
        void                CreateIndexBuffer();
        InputElement[]      InitElementsLayout();
        void                ChangeRasterizerState(CullMode cullMode);
        void                Draw();

        #endregion
    }

    

    class Entity : IEntity
    {
        protected List<TextureFormat>               m_textureFormats;

        protected Effect                            m_effect;
        protected InputLayout                       m_layout;
        protected readonly VertexBufferBinding[]    m_nullBinding = new VertexBufferBinding[3];
        protected VertexBufferBinding[]             m_binding;
        protected Device                            m_device;

        protected InputElement[]                    m_elements;

        //One Buffer for Indices
        protected int                               m_indexCount;
        protected Buffer                            m_indexBuffer;

        //One Buffer for positions, normals, texCoords
        protected Buffer                            m_vertexBuffer;

        protected int                               m_numberOfElements;
        protected int                               m_bytesPerElement;

        public Vector3                              m_SelfRotation;
        public Vector3                              m_Rotation;
        public Vector3                              m_Translation;

        public virtual Effect                       Effect { get { return m_effect; } }


        public Entity()
        {
        }

        public void Init(Device device, string effectName, List<TextureFormat> textureFormats)
        {
            m_SelfRotation = new Vector3(0.0f, 0.0f, 0.0f);
            m_Rotation = new Vector3(0.0f, 0.0f, 0.0f);
            m_Translation = new Vector3(0.0f, 0.0f, 0.0f);

            m_device = device;
            //Shader
            m_effect = Effect.FromFile(device, effectName, "fx_4_0");

            //Texture
            m_textureFormats = textureFormats;

            if (m_textureFormats != null)
            {
                foreach (TextureFormat textureFormat in  m_textureFormats)
                {
                    textureFormat.LoadFromFile(device);
                }
            }

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

        //Override in subclass!!!
        public virtual void CreateVertexBuffer()
        {
            m_numberOfElements = 1;

            //Create Vertex Buffer
            m_vertexBuffer = InitVertexBuffer();

            //Create Vertices
            SVertex3P3N2T[] vertices = new SVertex3P3N2T[m_numberOfElements];
            //[Position(float3), Normal(float3), TexCoord(float2)]
            vertices[0] = new SVertex3P3N2T(new Vector3(1.0f, 0.0f, 1.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f));

            //Write Vertices to Buffer
            DataStream stream = m_vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<SVertex3P3N2T>(vertices);
            m_vertexBuffer.Unmap();
        }

        protected Buffer InitVertexBuffer()
        {
            return new Buffer(
                    m_device,
                    m_numberOfElements * m_bytesPerElement,
                    ResourceUsage.Dynamic,
                    BindFlags.VertexBuffer,
                    CpuAccessFlags.Write,
                    ResourceOptionFlags.None
            );
        }

        public virtual void CreateIndexBuffer()
        {
            //Default: Draw each Element once
            m_indexCount = m_numberOfElements;

            //Create Vertex Buffer
            m_indexBuffer = InitIndexBuffer();
            
            //Create Default indices
            UInt32[] indices = new UInt32[m_indexCount];
            for (UInt32 i = 0; i < m_indexCount; i++)
            {
                indices[i] = i;
            }

            //Write Vertices to Buffer
            DataStream stream = m_indexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<UInt32>(indices);
            m_indexBuffer.Unmap();
        }

        protected Buffer InitIndexBuffer()
        {
            return new Buffer(
                    m_device,
                    m_indexCount * sizeof(int),
                    ResourceUsage.Dynamic,
                    BindFlags.VertexBuffer,
                    CpuAccessFlags.Write,
                    ResourceOptionFlags.None
            );
        }

        public virtual InputElement[] InitElementsLayout()
        {
            m_bytesPerElement = 32;

            return new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0), //3 * 4 Byte(float) = 12 Bytes 
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0), //3 * 4 Byte(float) = 12 Bytes 
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0) //2 * 4 Byte(float) = 8 Bytes 
            };
        }

        public virtual void ChangeRasterizerState(CullMode cullMode)
        {
            RasterizerState state = SlimDX.Direct3D10.RasterizerState.FromDescription
            (
                m_device, new RasterizerStateDescription()
                {
                    CullMode = cullMode,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    FillMode = FillMode.Solid,
                    IsAntialiasedLineEnabled = false,
                    IsDepthClipEnabled = false,
                    IsFrontCounterclockwise = true,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    SlopeScaledDepthBias = 0.0f
                }
            );

            m_device.Rasterizer.State = state;
        }

        public virtual void Draw()
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

        public virtual void Dispose()
        {
            if (m_textureFormats != null)
            {
                foreach (TextureFormat textureFormat in m_textureFormats)
                {
                    textureFormat.Dispose();
                }
            }

            m_indexBuffer.Dispose();
            m_vertexBuffer.Dispose();
            m_effect.Dispose();
            m_layout.Dispose();
        }
    }
}
