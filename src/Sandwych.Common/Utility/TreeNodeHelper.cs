using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Utility
{

    /// <summary>
    /// 用于将字符串构成的路径切分添加到 NodeType 组成的树结构中
    /// </summary>
    /// <typeparam name="NodeType"></typeparam>
    public sealed class TreeNodeMaker<NodeType> where NodeType : class
    {
        public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);

        private Func<NodeType, string, NodeType> m_findChildNode;
        private Func<NodeType, string, NodeType> m_addChildNode;
        private char m_separator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findChildNodeProc">查找特定子节点的方法（只是第一级，非递归）</param>
        /// <param name="addChildNodeProc">添加子节点的方法</param>
        /// <param name="separator">路径分隔符</param>
        public TreeNodeMaker(Func<NodeType, string, NodeType> findChildNodeProc, 
            Func<NodeType, string, NodeType> addChildNodeProc, char separator )
        {
            m_addChildNode = addChildNodeProc;
            m_findChildNode = findChildNodeProc;
            m_separator = separator;
        }

        public void InsertNode(NodeType root, string path)
        {
            InsertNode(root, path, null);
        }

        public void InsertNode(NodeType root, string path,
            Action<NodeType> leafProc)
        {
            var node = root;
            int begin = 0, len = 0;
            for (int p = 0; p <= path.Length; p++)
            {

                if (p == path.Length || path[p] == '/')
                {
                    len = p - begin;
                    //插入树
                    node = InsertChildNode(node, path.Substring(begin, len));                   
                    begin = p + 1;
                }
                if (leafProc != null) leafProc(node);
            }
        }

        private NodeType InsertChildNode(NodeType parent, string text)
        {
            var node = m_findChildNode(parent, text);
            if (node == null) 
                node = m_addChildNode(parent, text);

            return node;
        }
    }
}