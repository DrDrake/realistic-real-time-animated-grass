using System;
using System.Collections.Generic;
using System.Text;
using Barrage.Meshes;
using SlimDX;

namespace Barrage.Items
{
    /// <summary>
    /// Simply an item with a SkyboxMesh mesh that attachs to the current camera on each render.
    /// </summary>
    public class Skybox : Item
    {
        /// <summary>
        /// Constructor. Inits the skybox mesh with the specified texture
        /// </summary>
        /// <param name="texture">Skybox's cube texture</param>
        public Skybox(string texture)
        {
            addMesh(new SkyboxMesh(texture));
        }

        /// <summary>
        /// \internal Attachs the skybox matrix to the camera position and renders the skybox
        /// </summary>
        protected override void DoRender()
        {
            motion.Translation = Scene.CurrentInstance.Camera.Position;
            base.DoRender();
        }
    }
}
