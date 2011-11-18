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
    class LMaterial
    {
        private float ambient, diffuse, specular,shininess;

        public LMaterial()
        {
        }

        public void Init(float Ka, float Kd, float Ks, float A)
        {
            this.ambient = Ka;
            this.diffuse = Kd;
            this.specular = Ks;
            this.shininess = A;

        }

        public float Ka() {
            return this.ambient;
        }
        public float Kd()
        {
            return this.diffuse;
        }
        public float Ks()
        {
            return this.specular;
        }
        public float A()
        {
            return this.shininess;
        }
    }
}
