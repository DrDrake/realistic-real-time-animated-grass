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

using Uncut.Rendering.Mesh;

namespace Uncut{

    class Hoehenkarte : IDisposable{

        public Effect Effect { get { return effect; } }

        private Effect effect;
        private InputLayout layout;
        private VertexBufferBinding[] binding;
        private InputElement[] elements;
        private Device device;
        private Buffer vertexBuffer;
        private Bitmap hoehenkarte;

        public Hoehenkarte(Device device, string effectName)
        {

            this.device = device;
            effect = Effect.FromFile(device, effectName, "fx_4_0");

            elements = new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("COLOR", 0, SlimDX.DXGI.Format.R32G32B32A32_Float, 16, 0),
                
            };

            layout = new InputLayout(device, effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature, elements);

            vertexBuffer = new Buffer(device,4000000 *32,
                    ResourceUsage.Dynamic,
                    BindFlags.VertexBuffer,
                    CpuAccessFlags.Write,
                    ResourceOptionFlags.None
            );

            LoadVertices();

            binding = new[] { new VertexBufferBinding(vertexBuffer, 32, 0) };
                                                                                                                                            
        }

        private void LoadVertices(){
            
            hoehenkarte = new Bitmap("Resources/hoehenkarte/huegel1000.jpg");
            Color i;
            float xf = 0;
            float yf = 0;
            float start = 10.0f;
            
            DataStream stream = vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);

                    for (int x = 0; x < 1000; x++){
                        xf = x * 0.1f;
                        for (int y = 0; y < 1000; y++){

                            yf = y * 0.1f;
                            i = hoehenkarte.GetPixel(x, y);
                            //System.Console.Write("Wert " + (i.GetBrightness() * 100) + ", "); 
                    stream.WriteRange(new[] {
                //          Position                              Farbe205,170,125

                new Vector4(start + 0.1f +  xf, 0.0f + (i.GetBrightness()*10), start + 0.1f + yf, 1.0f), new Vector4(0.8f, 0.66f, 0.5f, 1.0f),//or
                new Vector4(start +         xf, 0.0f + (i.GetBrightness()*10), start + 0.1f + yf, 1.0f), new Vector4(0.8f, 0.66f, 0.5f, 1.0f),//ur
				new Vector4(start + 0.1f +  xf, 0.0f + (i.GetBrightness()*10), start        + yf, 1.0f), new Vector4(0.8f, 0.66f, 0.5f, 1.0f),//lo
                new Vector4(start +         xf, 0.0f + (i.GetBrightness()*10), start        + yf, 1.0f), new Vector4(0.8f, 0.66f, 0.5f, 1.0f),//lu
			});
                     }
                }
                    
            vertexBuffer.Unmap();
        }

        public void Draw(){

            device.InputAssembler.SetInputLayout(layout);
            device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.LineStrip);// Strip benötigt nur vier Punkte.Punkte der diagonale werden nur einmal gebraucht
            device.InputAssembler.SetVertexBuffers(0, binding);
            effect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply();
            device.Draw(4000000, 0);

        }

        public void Dispose(){

            effect.Dispose();
            layout.Dispose();
        }  
    }
}
