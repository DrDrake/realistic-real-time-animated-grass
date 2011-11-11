using System;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using SlimDX.D3DCompiler;
using Buffer = SlimDX.Direct3D10.Buffer;
using Device = SlimDX.Direct3D10.Device;
using PrimitiveTopology = SlimDX.Direct3D10.PrimitiveTopology;

using Uncut.Entities;
using Uncut.Rendering.Mesh;

namespace Uncut
{
    class SimpleModel : Entity
    {
        #region Members
        protected string m_meshName;
        #endregion


        #region Methods

        public SimpleModel(Device device, string effectName, string meshName, string textureName)
            : base(device, effectName, textureName)
        {
            m_meshName = meshName;
        }

        protected override void initPipelineInput()
        {
            LoadMesh(m_meshName);

            //Pipelineinput
            m_elements = new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0), 
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 0, 1), 
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0, 2)
            };

            m_binding = new[] {
                new VertexBufferBinding(m_vertices, 12, 0),
                new VertexBufferBinding(m_normals, 12, 0), 
                new VertexBufferBinding(m_texCoords, 8, 0)
            };

            m_layout = new InputLayout(m_device, m_effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature, m_elements);
        }

        private void LoadMesh(string meshName)
        {
            var meshData = Smd.FromFile(meshName);
            using (var data = new DataStream(meshData.Indices.ToArray(), true, false))
            {
                m_indices = new Buffer(m_device, data, new BufferDescription(meshData.Indices.Count * 4, ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None));
                m_indexCount = meshData.Indices.Count;
            }

            using (var data = new DataStream(meshData.Positions.ToArray(), true, false))
            {
                m_vertices = new Buffer(m_device, data, new BufferDescription(meshData.Positions.Count * 4 * 3, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None));
            }
            using (var data = new DataStream(meshData.Normals.ToArray(), true, false))
            {
                m_normals = new Buffer(m_device, data, new BufferDescription(meshData.Normals.Count * 4 * 3, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None));
            }

            using (var data = new DataStream(meshData.TextureCoordinates.ToArray(), true, false))
            {
                m_texCoords = new Buffer(m_device, data, new BufferDescription(meshData.TextureCoordinates.Count * 4 * 2, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None));
            }
        }

        public override void Draw()
        {
            m_effect.GetVariableByName("model_texture").AsResource().SetResource(m_textureView);
            m_device.InputAssembler.SetInputLayout(m_layout);
            //Choose Primitive to draw
            m_device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            m_device.InputAssembler.SetIndexBuffer(m_indices, Format.R32_UInt, 0);
            m_device.InputAssembler.SetVertexBuffers(0, m_binding);

            m_effect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply();
            m_device.DrawIndexed(m_indexCount, 0, 0);

            m_device.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);
            m_device.InputAssembler.SetVertexBuffers(0, m_nullBinding);
        }

        public override void Dispose()
        {
            m_indices.Dispose();
            m_normals.Dispose();
            m_vertices.Dispose();
            m_texCoords.Dispose();
            m_texture.Dispose();
            m_effect.Dispose();
            m_textureView.Dispose();
            m_layout.Dispose();
        }
        #endregion
    }
}
