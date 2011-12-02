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
        private Vector3 m_color;
        public Vector3 Color { get { return m_color; } set { m_color = value; } }
        private Vector3 m_direction;
        public Vector3 Direction { get { return m_direction; } set { m_direction = value; } }

        public Light(Vector3 color, Vector3 dir)
        {
            m_color = color;
            m_direction = dir;
        }
    }
}
