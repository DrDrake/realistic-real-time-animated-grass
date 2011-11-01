using System;
using System.Collections.Generic;
using System.Text;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D10;
using Buffer = SlimDX.Direct3D10.Buffer;
using Device = SlimDX.Direct3D10.Device;

namespace Uncut
{
    class Cube : Renderable
    {
        public Cube(Device device, string effectName, string textureName) : base (device, effectName, textureName)
        {
            Init(device, effectName, textureName);
        }

        public override InputElement[] InitElementsLayout()
        {
            return new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0), //3 x 4 Byte.
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0) //4 x 4 Byte. Offset = 4 x sizeof(float)

                //Layout im Speicher:
                //Bytes: [1-4][5-8][9-12][13-16][17-20][21-24][25-28][29-32]
                //Inhalt:[R32][G32][B32 ][LEER ][R32  ][G32  ][B32  ][A32  ]
                //                         ^-> Hier ist eine leere Stelle da beim zweiten InputElement ein Offset von 16 Byte gewählt wurde (vorletzter Parameter).
                //                             BTW: Später Schreiben wir an diese Stelle trotzdem noch Daten, bin mir nicht sicher ob das ein Bug ist.
            };
        }

        public override Vector4[] InitVertices()
        {
            return new[] {
                new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(-0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(-0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(-0.5f, 0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-0.5f, 0.5f, -0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-0.5f, -0.5f, -0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),

                new Vector4(-0.5f, -0.5f, -0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
				new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-0.5f, 0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),

                new Vector4(0.5f, 0.5f, -0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
				new Vector4(-0.5f, 0.5f, -0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                new Vector4(-0.5f, 0.5f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),

                new Vector4(-0.5f, 0.5f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
				new Vector4(0.5f, 0.5f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                new Vector4(0.5f, 0.5f, -0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),

                new Vector4(0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
				new Vector4(-0.5f, 0.5f, -0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),

                new Vector4(-0.5f, 0.5f, -0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
				new Vector4(0.5f, 0.5f, -0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),

                new Vector4(0.5f, 0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
				new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                new Vector4(0.5f, -0.5f, -0.5f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),

                new Vector4(0.5f, -0.5f, -0.5f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
				new Vector4(0.5f, 0.5f, -0.5f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                new Vector4(0.5f, 0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),

                new Vector4(0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
				new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),

                new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
				new Vector4(-0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                new Vector4(0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
			};
        }

        public override int NumberOfElements()
        {
            return 36;
        }

        public override int NumberOfBytesForOneElement()
        {
            return 32;
        }

        public override bool IsRenderedWithCulling()
        {
            return true;
        }
    }
}
