#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using Gel.Compiler.Ast;

#endregion

namespace Gel.Compiler.Graph
{
    class ControlFlowGraph: Graph<AstNode>
    {
		private GraphNode<AstNode> m_rootNode;
		private GraphNode<AstNode> m_endNode;

		public ControlFlowGraph()
		{
			m_rootNode = CreateNode(null);
			m_endNode = CreateNode(null);
		}

		public GraphNode<AstNode> GetRootNode()
		{
            return m_rootNode;
        }

		public GraphNode<AstNode> GetEndNode()
		{
			return m_endNode;
		}
    }
}
