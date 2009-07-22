using System;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class UnboxExpression : Expression
	{
		Expression m_expr;

		public UnboxExpression(Type targetType, Expression expr, Location loc)
		{
			this.ExpressionType = targetType;
			m_expr = expr;
			Location = loc;
		}

		public Expression Expr
		{
			get { return m_expr; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "Unbox {0}", this.ExpressionType);

			m_expr.PrintTree(indent + 2);
		}

		public override void EmitExpression(EmitContext ec)
		{
			m_expr.EmitExpression(ec);

			ec.IlGen.Emit(OpCodes.Unbox_Any, this.ExpressionType);
		}
	}
}
