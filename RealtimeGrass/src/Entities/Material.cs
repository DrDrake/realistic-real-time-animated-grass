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
        private float m_ambient { get { return m_ambient; } set { m_ambient = value; } }
        private float m_diffuse { get { return m_ambient; } set { m_ambient = value; } }
        private float m_specular { get { return m_ambient; } set { m_ambient = value; } }
        private float m_shininess { get { return m_ambient; } set { m_ambient = value; } }

        public Material(float ambient, float diffuse, float specular, float shininess)
        {
            m_ambient = ambient;
            m_diffuse = diffuse;
            m_specular = specular;
            m_shininess = shininess;
        }
    }
}
