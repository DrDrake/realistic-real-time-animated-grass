using System;
using System.Reflection;
using System.Drawing;
using System.Collections;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using SlimDX.D3DCompiler;
using Buffer = SlimDX.Direct3D10.Buffer;
using Device = SlimDX.Direct3D10.Device;
using PrimitiveTopology = SlimDX.Direct3D10.PrimitiveTopology;
using MapFlags = SlimDX.Direct3D10.MapFlags;

using RealtimeGrass.Utility;

namespace RealtimeGrass.Entities
{
    class Heightmap : Entity
    {
        private Bitmap m_heightmap;
        private Point m_dimension;
        private float m_interS; //Needs to be an Integer!
        private float m_start;
        private int m_sum;
        private int m_sum1;
        private int m_numberOfRootElements;
        public int numberOfRootElements { get { return m_numberOfRootElements; } set { m_numberOfRootElements = value; } }
        //private Vector3[] m_roots;
        private ArrayList[,] m_roots;
        public ArrayList[,] Roots { get { return m_roots; } set { m_roots = value; } }
        private LODNode<GrassRootsChunk> m_rootsNode;
        public LODNode<GrassRootsChunk> RootsNode { get { return m_rootsNode; } set { m_rootsNode = value; } }

        public Heightmap(float ambient, float diffuse, float specular, float shininess, string heightMapName, float intersectionSize, float start, int hillyness, int heightOffset)
            :base(ambient, diffuse, specular, shininess)
        {
            m_heightmap = new Bitmap(heightMapName);
            m_dimension.X = m_heightmap.Width;
            m_dimension.Y = m_heightmap.Height;
            m_interS = intersectionSize;
            m_start = start;
            m_sum = hillyness;
            m_sum1 = heightOffset;
        }

        public override void CreateVertexBuffer(){

            Color i;
            float xf = 0;
            float zf = 0;
            float interspace = m_interS;
            float start = m_start;

            m_numberOfElements = m_dimension.X * m_dimension.Y;
            m_vertexBuffer = InitVertexBuffer();
            SVertex3P3N2T[] vertices = new SVertex3P3N2T[m_numberOfElements];
            int rootsPerIntersectionMultiplicator = 51;//51; //Needs to be at least 2! //21
            m_numberOfRootElements = m_numberOfElements * rootsPerIntersectionMultiplicator;
            //m_roots = new Vector3[m_numberOfRootElements];
            m_roots = new ArrayList[m_dimension.X-1, m_dimension.Y-1];
            Vector3 lastPos = new Vector3();
            int rootNextIndex = 0;
            //Bad constants that only look good on a certain heighmap texture. Should be replaced in future versions to support any texture equaly good ...
            int kHeightNormalizationHuegel1000x1000 = 1500; //This value defines an offset by which the terrain is lowered to harmonize with the water surface.
            int kHeightNormalizationHuegel500x500 = 1000;
            int kHillynessHuegel1000x1000 = 65*10; //This value defines the height of hills. Lower values will generate flat lands.
            int kHillynessHuegel500x500 = 45*10; // * 50 normalerweise?

            //System.Console.WriteLine(((m_dimension.X / 4) + m_sum) - ((m_dimension.X / 800) + m_sum1));

            for (int y = 0; y < m_dimension.Y; y++)
            {
                zf = y * interspace;
                for (int x = 0; x < m_dimension.X; x++)
                {
                    xf = x * interspace;
                    i = m_heightmap.GetPixel(x, y);

                    float b = i.GetBrightness();

                    int index = (y * m_dimension.X) + x;
                    Vector3 pos = new Vector3(
                        /*start +*/ xf - ((m_dimension.X/2) * interspace),
                        b /** interspace*/ * kHillynessHuegel500x500 - kHeightNormalizationHuegel500x500+800,//(m_sum - m_sum1 / 800),// * ((m_dimension.X/4)+m_sum) - ((m_dimension.X/800)+m_sum1), 

                        /*start +*/ zf - ((m_dimension.Y / 2) * interspace)
                    );

                    vertices[index] = new SVertex3P3N2T(
                        pos, 
                        new Vector3(0.0f, 1.0f, 0.0f),
                        new Vector2(x / (float)m_dimension.X, y / (float)m_dimension.Y)
                    );

                    /*if (index < m_numberOfRootElements)
                    {
                        m_roots[index] = pos;
                    }*/
                    /*for (int rootindex = 0; rootindex < rootsPerIntersectioMultiplicator; ++rootindex)
                    {
                        Vector3 nextPos = new Vector3();
                        nextPos = lastPos + ((float)rootindex / (float)rootsPerIntersectioMultiplicator) * (pos - lastPos);
                        if (rootNextIndex < numberOfRootElements)
                        {
                            //System.Console.WriteLine(rootNextIndex);
                            m_roots[rootNextIndex++] = nextPos;
                        }
                        else
                        {
                            System.Console.WriteLine(rootNextIndex);
                        }
                    }
                    lastPos = pos;*/
                }
            }

            Random randomNumber = new Random();
            for (int y = 0; y < m_dimension.Y-1; ++y)
            {
                for (int x = 0; x < m_dimension.X-1; ++x)
                {
                    //Read a quad.
                    int index = (y * m_dimension.X) + x;
                    Vector3 pos1 = vertices[index].Position;
                    index = ((y + 1) * m_dimension.X) + x;
                    Vector3 pos2 = vertices[index].Position;
                    index = (y * m_dimension.X) + (x + 1);
                    Vector3 pos3 = vertices[index].Position;
                    index = ((y + 1) * m_dimension.X) + (x + 1);
                    Vector3 pos4 = vertices[index].Position;

                    //Make random roots the amount of 'rootsPerIntersectioMultiplicator'
                    //Divide between two polygons
                    //pol 1 = pos1 + pos3 + pos2
                    //pol 2 = pos1 + pos3 + pos4
                    m_roots[x, y] = new ArrayList();
                    Vector3 ab = new Vector3();
                    Vector3 ac = new Vector3();
                    ab = pos3 - pos1;
                    ac = pos2 - pos1;
                    rootNextIndex = 0;
                    ArrayList list = new ArrayList();
                    list.Add(new Vector3(0,0,0));
                    for (UInt32 roots = 0; roots < rootsPerIntersectionMultiplicator / 2; ++roots)
                    {
                        //random pos in pol 1;
                        double rand1 = randomNumber.NextDouble();
                        double rand2 = randomNumber.NextDouble();
                        if (rand1 + rand2 >= 1)
                        {
                            rand1 = 1 - rand1;
                            rand2 = 1 - rand2;
                        }
                        Vector3 randomPoint = pos1 + (float)rand1 * ab + (float)rand2 * ac;
                        if (rootNextIndex < m_numberOfRootElements)
                        {
                            //System.Console.WriteLine(rootNextIndex);
                            //m_roots[rootNextIndex++] = randomPoint;
                            m_roots[x, y].Add(randomPoint);
                        }
                        else
                        {
                            System.Console.WriteLine("Generated to much roots. Current total: "+rootNextIndex);
                        }
                    }
                    ac = pos4 - pos1;
                    rootNextIndex = 0;
                    for (UInt32 roots = 0; roots < rootsPerIntersectionMultiplicator / 2; ++roots)
                    {
                        //random pos in pol 2;                        
                        double rand1 = randomNumber.NextDouble();
                        double rand2 = randomNumber.NextDouble();
                        if (rand1 + rand2 >= 1)
                        {
                            rand1 = 1 - rand1;
                            rand2 = 1 - rand2;
                        }
                        Vector3 randomPoint = pos1 + (float)rand1 * ab + (float)rand2 * ac;
                        if (rootNextIndex < m_numberOfRootElements)
                        {
                            //System.Console.WriteLine(rootNextIndex);
                            //m_roots[rootNextIndex++] = randomPoint;
                            m_roots[x, y].Add(randomPoint);
                        }
                        else
                        {
                            System.Console.WriteLine("Generated to much roots. Current total: " + rootNextIndex);
                        }
                    }
                }
            }
/*
            LODNode<GrassRootsChunk> root = new LODNode<GrassRootsChunk>();
            GrassRootsChunk rootChunk = new GrassRootsChunk();
            rootChunk.roots = m_roots;
            rootChunk.width = m_dimension.X;
            rootChunk.height = m_dimension.Y;
            rootChunk.entity = new Grass[1];
            root.data = rootChunk;
            //LODNode<Grass> testn = new LODNode<Grass>();

            GrassNodesLoader loader = new GrassNodesLoader();
            loader.initWithMaximalDepth(ref root, 9);

            m_rootsNode = root;
*/
            DataStream stream = m_vertexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<SVertex3P3N2T>(vertices);
            m_vertexBuffer.Unmap();                                                                                             
        }
        static Random r = new Random(); 
        public void prepareGrassNodes()
        {
            LODNode<GrassRootsChunk> root = new LODNode<GrassRootsChunk>();
            GrassRootsChunk rootChunk = new GrassRootsChunk();

            rootChunk.roots = m_roots;
            rootChunk.width = m_dimension.X * m_interS;
            rootChunk.height = m_dimension.Y * m_interS;
            rootChunk.originX = -((m_dimension.X / 2) * m_interS) + (((float) r.Next())%100)/10;
            rootChunk.originY = -((m_dimension.Y / 2) * m_interS);
            rootChunk.entity = new Grass[1];
            rootChunk.entity[0] = new Grass(0.1f, 0.9f, 0.8f, 100, m_roots, m_numberOfRootElements);
            //rootChunk.entity[0].Init(device, effectName, textureFormats);
            root.data = rootChunk;
            //LODNode<Grass> testn = new LODNode<Grass>();

            GrassNodesLoader loader = new GrassNodesLoader();
            loader.InitWithMaximalDepth(ref root, 4);

            m_rootsNode = root;
        }

        public override void CreateIndexBuffer()
        {

            m_indexCount = ((m_dimension.X - 1)*(m_dimension.Y - 1)) * 6; // -1 because points --> quads ; 6 indices per quad
            m_indexBuffer = InitIndexBuffer();
            UInt32[] indices = new UInt32[m_indexCount];

            int count = 0;
            for (UInt32 y = 0; y < m_dimension.Y - 1; y++)
            {

                for (UInt32 x = 0; x < m_dimension.X - 1; x++)
                {
                    indices[count] = y * (UInt32)m_dimension.X + x;
                    count++;
                    indices[count] = y * (UInt32)m_dimension.X + x + (UInt32)m_dimension.Y + 1;
                    count++;
                    indices[count] = y * (UInt32)m_dimension.X + x + 1;
                    count++;
                    indices[count] = y * (UInt32)m_dimension.X + x;
                    count++;
                    indices[count] = (y + 1) * (UInt32)m_dimension.X + x;
                    count++;
                    indices[count] = y * (UInt32)m_dimension.X + x + (UInt32)m_dimension.Y + 1;
                    count++;
                }
            }

            DataStream stream = m_indexBuffer.Map(MapMode.WriteDiscard, MapFlags.None);
            stream.WriteRange<UInt32>(indices);
            m_indexBuffer.Unmap();
        }
    }
}
