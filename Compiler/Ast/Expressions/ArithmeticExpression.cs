using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class ArithmeticExpression : BinaryExpression
	{
		private Operator		m_op;

		public enum Operator
		{
			None,
			Plus,
			Minus,
			Mul,
			Div,
			Mod,
			BAnd,
			BXor,
			BOr,
			Shl,
			Shr
		}

		public ArithmeticExpression(Operator op, Expression e1, Expression e2, Location loc)
		{
			m_op = op;
			m_left = e1;
			m_right = e2;
			Location = loc;
		}

		public Operator Oper
		{
			get
			{
				return m_op;
			}
		}


		public override string ToString()
		{
			return String.Format("{0} {1}", m_op.ToString(), this.ExpressionType);
		}

		public override void PrintTree(int indent)
		{
			Output(indent, ToString());
			m_left.PrintTree(indent+2);
			m_right.PrintTree(indent+2);
		}

		public override Expression ResolveExpression()
		{
			m_left = m_left.ResolveExpression();
			m_right = m_right.ResolveExpression();

			if (!TypeCheck(ref m_left, ref m_right))
			{
				throw new CompileException("Type mismatch {0}, {1}", this, m_left.ExpressionType, m_right.ExpressionType);
			}

			this.ExpressionType = m_left.ExpressionType;

			// Simple constant folding
			if (m_left is IntegerLiteralExpression && m_right is IntegerLiteralExpression)
			{
				int a = ((IntegerLiteralExpression)m_left).Number;
				int b = ((IntegerLiteralExpression)m_right).Number;

				if (m_op == Operator.Plus)
					return new IntegerLiteralExpression(a + b, m_left.Location);
				else if (m_op == Operator.Minus)
					return new IntegerLiteralExpression(a - b, m_left.Location);
				else if (m_op == Operator.Mul)
					return new IntegerLiteralExpression(a * b, m_left.Location);
				else if (m_op == Operator.Div)
					return new IntegerLiteralExpression(a / b, m_left.Location);
			}

			if (this.ExpressionType == typeof(string))
			{
				List<Expression> args = new List<Expression>();
				args.Add(m_left);
				args.Add(m_right);

				MethodInfo mi = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });

				return new MethodCallExpression(mi, null, args, this.Location);
			}
			else
			{
				return this;
			}
		}

		public override void EmitExpression(EmitContext ec)
		{
			m_left.EmitExpression(ec);
			m_right.EmitExpression(ec);

			switch (m_op)
			{
				case ArithmeticExpression.Operator.Plus:
					ec.IlGen.Emit(OpCodes.Add);
					break;
				case ArithmeticExpression.Operator.Minus:
					ec.IlGen.Emit(OpCodes.Sub);
					break;
				case ArithmeticExpression.Operator.Mul:
					ec.IlGen.Emit(OpCodes.Mul);
					break;
				case ArithmeticExpression.Operator.Div:
					ec.IlGen.Emit(OpCodes.Div);
					break;
				case ArithmeticExpression.Operator.Mod:
					ec.IlGen.Emit(OpCodes.Rem);
					break;
				case ArithmeticExpression.Operator.BAnd:
					ec.IlGen.Emit(OpCodes.And);
					break;
				case ArithmeticExpression.Operator.BXor:
					ec.IlGen.Emit(OpCodes.Xor);
					break;
				case ArithmeticExpression.Operator.BOr:
					ec.IlGen.Emit(OpCodes.Or);
					break;
				case ArithmeticExpression.Operator.Shl:
					ec.IlGen.Emit(OpCodes.Shl);
					break;
				case ArithmeticExpression.Operator.Shr:
					ec.IlGen.Emit(OpCodes.Shr);
					break;

				default:
					throw new Exception("Unimlemented");
			}

		}
	}
}
