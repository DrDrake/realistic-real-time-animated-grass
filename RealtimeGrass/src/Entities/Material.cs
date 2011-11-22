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
    class Material
    {
        private float m_ambient;
        public float Ambient { get { return m_ambient; } set { m_ambient = value; } }

        private float m_diffuse;
        public float Diffuse { get { return m_diffuse; } set { m_diffuse = value; } }

        private float m_specular;
        public float Specular { get { return m_specular; } set { m_specular = value; } }

        private float m_shininess;
        public float Shininess { get { return m_shininess; } set { m_shininess = value; } }


        public Material(float ambient, float diffuse, float specular, float shininess)
        {
            m_ambient = ambient;
            m_diffuse = diffuse;
            m_specular = specular;
            m_shininess = shininess;
        }
    }
}
