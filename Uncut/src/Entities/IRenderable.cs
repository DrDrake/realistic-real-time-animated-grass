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

using Uncut.Rendering.Mesh;

namespace Uncut
{
    public interface IRenderable : IDisposable
    {
        #region Public Interface

        InputElement[]   InitElementsLayout();
        Buffer           InitVertexBuffer();
        void             LoadVertices();
        Vector4[]        InitVertices();
        int              NumberOfElements();
        int              NumberOfBytesForOneElement();
        void             Draw();
        bool             IsRenderedWithCulling();
        //Effect         Effect();
        void             Init(Device device, string effectName, string textureName);

        #endregion
    }

    class Renderable : IRenderable
    {
        protected ShaderResourceView textureView;
        protected Effect effect;
        protected InputLayout layout;
        protected readonly VertexBufferBinding[] nullBinding = new VertexBufferBinding[3];
        protected VertexBufferBinding[] binding;
        private InputElement[] elements;
        private int indexCount;
        protected Device device;
        private Buffer indices;
        private Buffer normals;
        private Buffer vertices;
        private Buffer texCoords;
        private Buffer vertexBuffer;
        private Texture2D texture;
        protected bool isTextured;

        public Renderable(Device device, string effectName, string textureName)
        {
            Init(device, effectName, textureName);
        }

        public virtual void Init(Device device, string effectName, string textureName)
        {
            this.device = device;

            effect = Effect.FromFile(device, effectName, "fx_4_0");

            isTextured = false;
            if (textureName != null) //textureName.Length.Equals(0))
            {
                isTextured = true;
            }

            if (isTextured)
            {
                texture = Texture2D.FromFile(device, textureName);
                textureView = new ShaderResourceView(device, texture);
            }

            elements = InitElementsLayout();

            layout = new InputLayout(device, effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature, elements);

            vertexBuffer = InitVertexBuffer();

            LoadVertices();
            /*
            binding = new[] {
                new VertexBufferBinding(vertices, 12, 0),
                new VertexBufferBinding(normals, 12, 0), 
                new VertexBufferBinding(texCoords, 8, 0)
            };*/
            binding = new[] { new VertexBufferBinding(vertexBuffer, NumberOfBytesForOneElement(), 0) };
        }

        public virtual void LoadVertices()
        {
            DataStream stream = vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange(InitVertices());
            vertexBuffer.Unmap();
        }

        public virtual Vector4[] InitVertices()
        {
            return new[] {
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
			};
        }

        public virtual int NumberOfElements()
        {
            return 0;
        }

        public virtual int NumberOfBytesForOneElement()
        {
            return 0;
        }

        public virtual InputElement[] InitElementsLayout()
        {
            return new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("COLOR", 0, SlimDX.DXGI.Format.R32G32B32A32_Float, 16, 0)
            };
        }

        public virtual Buffer InitVertexBuffer()
        {
            return new Buffer(
                    device,
                    NumberOfElements() * NumberOfBytesForOneElement(),
                    ResourceUsage.Dynamic,
                    BindFlags.VertexBuffer,
                    CpuAccessFlags.Write,
                    ResourceOptionFlags.None
            );
        }

        public virtual bool IsRenderedWithCulling()
        {
            return true;
        }

        public virtual void Draw()
        {
            if (!IsRenderedWithCulling())
            {
                RasterizerState state = SlimDX.Direct3D10.RasterizerState.FromDescription(device, new RasterizerStateDescription()
                {
                    CullMode = CullMode.None,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    FillMode = FillMode.Solid,
                    IsAntialiasedLineEnabled = false,
                    IsDepthClipEnabled = false,
                    IsFrontCounterclockwise = true,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    SlopeScaledDepthBias = 0.0f
                });

                device.Rasterizer.State = state;
            }
            
            if (isTextured)
            {
                effect.GetVariableByName("model_texture").AsResource().SetResource(textureView);
            }

            device.InputAssembler.SetInputLayout(layout);
            device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            //device.InputAssembler.SetIndexBuffer(indices, Format.R32_UInt, 0);
            device.InputAssembler.SetVertexBuffers(0, binding);

            effect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply();
            //device.DrawIndexed(indexCount, 0, 0);
            device.Draw(NumberOfElements(), 0);

            //device.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);
            device.InputAssembler.SetVertexBuffers(0, nullBinding);

            //Context10.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, 32, 0)); // Stride: Größe des Elemets (Quads) 2 x 4 x 4byte (2 Vector4 x 4 floats á 4 Byte)    
        }

        public virtual void Dispose()
        {
            indices.Dispose();
            normals.Dispose();
            vertices.Dispose();
            texCoords.Dispose();
            texture.Dispose();
            effect.Dispose();
            textureView.Dispose();
            layout.Dispose();
        }

        public virtual Effect Effect { get { return effect; } }
    }
}
