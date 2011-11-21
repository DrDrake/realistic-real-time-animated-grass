using System;


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
    class Light
    {
        private
        Vector4 l_color;
        Vector4 l_dir;

        public Light()
        {
        }

        public void Init(Vector4 color, Vector4 dir)
        {
            this.l_color = color;
            this.l_dir = dir;

        }

        public Vector4 Color()
        {
            return this.l_color;
        }
        public Vector4 Dir()
        {
            return this.l_dir;
        }
    }
}
