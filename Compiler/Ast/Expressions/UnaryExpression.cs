
using System.Reflection.Emit;
using System;
namespace Gel.Compiler.Ast
{
	class UnaryExpression : Expression
	{
		private Operator m_op;
		private Expression m_expr;

		public enum Operator
		{
			None,
			Plus,
			Minus,
			LNot,
			BNot,
		}

		public UnaryExpression(Operator op, Expression expr, Location loc)
		{
			m_op = op;
			m_expr = expr;
			Location = loc;
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "Unary({0})", m_op.ToString());
			m_expr.PrintTree(indent + 2);
		}

		public override Expression ResolveExpression()
		{
			m_expr = m_expr.ResolveExpression();

			switch (m_op)
			{
				case Operator.Plus:
				case Operator.Minus:
					// xxx
					if (m_expr.ExpressionType == typeof(int))
					{
						this.ExpressionType = m_expr.ExpressionType;
						return this;
					}

					break;

				case Operator.LNot:
					if (m_expr.ExpressionType == typeof(bool))
					{
						this.ExpressionType = m_expr.ExpressionType;
						return this;
					}

					break;

				case Operator.BNot:
					// xxx
					if (m_expr.ExpressionType == typeof(int))
					{
						this.ExpressionType = m_expr.ExpressionType;
						return this;
					}

					break;
			}

			throw new CompileException("failuRE!", m_expr.Location);
		}

		public override void EmitExpression(EmitContext ec)
		{
			switch (m_op)
			{
				case Operator.Plus:
					m_expr.EmitExpression(ec);
					break;

				case Operator.Minus:
					m_expr.EmitExpression(ec);
					ec.IlGen.Emit(OpCodes.Neg);
					break;

				case Operator.LNot:
					m_expr.EmitExpression(ec);
					ec.IlGen.Emit(OpCodes.Ldc_I4_1);
					ec.IlGen.Emit(OpCodes.Xor);
					break;

				case Operator.BNot:
					m_expr.EmitExpression(ec);
					ec.IlGen.Emit(OpCodes.Not);
					break;

				case Operator.None:
					throw new Exception("kala");
			}
		}

		public override void EmitConditionalStatement(EmitContext ec, Label? trueLabel, Label? falseLabel)
		{
			if (m_op != Operator.LNot)
				throw new CompileException("Emitting conditional code when not unary expr is not LNot", Location);

			m_expr.EmitConditionalStatement(ec, falseLabel, trueLabel);
		}
	}
}
