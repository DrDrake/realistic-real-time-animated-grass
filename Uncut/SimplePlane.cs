using System;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using SlimDX.D3DCompiler;
using Buffer = SlimDX.Direct3D10.Buffer;
using Device = SlimDX.Direct3D10.Device;
using PrimitiveTopology = SlimDX.Direct3D10.PrimitiveTopology;
using MapFlags = SlimDX.Direct3D10.MapFlags;

using Uncut.Rendering.Mesh;
using System.Reflection;

namespace Uncut
{
    class SimplePlane : IDisposable
    {
        public SimplePlane(Device device, string effectName, string textureName)
        {
            this.device = device;

            effect = Effect.FromFile(device, effectName, "fx_4_0");

            //texture = Texture2D.FromFile(device, textureName);

            elements = new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("COLOR", 0, SlimDX.DXGI.Format.R32G32B32A32_Float, 16, 0) //Offset = 4 x sizeof(float)
                //new InputElement("NORMAL", 0, Format.R32G32B32_Float, 0, 1), 
                //new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0, 2)
            };

            layout = new InputLayout(device, effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature, elements);

            //textureView = new ShaderResourceView(device, texture);

            vertexBuffer = new Buffer(
                    device,
                    6 * 32,
                    ResourceUsage.Dynamic,
                    BindFlags.VertexBuffer,
                    CpuAccessFlags.Write,
                    ResourceOptionFlags.None
            );

            LoadVertices();
            /*
            binding = new[] {
                new VertexBufferBinding(vertices, 12, 0),
                new VertexBufferBinding(normals, 12, 0), 
                new VertexBufferBinding(texCoords, 8, 0)
            };*/
            binding = new[] { new VertexBufferBinding(vertexBuffer, 32, 0) };
        }

        private void LoadVertices()
        {
            DataStream stream = vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange(new[] {
                new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.07f, 0.5f, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, -1.0f, 1.0f), new Vector4(0.07f, 0.5f, 0.0f, 1.0f),
				new Vector4(-1.0f, 0.0f, -1.0f, 1.0f), new Vector4(0.05f, 0.5f, 0.0f, 1.0f),

                new Vector4(-1.0f, 0.0f, -1.0f, 1.0f), new Vector4(0.05f, 0.5f, 0.0f, 1.0f),
				new Vector4(-1.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.07f, 0.9f, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.07f, 0.5f, 0.0f, 1.0f),
			});
            vertexBuffer.Unmap();
        }

        public void Draw()
        {
            //effect.GetVariableByName("model_texture").AsResource().SetResource(textureView);
            device.InputAssembler.SetInputLayout(layout);
            device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            //device.InputAssembler.SetIndexBuffer(indices, Format.R32_UInt, 0);
            device.InputAssembler.SetVertexBuffers(0, binding);

            effect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply();
            device.DrawIndexed(indexCount, 0, 0);
            device.Draw(6, 0);

            //device.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);
            device.InputAssembler.SetVertexBuffers(0, nullBinding);

            //Context10.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, 32, 0)); // Stride: Größe des Elemets (Quads) 2 x 4 x 4byte (2 Vector4 x 4 floats á 4 Byte)
        }

        public void Dispose()
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

        public Effect Effect { get { return effect; } }

        private ShaderResourceView textureView;
        private Effect effect;
        private InputLayout layout;
        private readonly VertexBufferBinding[] nullBinding = new VertexBufferBinding[3];
        private VertexBufferBinding[] binding;
        private InputElement[] elements;
        private int indexCount;
        private Device device;
        private Buffer indices;
        private Buffer normals;
        private Buffer vertices;
        private Buffer texCoords;
        private Buffer vertexBuffer;

        private Texture2D texture;
    }
}
