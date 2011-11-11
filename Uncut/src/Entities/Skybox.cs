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
    class Skybox : Renderable
    {
        Texture2D top, bottom, left, right, front, back;
        ShaderResourceView topView, bottomView, leftView, rightView, frontView, backView;

        public Skybox(Device device, string effectName, string textureName)
            : base(device, effectName, textureName)
        {
            Init(device, effectName, textureName);

            top = Texture2D.FromFile(device, Asset.Dir("skycube").file("oben.jpg"));
            bottom = Texture2D.FromFile(device, Asset.Dir("skycube").file("unten.jpg"));
            left = Texture2D.FromFile(device, Asset.Dir("skycube").file("links.jpg"));
            right = Texture2D.FromFile(device, Asset.Dir("skycube").file("rechts.jpg"));
            front = Texture2D.FromFile(device, Asset.Dir("skycube").file("vorne.jpg"));
            back = Texture2D.FromFile(device, Asset.Dir("skycube").file("hinten.jpg"));
            topView = new ShaderResourceView(device, top);
            bottomView = new ShaderResourceView(device, bottom);
            leftView = new ShaderResourceView(device, left);
            rightView = new ShaderResourceView(device, right);
            frontView = new ShaderResourceView(device, front);
            backView = new ShaderResourceView(device, back);

            isTextured = true;
        }

        public override InputElement[] InitElementsLayout()
        {
            return new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("COLOR", 0, SlimDX.DXGI.Format.R32G32B32A32_Float, 16, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0)
            };

            //new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0)//, 
            //new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0) // Oder 32?
            //Aufglieder auf 3 Buffer.
        }

        public override Vector4[] InitVertices()
        {
            return new[] {
                new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(-0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(-0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),

                new Vector4(-0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-0.5f, 0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),

                new Vector4(-0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
				new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(0.5f, 0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(-0.5f, 0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),

                new Vector4(-0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
				new Vector4(0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(0.5f, 0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
				new Vector4(-0.5f, 0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(-0.5f, 0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(0.5f, 0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),

                new Vector4(0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),

                new Vector4(0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
				new Vector4(0.5f, 0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),

                new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(-0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(0.5f, -0.5f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
			};
        }

        public override void Draw()
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

            device.InputAssembler.SetInputLayout(layout);
            device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            device.InputAssembler.SetVertexBuffers(0, binding);

            DrawTexturedSide(frontView, 0);
            DrawTexturedSide(rightView, 1);
            DrawTexturedSide(topView, 2);
            DrawTexturedSide(backView, 3);
            DrawTexturedSide(leftView, 4);
            DrawTexturedSide(bottomView, 5);

            device.InputAssembler.SetVertexBuffers(0, nullBinding);
        }

        private void DrawTexturedSide(ShaderResourceView side, int index)
        {
            effect.GetVariableByName("model_texture").AsResource().SetResource(side);
            effect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply();

            device.Draw(6, index * 6);
        }

        public override bool IsRenderedWithCulling()
        {
            return false;
        }

        public override int NumberOfElements()
        {
            return 36;
        }

        public override int NumberOfBytesForOneElement()
        {
            return 48;
        }
    }
}
