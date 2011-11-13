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

using Uncut.Utility;
using Uncut.Rendering.Mesh;

namespace Uncut{

    class Hoehenkarte : Entity{

        private Bitmap hoehenkarte;

        public Hoehenkarte(){
 
        }

        public override void CreateVertexBuffer(){

            Color i;
            float xf = 0;
            float zf = 0;
            float start = 0;
            hoehenkarte = new Bitmap("Resources/texture/heightmap/huegel1000x1000.jpg");
            
            m_numberOfElements = 1000000;
            m_vertexBuffer = InitVertexBuffer();
            SVertex3P3N2T[] vertices = new SVertex3P3N2T[m_numberOfElements];
                for (int y = 0; y < 1000; y++){
                    zf = y * 0.5f;
                    for (int x = 0; x < 1000; x++){
                        xf = x * 0.5f;
                        i = hoehenkarte.GetPixel(x, y);
                        vertices[(y * 1000) + x] = new SVertex3P3N2T(new Vector3(start + xf, 0.0f + ((i.GetBrightness() * 10) - 10), start + zf), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f));
                    }
                }

            DataStream stream = m_vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<SVertex3P3N2T>(vertices);
            m_vertexBuffer.Unmap();
                                                                                                                           
        }

        public override void CreateIndexBuffer(){

            m_indexCount = 998001 * 6;
            m_indexBuffer = InitIndexBuffer();
            UInt32[] indices = new UInt32[m_indexCount];

            int z = 0;
            for (UInt32 y = 0; y < 999; y++){

                for (UInt32 x = 0; x < 999; x++){

                    indices[z] = y * 1000 + x;
                    z++;
                    indices[z] = y * 1000 + x + 1001;
                    z++;
                    indices[z] = y * 1000 + x + 1;
                    z++;
                    indices[z] = y * 1000 + x;
                    z++;
                    indices[z] = (y + 1) * 1000 + x;
                    z++;
                    indices[z] = y * 1000 + x + 1001;
                    z++;
                    }
                }

                DataStream stream = m_indexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
                stream.WriteRange<UInt32>(indices);
                m_indexBuffer.Unmap();

            }
        }
}
