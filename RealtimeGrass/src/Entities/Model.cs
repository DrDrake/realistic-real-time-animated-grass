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

namespace RealtimeGrass
{
    class Model : Entity
    {
        protected string m_meshName;
        private MeshData m_meshData;


        public Model(string meshName)
        {
            m_meshName = meshName;
        }

        public override void CreateVertexBuffer()
        {
            m_meshData = Smd.FromFile(m_meshName);

            m_numberOfElements = m_meshData.Vertices.Count;

            //Create Vertex Buffer
            m_vertexBuffer = InitVertexBuffer();

            //Write Vertices to Buffer
            DataStream stream = m_vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<SVertex3P3N2T>(m_meshData.Vertices.ToArray());
            m_vertexBuffer.Unmap();
        }

        public override void CreateIndexBuffer()
        {
            //Default: Draw each Element once
            m_indexCount = m_meshData.Indices.Count;

            //Create Vertex Buffer
            m_indexBuffer = InitIndexBuffer();

            //Write Vertices to Buffer
            DataStream stream = m_indexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<int>(m_meshData.Indices.ToArray());
            m_indexBuffer.Unmap();
        }
    }
}
