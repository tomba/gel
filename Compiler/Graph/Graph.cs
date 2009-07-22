#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using Gel.Compiler.Ast;

#endregion

namespace Gel.Compiler.Graph
{
    class GraphNode<T> where T: AstNode
    {
		private Graph<T> m_graph;
		private T m_value;
        private List<GraphNode<T>> m_successors = new List<GraphNode<T>>();
        private List<GraphNode<T>> m_predecessors = new List<GraphNode<T>>();

		public GraphNode(Graph<T> graph, T value)
		{
            m_graph = graph;
            m_value = value;
        }
     
        public T AstNode
        {
            get
            {
                return m_value;
            }
        }
        public List<GraphNode<T>> Successors
        {
            get
            {
                return m_successors;
            }
        }
        public List<GraphNode<T>> Predecessors
        {
            get
            {
                return m_predecessors;
            }
        }

        public override string ToString()
        {
            if (m_value == null)
                return "<null>";

			return m_value.ToString();
		}
    }

    class Graph<T> where T: AstNode
    {
		private List<GraphNode<T>> m_nodes = new List<GraphNode<T>>();

		public Graph()
		{
		}

		public GraphNode<T> CreateNode(T node)
		{
            GraphNode<T> graphNode = new GraphNode<T>(this, node);
            m_nodes.Add(graphNode);

            return graphNode;
        }

		public GraphNode<T> CreateNodeAndLink(T data, ref GraphNode<T> lastNode)
		{
			GraphNode<T> newNode = CreateNode(data);
			AddEdge(lastNode, newNode);
			lastNode = newNode;
			return newNode;
		}

		public void AddEdge(GraphNode<T> src, GraphNode<T> dest)
		{
			if (src == null || dest == null)
				return;

			src.Successors.Add(dest);
			dest.Predecessors.Add(src);
		}

		public void Print(int indent)
        {
            int i = 0;

            foreach (GraphNode<T> node in m_nodes)
            {
				String str = String.Format("{0,2}: {1,-40}", i, node);

				str += "pre: ";
				//i + ": " + node + "\t\tpre: ";

				foreach (GraphNode<T> pre in node.Predecessors)
				{

                    str += m_nodes.IndexOf(pre)+" ";
                }

                str += "\tsuc: ";

				foreach (GraphNode<T> suc in node.Successors)
				{
                    str += m_nodes.IndexOf(suc) + " ";
                }

				Gel.Compiler.Output.WriteLine(indent, str);

                i++;
            }
        }
    }
}
