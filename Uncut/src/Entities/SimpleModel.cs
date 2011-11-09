using System;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using SlimDX.D3DCompiler;
using Buffer = SlimDX.Direct3D10.Buffer;
using Device = SlimDX.Direct3D10.Device;
using PrimitiveTopology = SlimDX.Direct3D10.PrimitiveTopology;


using Uncut.Rendering.Mesh;
using Uncut.Utility;

namespace Uncut
{
    class SimpleModel : Entity
    {
        #region Members
        protected string m_meshName;
        #endregion


        #region Methods

        public SimpleModel(string meshName)
        {
            m_meshName = meshName;
        }

        public override InputElement[] InitElementsLayout()
        {
            return new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("COLOR", 0, SlimDX.DXGI.Format.R32G32B32A32_Float, 16, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0)
            };
        }

        public Vector4[] InitVertices()
        {
            return null;
        }

        private void LoadMesh(string meshName)
        {
            var meshData = Smd.FromFile(meshName);
            using (var data = new DataStream(meshData.Indices.ToArray(), true, false))
            {
                m_indexBuffer = new Buffer(m_device, data, new BufferDescription(meshData.Indices.Count * 4, ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None));
                m_indexCount = meshData.Indices.Count;
            }

            using (var data = new DataStream(meshData.Positions.ToArray(), true, false))
            {
                //m_vertices = new Buffer(m_device, data, new BufferDescription(meshData.Positions.Count * 4 * 3, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None));
            }
            using (var data = new DataStream(meshData.Normals.ToArray(), true, false))
            {
                //m_normals = new Buffer(m_device, data, new BufferDescription(meshData.Normals.Count * 4 * 3, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None));
            }

            using (var data = new DataStream(meshData.TextureCoordinates.ToArray(), true, false))
            {
                //m_texCoords = new Buffer(m_device, data, new BufferDescription(meshData.TextureCoordinates.Count * 4 * 2, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None));
            }
        }

        public override void Draw()
        {
            //Texturing
            foreach (TextureFormat textureFormat in m_textureFormats)
            {
                m_effect.GetVariableByName(textureFormat.ShaderName).AsResource().SetResource(textureFormat.ShaderResource);
            }
            //Set Layout
            m_device.InputAssembler.SetInputLayout(m_layout);
            //Draw a List of Triangles, 3 Vertices make up 1 Triangle
            m_device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            //Set Buffers
            m_device.InputAssembler.SetIndexBuffer(m_indexBuffer, Format.R32_UInt, 0);
            m_device.InputAssembler.SetVertexBuffers(0, m_binding);
            //Apply Shader
            m_effect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply();

            //Draw Indices
            m_device.DrawIndexed(m_indexCount, 0, 0);

            //Unset Buffers
            m_device.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);
            m_device.InputAssembler.SetVertexBuffers(0, m_nullBinding);
        }

        public override void Dispose()
        {
            m_indexBuffer.Dispose();
            m_effect.Dispose();
            m_layout.Dispose();
        }
        #endregion
    }
}
