using System;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class BoxExpression : Expression
	{
		Expression m_expr;

		public BoxExpression(Expression expr, Location loc)
		{
			m_expr = expr;
			Location = loc;
			this.ExpressionType = typeof(object);
		}

		public Expression Expr
		{
			get { return m_expr; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "Box {0}", this.ExpressionType);

			m_expr.PrintTree(indent + 2);
		}

		public override void EmitExpression(EmitContext ec)
		{
			m_expr.EmitExpression(ec);

			ec.IlGen.Emit(OpCodes.Box, m_expr.ExpressionType);
		}
	}
}
