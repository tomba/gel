#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using Gel.Compiler.Ast;

#endregion

namespace Gel.Compiler.Graph.Visitor
{
    class ControlFlowCreateVisitor : IAstVisitor
    {
		ControlFlowGraph m_graph;
		private GraphNode<AstNode> m_lastNode;

		public ControlFlowCreateVisitor(ControlFlowGraph graph)
		{
			m_graph = graph;
		}
        
        public object Visit(MethodDeclaration method)
        {
			m_lastNode = m_graph.GetRootNode();

            DoVisit(method.Block);

            return method;
        }

        public object Visit(Block block)
        {
            DoVisit(block.StatementList);

            return block;
        }

		public object Visit(StatementList stmList)
		{
			foreach (Statement stmt in stmList.Statements)
			{
				DoVisit(stmt);
			}

			return stmList;
		}

		public object Visit(ExpressionStatement expStm)
		{
			DoVisit(expStm.Expr);

			return expStm;
		}

		public object Visit(ReturnStatement stm)
		{
			DoVisit(stm.Expr);

			m_graph.CreateNodeAndLink(stm, ref m_lastNode);

			m_graph.AddEdge(m_lastNode, m_graph.GetEndNode());

			m_lastNode = null;

			return stm;
		}

		public object Visit(IfStatement stm)
		{
			stm.Condition = (Expression)DoVisit(stm.Condition);

			GraphNode<AstNode> ifNode = m_graph.CreateNode(stm);

			m_graph.AddEdge(m_lastNode, ifNode);

			GraphNode<AstNode> joinNode = m_graph.CreateNode(null);

			// true part
			m_lastNode = ifNode;

			stm.TrueStatement = (Statement)DoVisit(stm.TrueStatement);

			m_graph.AddEdge(m_lastNode, joinNode);

			// false part
			m_lastNode = ifNode;

			if (stm.FalseStatement != null)
			{
				stm.FalseStatement = (Statement)DoVisit(stm.FalseStatement);
			}

			m_graph.AddEdge(m_lastNode, joinNode);


			m_lastNode = joinNode;

			return stm;
		}
/*
		public object Visit(ForStatement stm)
		{
			if(stm.InitializerList != null)
				stm.InitializerList = (StatementList)DoVisit(stm.InitializerList);

			if (stm.Condition != null)
			{
				stm.Condition = (Expression)DoVisit(stm.Condition);

				if (stm.Condition.Type != TypeManager.s_bool)
				{
					throw new CompileException("Condition for if is not boolean", stm.Condition.Location);
				}
			}

			if(stm.LoopStatementList != null)
				stm.LoopStatementList = (StatementList)DoVisit(stm.LoopStatementList);

			stm.Body = (Statement)DoVisit(stm.Body);

			return stm;
		}
*/
		/*
		 * Expressions
		 */

		public object Visit(AssignmentExpression exp)
		{
			DoVisit(exp.Left);
			DoVisit(exp.Right);

			m_graph.CreateNodeAndLink(exp, ref m_lastNode);

			return exp;
		}

		public object Visit(ArithmeticExpression exp)
		{
			DoVisit(exp.Left);
			DoVisit(exp.Right);

			m_graph.CreateNodeAndLink(exp, ref m_lastNode);

            return exp;
		}

		public object Visit(LogicalExpression exp)
		{
			DoVisit(exp.Left);
			DoVisit(exp.Right);

			m_graph.CreateNodeAndLink(exp, ref m_lastNode);

			return exp;
		}

		public object Visit(ComparisonExpression exp)
		{
			DoVisit(exp.Left);
			DoVisit(exp.Right);

			m_graph.CreateNodeAndLink(exp, ref m_lastNode);

            return exp;
		}

		public object Visit(IntegerLiteralExpression exp)
		{
			m_graph.CreateNodeAndLink(exp, ref m_lastNode);

            return exp;
		}

		public object Visit(LocalAccessExpression exp)
		{
			m_graph.CreateNodeAndLink(exp, ref m_lastNode);

			return exp;
        }
    }
}
