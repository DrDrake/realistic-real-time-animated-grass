using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

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

/*
 * Synopsis:
 *  GrassRootsLoader is a class used to create an arbitary amount of random dots uppon any given highmap surface.
 * 
 *  GrassRootsSelector is a class used to iterate through an tree structure of GrassRootsChunk elements, and select an apropriate amount of chunks to render.
 *  
 * Notes:
 * The class Highmap uses an instance of GrassRootsLoader in its method prepareGrassNodes().
 * There, via the second value of the method "loader.InitWithMaximalDepth(ref root, 4);", you can define the depth of the chunk tree.
 * The value '1' will create only one chunk, covering the whole map.
 * A value of 2 will create one parent chunk covering the whole map, and 4 child chunks covering a quarter each. And so on.
 * 
 * The class Grass overloads the Draw() method. There it performs an selection of it's GrassRootsChunk tree, using an instance of GrassRootsSelector.
 * The class Grass also kinda implements a constructor, with which it can be used withoud an chunk tree and with a static vertice map instead.
 * Inside the Draw() method, the selection process can be controlled via the contents of the ranges[] array, and the 'highestLODLevel' variable.
 * Both are used in the "selector.LODSelect(ranges, highestLODLevel, m_camera, m_cameraDirection)" method call, which invoces the selection process.
 * 'highestLODLevel' tells the selector what the highest lod level at the beginning is. Basically this means how deep the tree will be iterated.
 * An 'highestLODLevel' of 2 will go through 3 layers of chunks. An chunk at lod level 0 is forced to render and cannot search its childs.
 * The ranges[] array defines a range around the given camera position parameter. 'ranges[highestLODLevel]' defines the highest possible range from within rendering may occur.
 * Only chunks intersecting their ranges[lodLevel] can be drawn. The number of elements in ranges[] may never be below 'highestLODLevel'.
 * Each additional element after ranges[highestLODLevel] allow the selection of one more level of children. (Which means more details).
 * - If you create a tree of depth X, you should also define an ranges[] array of size X, and start with a 'highestLODLevel' of X-1 (-1 because its including 0).
 * 
 * Each GrassRootsChunk contains a (kinda sorta) pointer to an Grass class, which is used for rendering the chunk data.
 * These Grass classes, including their vertex buffers, are instantiate on demand. (Instantiating every node of the tree immediately would take an unacceptable amount of time)
 * To speed up the creating process, the Grass class implements a (kinda sorta) texture cache.
 * Also, after a GrassRootsChunk has initialized its instance of the Grass class, it never disposes it during runtime! Which in turn means, 
 * that it is instantly ready when its needed to render a second time. This is sort of helpfull.
 */

namespace RealtimeGrass.Entities
{
    struct GrassRootsChunk
    {
        //public Vector3[] roots;
        public ArrayList[,] roots;
        public float width;
        public float height;
        public float originX;
        public float originY;
        public Grass[] entity;
    }

    class LODNode<C>
    {
        LODNode<C>[] m_parent;
        LODNode<C>[] m_children;
        C m_data;

        public LODNode<C>[] parent { get { return m_parent; } set { m_parent = value; } }
        public LODNode<C>[] children { get { return m_children; } set { m_children = value; } }
        public C data { get { return m_data; } set { m_data = value; } }

        public LODNode()
        {
            parent = new LODNode<C>[1];
            children = new LODNode<C>[4];
        }
    }

    class GrassNodesLoader
    {
        private int m_maximalDepth;
        public GrassNodesLoader()
        {
        }

        public void InitWithMaximalDepth(ref LODNode<GrassRootsChunk> rootNode, int maximalDepth)
        {
            System.Diagnostics.Debug.Assert(rootNode.data.height == rootNode.data.width);
            m_maximalDepth = maximalDepth;
            Traverse(ref rootNode, 0);
        }
        private void Traverse(ref LODNode<GrassRootsChunk> node, int depth)
        {
            node.data.entity[0] = new Grass(0.1f, 0.9f, 0.8f, 20, node.data.roots, node.data.roots.Length*node.data.roots[0,0].Count);
            ++depth;
            if (depth > m_maximalDepth) return;
            if (node.data.roots.Length < 10) //If a node has less than 10 roots, it wont be devided into new children.
            {
                //m_lodStages = lodStage;
                return;
            }

            LODNode<GrassRootsChunk> childTR = new LODNode<GrassRootsChunk>();
            GrassRootsChunk rootChunkTR = new GrassRootsChunk();
            rootChunkTR.roots = new ArrayList[node.data.roots.GetLength(0) / 2, node.data.roots.GetLength(1) / 2];
            rootChunkTR.entity = new Grass[1];
            childTR.parent[0] = node;
            LODNode<GrassRootsChunk> childTL = new LODNode<GrassRootsChunk>();
            GrassRootsChunk rootChunkTL = new GrassRootsChunk();
            rootChunkTL.roots = new ArrayList[node.data.roots.GetLength(0) / 2, node.data.roots.GetLength(1) / 2];
            rootChunkTL.entity = new Grass[1];
            childTL.parent[0] = node;
            LODNode<GrassRootsChunk> childBR = new LODNode<GrassRootsChunk>();
            GrassRootsChunk rootChunkBR = new GrassRootsChunk();
            rootChunkBR.roots = new ArrayList[node.data.roots.GetLength(0) / 2, node.data.roots.GetLength(1) / 2];
            rootChunkBR.entity = new Grass[1];
            childBR.parent[0] = node;
            LODNode<GrassRootsChunk> childBL = new LODNode<GrassRootsChunk>();
            GrassRootsChunk rootChunkBL = new GrassRootsChunk();
            rootChunkBL.roots = new ArrayList[node.data.roots.GetLength(0) / 2, node.data.roots.GetLength(1) / 2];
            rootChunkBL.entity = new Grass[1];
            childBL.parent[0] = node;

            int sideLength = (int)(Math.Sqrt(node.data.roots.Length)/5.5);
            if (sideLength <= 0) return;
            int offset = sideLength / 2; //von 0% bis 50%
            //int columns = 0;
            //int cindex = 0;
            //bool b = true;
            //int nodeX = 0;
            //int nodeY = 0;
            for (int indexX = 0; indexX < node.data.roots.GetLength(0); ++ indexX)
            {
                for (int indexY = 0; indexY < node.data.roots.GetLength(1); ++indexY)
                {
                    for (int chunkIndex = 0; chunkIndex < node.data.roots[indexY, indexY].Count; ++ chunkIndex)
                    {
                        if (indexY < node.data.roots.GetLength(1)/2) { //lower left rect
                            if (indexX < node.data.roots.GetLength(0)/2)
                            {
                                //rootChunkBL.roots[indexX, indexY, chunkIndex] = node.data.roots[indexX, indexY, chunkIndex];
                                rootChunkBL.roots[indexX, indexY] = (ArrayList)node.data.roots[indexX, indexY].Clone();
                            }
                        }if (indexY >= node.data.roots.GetLength(1) / 2) //top left rect
                        {
                            if (indexX < node.data.roots.GetLength(0) / 2)
                            {
                                int ny = indexY - (int)Math.Floor(node.data.roots.GetLength(1) / 2.0);
                                if (ny >= rootChunkBR.roots.GetLength(1)) continue;
                                rootChunkTL.roots[indexX, ny] = (ArrayList)node.data.roots[indexX, indexY].Clone();
                            }
                        }
                        if (indexY < node.data.roots.GetLength(1) / 2) //lower right rect
                        {
                            if (indexX >= node.data.roots.GetLength(0) / 2)
                            {
                                int nx = indexX - (int)Math.Floor(node.data.roots.GetLength(0) / 2.0);
                                if (nx >= rootChunkBR.roots.GetLength(0)) continue;
                                rootChunkBR.roots[nx, indexY] = (ArrayList)node.data.roots[indexX, indexY].Clone();
                            }
                        }
                        if (indexY >= node.data.roots.GetLength(1) / 2) //top right rect
                        {
                            if (indexX >= node.data.roots.GetLength(0) / 2)
                            {
                                int nx = indexX - (int)Math.Floor(node.data.roots.GetLength(0) / 2.0);
                                if (nx >= rootChunkBR.roots.GetLength(0)) continue;
                                int ny = indexY - (int)Math.Floor(node.data.roots.GetLength(1) / 2.0);
                                if (ny >= rootChunkBR.roots.GetLength(1)) continue;
                                rootChunkTR.roots[nx, ny] = (ArrayList)node.data.roots[indexX, indexY].Clone();
                            }
                        }
                    }
                    //nodeY++;
                }
                //nodeY = 0;
                //nodeX++;
            }

            //Parent node has maximal details. Now that the Children have overtaken the highest detail lod, we lower the details of the parent!
/*            if (node.data.roots[0, 0].Count > 0)
            {
                Random rand = new Random();
                for (int indexX = 0; indexX < node.data.roots.GetLength(0); ++indexX)
                {
                    for (int indexY = 0; indexY < node.data.roots.GetLength(1); ++indexY)
                    {
                        int rootsCount = node.data.roots[indexX, indexY].Count;
                        int detailBurndownRate = rand.Next(1, rootsCount);
                        if (depth == 2) detailBurndownRate = rootsCount - 1;
                        node.data.roots[indexX, indexY].RemoveRange(0, detailBurndownRate);
                    }
                }
            }
*/
            /*for (int index = node.data.roots.Length / 2; index < node.data.roots.Length; index += sideLength)
            {
                if (b)
                {
                    b = false;
                }
                else
                {
                    b = true;
                    //continue;
                }
                //int l = 0;
                for (int i = index; i < index+sideLength; ++i)
                {
                    //int index = (y * m_dimension.X) + x;
                    //index = ((y + 1) * m_dimension.X) + x;
                    //index = (y * m_dimension.X) + (x + 1);
                    //index = ((y + 1) * m_dimension.X) + (x + 1);
                    //int range = (index+sideLength)-index;
                    //if (l ++ > range*2) break;
                    if (i >= node.data.roots.Length) break;
                    if (cindex >= node.data.roots.Length / 4) break;
                    //if (i >= (index + offset/2)) break;
                    rootChunkBL.roots[cindex] = node.data.roots[i];

                    ++cindex;
                    //break;
                }*/
                
                /*if (index % offset == 0 && index != 0)
                {
                    index += offset; //nächste zeile.
                    //columns++;
                }
                //if (columns >= offset) break;
            }*/
        /*
            double offsetd = Math.Sqrt(node.data.roots.Length)*2;
            int offset = (int)(Math.Ceiling(offsetd) - (4.0/depth)); //-4? Wut?
            int chunkIndex = 0;
            for (UInt32 index = 0; index < node.data.roots.Length / 2; ++index)
            {
                ///rootChunkBL.roots[chunkIndex] = node.data.roots[index];
                ///rootChunkBR.roots[chunkIndex] = node.data.roots[index + offset / 2];
                //rootChunkTR.roots[chunkIndex] = node.data.roots[index + node.data.roots.Length / 2];
                //rootChunkTL.roots[chunkIndex] = node.data.roots[index + ((node.data.roots.Length / 2) - (offset / 2))];

                if (index % (offset/2) == 0 && index != 0)
                {
                    index += (UInt32)(offset/2);
                }
                ++chunkIndex;
                if (chunkIndex == node.data.roots.Length / 4)
                {
                    break;
                }
            }
            chunkIndex = 0;
            for (int index = node.data.roots.Length / 2; index < node.data.roots.Length; ++index)
            {
                //rootChunkBL.roots[chunkIndex] = node.data.roots[index];
                //rootChunkBR.roots[chunkIndex] = node.data.roots[index + offset / 2];
                //rootChunkTR.roots[chunkIndex] = node.data.roots[index - offset / 2];
                //rootChunkTL.roots[chunkIndex] = node.data.roots[index];

                if (index % (offset / 2) == 0 && index != 0)
                {
                    index += (int)(offset / 2);
                }
                ++chunkIndex;
                if (chunkIndex == node.data.roots.Length / 4)
                {
                    break;
                }
            }
            */
/*
            double size = Math.Sqrt(node.data.roots.Length)*2;
            if (size % 2 != 0) // Rest?
            {
                size += 1.0;
            }
            size -= 4.0/depth;
            int page = (int)size;//Ceiling
            int chunkIndex = 0;
            for (UInt32 index = 0; index < node.data.roots.Length / 2; ++index)
            {
                if (index % (page / 2) == 0 && index != 0)
                {
                    index += (UInt32)(page / 2);
                }
                rootChunkBL.roots[chunkIndex] = node.data.roots[index];
                rootChunkBR.roots[chunkIndex] = node.data.roots[index + page / 2];
                rootChunkTR.roots[chunkIndex] = node.data.roots[index + node.data.roots.Length / 2];
                rootChunkTL.roots[chunkIndex] = node.data.roots[index + ((node.data.roots.Length / 2) - (page / 2))];

                
                ++chunkIndex;
                if (chunkIndex == node.data.roots.Length / 4)
                {
                    break;
                }
            }
*/
            rootChunkTR.originX = node.data.originX + node.data.width / 2;
            rootChunkTR.originY = node.data.originY + node.data.height / 2;
            rootChunkTR.width = node.data.width / 2;
            rootChunkTR.height = node.data.height / 2;
            childTR.data = rootChunkTR;
            rootChunkTL.originX = node.data.originX;
            rootChunkTL.originY = node.data.originY + node.data.height / 2;
            rootChunkTL.width = node.data.width / 2;
            rootChunkTL.height = node.data.height / 2;
            childTL.data = rootChunkTL;
            rootChunkBR.originX = node.data.originX + node.data.width / 2;
            rootChunkBR.originY = node.data.originY;
            rootChunkBR.width = node.data.width / 2;
            rootChunkBR.height = node.data.height / 2;
            childBR.data = rootChunkBR;
            rootChunkBL.originX = node.data.originX;
            rootChunkBL.originY = node.data.originY;
            rootChunkBL.width = node.data.width / 2;
            rootChunkBL.height = node.data.height / 2;
            childBL.data = rootChunkBL;

            Traverse(ref childTR, depth);
            node.children[0] = childTR;
            Traverse(ref childTL, depth);
            node.children[1] = childTL;
            Traverse(ref childBR, depth);
            node.children[2] = childBR;
            Traverse(ref childBL, depth);
            node.children[3] = childBL;
        }
    }
    /*
    public struct Circle
    {
        public Vector3 center;
        public float radius;
        public Circle(Vector3 c, float r)
        {
            center = c;
            radius = r;
        }
    }
    public struct Rect
    {
        public float a, b, c, d;
        public Rect(float newA, float newB, float newC, float newD)
        {
            a = newA;
            b = newB;
            c = newC;
            d = newD;
        }
    }
    */
    class GrassNodesSelector
    {
        public static List<LODNode<GrassRootsChunk>> selectionList;
        private LODNode<GrassRootsChunk> currentNode;
        public GrassNodesSelector(LODNode<GrassRootsChunk> root)
        {
            selectionList = new List<LODNode<GrassRootsChunk>>();
            currentNode = root;
        }

        public bool LODSelect(int[] ranges, int lodLevel, Vector3 camera, Vector3 cameraDirection)
        {
            if (!IntersectsSphere(ranges[lodLevel], camera))
            {
                //This node is out of sight. We do not add it to the selection list, but mark it as successfully processed.
                return true;
            }
            if (!IntersectsFrustum(camera, cameraDirection))
            {
                //This is not yet real frustum culling. Its just a check if the node is behind the camera. If so, it is not added to the selection list.
                return true;
            }
            if (lodLevel == 0) // @HACK This is a temporary hack. Should be 0.
            {
                //We reached the last node with the highest avaliable detail.
                AddToSelectionList();
                return true;
            }
            else
            {/*
                if (!IntersectsSphere(ranges[lodLevel - 1], camera)) // @HACK This is a temporary commented so that only lodLevel 4 will be drawn.
                {
                    //This node is the last node covering the area, so we need to render it.
                    AddToSelectionList();
                }
                else //This node is too big an has children which are better suited for selection.
                {*/
                foreach (LODNode<GrassRootsChunk> childNode in currentNode.children)
                    {
                        if (childNode == null)
                        {
                            AddNodeToSelectionList(currentNode);
                            return true;
                        }
                        currentNode = childNode;
                        if (!LODSelect(ranges, lodLevel - 1, camera, cameraDirection))
                        {
                            //This child node is maybe behin camera or to far away for its given lodLevel.
                            //Ideally we would copy the area covered by the child node with the grass roots from this node. But instead we just use the child.
                            AddNodeToSelectionList(childNode);
                        }
                    }
                //}
            }
            return true;
        }
        private void AddToSelectionList()
        {
            AddNodeToSelectionList(currentNode);
        }
        private void AddNodeToSelectionList(LODNode<GrassRootsChunk> node)
        {
            selectionList.Add(node);
        }
        private bool IntersectsSphere(int radius, Vector3 center)
        {
            //Ignore the height. Instead of spheres, we calculate with circles. Circles are the shit!
            float distX = Math.Abs(center.X - currentNode.data.originX - currentNode.data.width/2);
            float distY = Math.Abs(center.Z - currentNode.data.originY - currentNode.data.height/2);

            if (distX > (currentNode.data.width/2 + radius)) { return false; }
            if (distY > (currentNode.data.height/2 + radius)) { return false; }

            if (distX <= (currentNode.data.width/2)) { return true; }
            if (distY <= (currentNode.data.height/2)) { return true; }
            
            double cornerDist = Math.Pow(distX - currentNode.data.width/2, 2) + Math.Pow(distY - currentNode.data.height/2, 2);
            return (cornerDist <= Math.Pow(radius, 2));
        }
        private bool IntersectsFrustum(Vector3 camera, Vector3 cameraDirection)
        {
            //Check if currentNode is behind camera, using cameraDirection.
            return true;
        }
        /*private bool PointInRectangle(Vector3 point, Rect rect)
        {
            Vector2 p = new Vector2(point.X, point.Z);
            // D --- C
            // +     +
            // A --- B
            Vector2 a = new Vector2(rect.a, rect.b);
            Vector2 b = new Vector2(rect.a + rect.c, rect.b);
            Vector2 c = new Vector2(rect.a + rect.c, rect.b + rect.d);
            Vector2 d = new Vector2(rect.a, rect.b + rect.d);
            //0 ≤ AP·AB ≤ AB·AB and 0 ≤ AP·AD ≤ AD·AD
            if (((0 <= Vector2.Dot(a + p, a + b)) && (0 <= Vector2.Dot(a+b, a+b))) && ((0 <= Vector2.Dot(a+p, a+d)) && (0 <= Vector2.Dot(a+d, a+d))))
            {
                return true;
            }
            return false;
        }
        private bool intersectingCircles(Circle c1, Circle c2)
        {
            Vector2 cp1 = new Vector2(c1.center.X, c1.center.Y);
            Vector2 cp2 = new Vector2(c2.center.X, c2.center.Y);
            float distance = Vector2.Distance(cp1, cp2);
            if (distance > c1.radius + c1.radius)
            {
                return false;
            }
            return true;
        }*/
    }
}
