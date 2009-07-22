using System;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class AssignmentExpression : BinaryExpression
	{
		public AssignmentExpression(Expression left, Expression right, Location loc)
		{
			m_left = left;
			m_right = right;
			Location = loc;
		}

		public override string ToString()
		{
			return string.Format("AssignmentExpression {0}", this.ExpressionType);
		}

		public override void PrintTree(int indent)
		{
			Output(indent, ToString());
			m_left.PrintTree(indent+2);
			m_right.PrintTree(indent+2);
		}

		public override Expression ResolveExpression()
		{
			m_right = m_right.ResolveExpression();

			// We only create a new var automatically in the simple x = <expr> case
			if (m_left is SimpleName)
				ResolveContext.TypeHint = m_right.ExpressionType;

			m_left = m_left.ResolveExpression();

			ResolveContext.TypeHint = null;

			if (!TypeCheckImplicit(m_left.ExpressionType, ref m_right))
			{
				throw new CompileException("Type mismatch in assign {0}, {1}", this, m_left.ExpressionType, m_right.ExpressionType);
			}

			this.ExpressionType = m_left.ExpressionType;

			if (!(m_left is IAssignable))
			{
				throw new CompileException("Assigning to a non-lvalue", this);
			}

			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			EmitAssignment(ec, true);			
		}

		public override void EmitStatement(EmitContext ec)
		{
			EmitAssignment(ec, false);
		}

		void EmitAssignment(EmitContext ec, bool isExpression)
		{
			IAssignable lvalue = (IAssignable)m_left;
			lvalue.EmitAssignment(ec, isExpression, m_right);
		}
	}
}
