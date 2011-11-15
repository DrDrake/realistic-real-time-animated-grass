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

namespace Uncut{

    class kos : IDisposable{

        public Effect Effect { get { return effect; } }

        private Effect effect;
        private InputLayout layout;
        private VertexBufferBinding[] binding;
        private InputElement[] elements;
        private Device device;
        private Buffer vertexBuffer;

        public kos(Device device, string effectName){

            this.device = device;
            effect = Effect.FromFile(device, effectName, "fx_4_0");

            elements = new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("COLOR", 0, SlimDX.DXGI.Format.R32G32B32A32_Float, 16, 0),
                
            };

            layout = new InputLayout(device, effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature, elements);

            vertexBuffer = new Buffer(device,192,
                    ResourceUsage.Dynamic,
                    BindFlags.VertexBuffer,
                    CpuAccessFlags.Write,
                    ResourceOptionFlags.None
            );

            LoadVertices();

            binding = new[] { new VertexBufferBinding(vertexBuffer, 32, 0) };
                                                                                                                                            
        }

        private void LoadVertices(){

            DataStream stream = vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange(new[] {
                //          Position                              Farbe205,170,125
                new Vector4(500.0f, 0.0f, 0.0f ,1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                new Vector4(-500.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),

				new Vector4(0.0f, 500.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                new Vector4(0.0f,-500.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),

                new Vector4(0.0f, 0.0f, 500.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                new Vector4(0.0f, 0.0f,-500.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
			});
            vertexBuffer.Unmap();
        }

        public void Draw(){

            device.InputAssembler.SetInputLayout(layout);
            device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.LineList);
            device.InputAssembler.SetVertexBuffers(0, binding);
            effect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply();
            device.Draw(6, 0);

        }

        public void Dispose(){

            effect.Dispose();
            layout.Dispose();
        }  
    }
}
