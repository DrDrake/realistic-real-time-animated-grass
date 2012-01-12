using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using MapFlags = SlimDX.Direct3D10.MapFlags;
using Device = SlimDX.Direct3D10.Device;

using RealtimeGrass.Utility;

namespace RealtimeGrass.Entities
{
    class Grass : Entity
    {
        protected static Effect m_cachedEffect;
        protected static List<TextureFormat> m_cachedTextureFormats;
        //private Vector3[]       m_roots;
        private ArrayList[,] m_roots;
        private LODNode<GrassRootsChunk> m_rootsNode;
        public LODNode<GrassRootsChunk> rootsNode { get { return m_rootsNode; } set { m_rootsNode = value; } }
        public bool             isInitialized;
        private bool            isUsingLODTree;
        public Matrix m_world;
        public Matrix m_view;
        public Matrix m_proj;
        public float m_frameDelta;
        public Vector3 m_halfWay;
        public Vector3 m_light;
        public Vector3 m_camera;
        public Vector3 m_cameraDirection;

        public Grass(float ambient, float diffuse, float specular, float shininess, ArrayList[,] roots, int numberOfElements)
            : base(ambient, diffuse, specular, shininess)
        {
            m_roots = roots;
            m_numberOfElements = numberOfElements;

            isInitialized = false;
            isUsingLODTree = false;
        }

        public Grass(float ambient, float diffuse, float specular, float shininess, LODNode<GrassRootsChunk> rootsNode, int numberOfElements)
            : base(ambient, diffuse, specular, shininess)
        {
            m_rootsNode = rootsNode;
            m_roots = rootsNode.children[0]/*.children[0]/*.children[0]*/.data.roots;
            //jedes child wird quase eine Grass klasse, mit eigenem index buffer & co!
            //die Render methode hier wird hier dann letzendlich nur die childs aussuchen und diese dann mit .Draw() aufrufen!

            m_numberOfElements = numberOfElements;
            isInitialized = false;
            isUsingLODTree = true;
        }

        public override void Init(Device device, string effectName, List<TextureFormat> textureFormats)
        {
            m_SelfRotation = new Vector3(0.0f, 0.0f, 0.0f);
            m_Rotation = new Vector3(0.0f, 0.0f, 0.0f);
            m_Translation = new Vector3(0.0f, 0.0f, 0.0f);

            m_device = device;
            //Shader
            if (Grass.m_cachedEffect == null)
            {
                Grass.m_cachedEffect = Effect.FromFile(device, effectName, "fx_4_0");
            }
            m_effect = Grass.m_cachedEffect;
                //Effect.FromFile(device, effectName, "fx_4_0");

            //Texture
            if (Grass.m_cachedTextureFormats == null)
            {
                Grass.m_cachedTextureFormats = textureFormats;
                foreach (TextureFormat textureFormat in Grass.m_cachedTextureFormats)
                {
                    textureFormat.LoadFromFile(device);
                }
            }
            m_textureFormats = Grass.m_cachedTextureFormats;
                //textureFormats;
            /*
            if (m_textureFormats != null)
            {
                foreach (TextureFormat textureFormat in m_textureFormats)
                {
                    textureFormat.LoadFromFile(device);
                }
            }*/

            //VertexBuffer-Layout for Shader
            m_elements = InitElementsLayout();
            m_layout = new InputLayout(
                device,
                m_effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature,
                m_elements
            );

            //Create Vertexbuffer & fill with data
            CreateVertexBuffer();

            //Create Indexbuffer & fill with data (Default: Draw each Vertex from 'CreateVertexBuffer()' once)
            CreateIndexBuffer();

            //Get a binding to render with
            m_binding = new[] { new VertexBufferBinding(m_vertexBuffer, m_bytesPerElement, 0) };
        }

        public override void CreateVertexBuffer()
        {
            int actualVertexCount = 0;
            for (int i = 0; i < m_roots.GetLength(0); ++i)
            {
                for (int b = 0; b < m_roots.GetLength(1); ++b)
                {
                    foreach (Vector3 vert in m_roots[i, b])
                    {
                        actualVertexCount++;
                    }
                }
            }
            m_numberOfElements = actualVertexCount;

            Vector3[] unorderedRoots = new Vector3[m_numberOfElements];
            int index = 0;
            for (int i = 0; i < m_roots.GetLength(0); ++i)
            {
                for (int b = 0; b < m_roots.GetLength(1); ++b)
                {
                    foreach (Vector3 vert in m_roots[i, b])
                    {
                        /*index++;
                        if (index > m_roots.Length)
                        {
                            System.Console.WriteLine(index);
                        }*/
                        unorderedRoots[index++] = vert;
                    }
                }
            }
            m_vertexBuffer = InitVertexBuffer();
            DataStream stream = m_vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<Vector3>(unorderedRoots);
            m_vertexBuffer.Unmap();

        }

        public override InputElement[] InitElementsLayout()
        {
            m_bytesPerElement = 12;

            return new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0) //3 * 4 Byte(float) = 12 Bytes
            };
        }

        public override void CreateIndexBuffer()
        {
            base.CreateIndexBuffer();
            isInitialized = true;
        }

        public override void Draw()
        {
            if (isUsingLODTree) //selection and rendering using m_camera
            {
                //int[] ranges = {10, 50, 100, 250, 400, 900, 1000, 1600, 2000}; //25.000 mapsize (500x500 x 50 scale), 6.250, 1.562, 390, 97
                int[] ranges = { 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000 };
                int highestLODLevel = 8; // -> 0, 1, 2, ...!
                GrassNodesSelector selector = new GrassNodesSelector(m_rootsNode);
                if (selector.LODSelect(ranges, highestLODLevel, m_camera, m_cameraDirection)) {
                    //System.Console.WriteLine(GrassNodesSelector.selectionList.Count);
                    foreach (LODNode<GrassRootsChunk> chunk in GrassNodesSelector.selectionList)
                    {
                        if (!chunk.data.entity[0].isInitialized)
                        {
                            ImageLoadInformation m_defaultLoadInfo = ImageLoadInformation.FromDefaults();
                            TextureFormat texFormat5 = new TextureFormat(
                                "Resources/texture/GrassDiffuse01.jpg",
                                m_defaultLoadInfo,
                                TextureType.TextureTypeDiffuse,
                                "grass_diffuse01"
                            );
                            TextureFormat texFormat6 = new TextureFormat(
                                "Resources/texture/GrassDiffuse02.jpg",
                                m_defaultLoadInfo,
                                TextureType.TextureTypeDiffuse,
                                "grass_diffuse02"
                            );
                            TextureFormat texFormat7 = new TextureFormat(
                                "Resources/texture/GrassAlpha.jpg",
                                m_defaultLoadInfo,
                                TextureType.TextureTypeDiffuse,
                                "grass_alpha"
                            );
                            TextureFormat texFormat8 = new TextureFormat(
                                "Resources/texture/noise1024x773.jpg",
                                m_defaultLoadInfo,
                                TextureType.TextureTypeDiffuse,
                                "grass_noise"
                            );
                            TextureFormat texFormat9 = new TextureFormat(
                                "Resources/texture/phasenverschiebung.jpg",
                                m_defaultLoadInfo,
                                TextureType.TextureTypeDiffuse,
                                "grass_shift"
                            );

                            List<TextureFormat> textureFormats5 = new List<TextureFormat>();
                            textureFormats5.Add(texFormat5);
                            textureFormats5.Add(texFormat6);
                            textureFormats5.Add(texFormat7);
                            textureFormats5.Add(texFormat8);
                            textureFormats5.Add(texFormat9);
                            chunk.data.entity[0].Init(m_device, "Resources/shader/GrassTextured.fx", textureFormats5);
                        }
                        chunk.data.entity[0].Effect.GetVariableByName("world").AsMatrix().SetMatrix(m_world);
                        chunk.data.entity[0].Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                        chunk.data.entity[0].Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                        chunk.data.entity[0].Effect.GetVariableByName("time").AsScalar().Set(m_frameDelta);
                        chunk.data.entity[0].SetShaderMaterial();
                        chunk.data.entity[0].Effect.GetVariableByName("halfwayWS").AsVector().Set(m_halfWay);
                        chunk.data.entity[0].Effect.GetVariableByName("l_dirWS").AsVector().Set(m_light);
                        chunk.data.entity[0].Effect.GetVariableByName("cam_Pos").AsVector().Set(m_camera);
                        chunk.data.entity[0].Draw();
                        //System.Console.WriteLine(chunk.data.originX + ":" + chunk.data.originY + " - " + chunk.data.width + ":" + chunk.data.height);
                    }
                }
/*                int childID = 0;
                if (!m_rootsNode.children[childID].data.entity[0].isInitialized)
                {
                    ImageLoadInformation m_defaultLoadInfo = ImageLoadInformation.FromDefaults();
                    TextureFormat texFormat5 = new TextureFormat(
                        "Resources/texture/GrassDiffuse01.jpg",
                        m_defaultLoadInfo,
                        TextureType.TextureTypeDiffuse,
                        "grass_diffuse01"
                    );
                    TextureFormat texFormat6 = new TextureFormat(
                        "Resources/texture/GrassDiffuse02.jpg",
                        m_defaultLoadInfo,
                        TextureType.TextureTypeDiffuse,
                        "grass_diffuse02"
                    );
                    TextureFormat texFormat7 = new TextureFormat(
                        "Resources/texture/GrassAlpha.jpg",
                        m_defaultLoadInfo,
                        TextureType.TextureTypeDiffuse,
                        "grass_alpha"
                    );
                    TextureFormat texFormat8 = new TextureFormat(
                        "Resources/texture/noise1024x773.jpg",
                        m_defaultLoadInfo,
                        TextureType.TextureTypeDiffuse,
                        "grass_noise"
                    );
                    TextureFormat texFormat9 = new TextureFormat(
                        "Resources/texture/phasenverschiebung.jpg",
                        m_defaultLoadInfo,
                        TextureType.TextureTypeDiffuse,
                        "grass_shift"
                    );

                    List<TextureFormat> textureFormats5 = new List<TextureFormat>();
                    textureFormats5.Add(texFormat5);
                    textureFormats5.Add(texFormat6);
                    textureFormats5.Add(texFormat7);
                    textureFormats5.Add(texFormat8);
                    textureFormats5.Add(texFormat9);
                    m_rootsNode.children[childID].data.entity[0].Init(m_device, "Resources/shader/GrassTextured.fx", textureFormats5);
                }
                //m_rootsNode.data.entity[0].Draw();
                m_rootsNode.children[childID].data.entity[0].Effect.GetVariableByName("world").AsMatrix().SetMatrix(m_world);
                m_rootsNode.children[childID].data.entity[0].Effect.GetVariableByName("view").AsMatrix().SetMatrix(m_view);
                m_rootsNode.children[childID].data.entity[0].Effect.GetVariableByName("proj").AsMatrix().SetMatrix(m_proj);
                m_rootsNode.children[childID].data.entity[0].Effect.GetVariableByName("time").AsScalar().Set(m_frameDelta);
                m_rootsNode.children[childID].data.entity[0].SetShaderMaterial();
                m_rootsNode.children[childID].data.entity[0].Effect.GetVariableByName("halfwayWS").AsVector().Set(m_halfWay);
                m_rootsNode.children[childID].data.entity[0].Effect.GetVariableByName("l_dirWS").AsVector().Set(m_light);
                m_rootsNode.children[childID].data.entity[0].Effect.GetVariableByName("cam_Pos").AsVector().Set(m_camera);
                m_rootsNode.children[childID].data.entity[0].Draw();*/
            }
            else
            {
                base.Draw();
            }
        }
    }
}
