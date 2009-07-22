using System;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class TypeCastExpression : Expression
	{
		Expression m_expr;

		public TypeCastExpression(Type destType, Expression expr, Location loc)
		{
			m_expr = expr;
			this.ExpressionType = destType;
			Location = loc;
		}

		public Expression Expr
		{
			set { m_expr = value; }
			get { return m_expr; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "TypeCast {0}", this.ExpressionType);

			m_expr.PrintTree(indent + 2);
		}

		public override Expression ResolveExpression()
		{
			m_expr = m_expr.ResolveExpression();

			if (this.ExpressionType == m_expr.ExpressionType)
			{
				// Do nothing
				return m_expr;
			}

			if (m_expr.ExpressionType.IsValueType)
			{
				if (this.ExpressionType.IsValueType)
				{
					// value -> value, conversion
					return new TypeConversionExpression(this.ExpressionType, m_expr, this.Location);
				}
				else if (this.ExpressionType == typeof(object))
				{
					// value -> object, box
					return new BoxExpression(m_expr, this.Location);
				}
				else
				{
					// value -> class
					throw new NotImplementedException("cast not implemented");
				}
			}
			else
			{
				if (this.ExpressionType.IsValueType)
				{
					// class -> value, unbox
					return new UnboxExpression(this.ExpressionType, m_expr, this.Location);
				}
				else
				{
					// class -> class, castclass
					return this;
				}
			}
		}

		public override void EmitExpression(EmitContext ec)
		{
			m_expr.EmitExpression(ec);

			ec.IlGen.Emit(OpCodes.Castclass, this.ExpressionType);
		}
	}
}
