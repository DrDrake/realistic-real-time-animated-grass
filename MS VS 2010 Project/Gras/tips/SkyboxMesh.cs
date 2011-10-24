using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using Barrage.Global;
using Barrage.Textures;
using Barrage.Global.Vertex;
using Barrage.Shaders;

namespace Barrage.Meshes
{
    /// <summary>
    /// SkyboxMesh it's just an extension of the Cube class that loads the Skybox.fx shader without having to do it by hand
    /// </summary>
    public class SkyboxMesh : Barrage.Meshes.Cube
    {
        /// <summary>
        /// Constructor. Loads the texture specified and sets the shader
        /// </summary>
        /// <param name="texture">Texture path</param>
        public SkyboxMesh(string texture)
        {
            this.Materials[0].Textures.Add(CubeTextureManager.Textures[texture]);
            this.Shader = new D3DShader("Skybox.fx");
        }
    }
}
