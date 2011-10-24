using System;
using System.Collections.Generic;
using System.Text;
using Barrage.Meshes;

namespace Barrage.Items
{
    /// <summary>
    /// An static skybox is simply an item with a skybox mesh which doesn't change position around the camera
    /// </summary>
    public class StaticSkybox : Item
    {
        /// <summary>
        /// Constructor. Inits the skybox with the specified texture
        /// </summary>
        /// <param name="texture">Skybox texture path (Cube map)</param>
        public StaticSkybox(string texture)
        {
            addMesh(new SkyboxMesh(texture));
        }
    }
}
