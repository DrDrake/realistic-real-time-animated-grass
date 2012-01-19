using System;
using System.Reflection;
using System.Drawing;

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
    class Node
    {
        public Node[] children;
        public Vector3 m_topLeft, m_topRight, m_bottomLeft, m_bottomRight;
        public Node()
        {
            m_topLeft = new Vector3();
            m_topRight = new Vector3();
            m_bottomLeft = new Vector3();
            m_bottomRight = new Vector3();
            children = new Node[4];
        }
        public Node NodeWithBounds(int originX, int originY, int maxX, int maxY)
        {
            Node node = new Node();
            node.m_topLeft = new Vector3(originX, maxY, 0);
            node.m_topRight = new Vector3(maxX, maxY, 0);
            node.m_bottomLeft = new Vector3(originX, originY, 0);
            node.m_bottomRight = new Vector3(maxX, originY, 0);
            return node;
        }
    }
}
