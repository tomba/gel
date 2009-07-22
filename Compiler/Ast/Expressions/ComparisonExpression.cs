using System;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class ComparisonExpression : BinaryExpression
	{
		private Operator	m_op;

		public enum Operator
		{
			None,
			EQ,
			NE,
			GT,
			LT,
			GE,
			LE
		}
		
		public ComparisonExpression(Operator op, Expression e1, Expression e2, Location loc)
		{
			m_op = op;
			this.Left = e1;
			this.Right = e2;
			Location = loc;
		}

		public Operator Oper
		{
			get
			{
				return m_op;
			}
		}

		public Operator ReverseOper
		{
			get
			{
				return NegateOperator(m_op);
			}
		}

		Operator NegateOperator(Operator op)
		{
			switch (op)
			{
				case Operator.EQ:
					return Operator.NE;

				case Operator.NE:
					return Operator.EQ;

				case Operator.GT:
					return Operator.LE;

				case Operator.LT:
					return Operator.GE;

				case Operator.GE:
					return Operator.LT;

				case Operator.LE:
					return Operator.GT;

				default:
					throw new Exception("undefined operator");
			}
		}

		public static string GetOperatorMethodName(Operator oper)
		{
			switch (oper)
			{
				case Operator.EQ:
					return "op_Equality";

				case Operator.NE:
					return "op_Inequality";

				case Operator.GT:
					return "op_GreaterThan";

				case Operator.LT:
					return "op_LesserThan";

				case Operator.GE:
					return "op_GreateThanOrEqual";

				case Operator.LE:
					return "op_LesserThanOrEqual";

				default:
					throw new Exception("undefined operator");
			}
		}

  		public override string ToString()
		{
			return String.Format("Comparison " + m_op.ToString() + " {0}", this.ExpressionType);
		}

		public override void PrintTree(int indent)
		{
			Output(indent, ToString());
			this.Left.PrintTree(indent + 2);
			this.Right.PrintTree(indent + 2);
		}

		public override Expression ResolveExpression()
		{
			m_left = m_left.ResolveExpression();
			m_right = m_right.ResolveExpression();

			if (!TypeCheck(ref m_left, ref m_right))
			{
				throw new CompileException("Comparison type mismatch", this.Location);
			}

			MethodInfo mi;

			if (m_left.ExpressionType.IsPrimitive && m_right.ExpressionType.IsPrimitive)
			{
				this.ExpressionType = typeof(bool);
				return this;
			}
			else if ( ((mi = m_left.ExpressionType.GetMethod(ComparisonExpression.GetOperatorMethodName(m_op))) != null) ||
				((mi = m_right.ExpressionType.GetMethod(ComparisonExpression.GetOperatorMethodName(m_op))) != null) )
			{
				List<Expression> args = new List<Expression>();
				args.Add(m_left);
				args.Add(m_right);
				MethodCallExpression call = new MethodCallExpression(mi, null, args, this.Location);

				call.ExpressionType = typeof(bool);

				ComparisonExpression comp = new ComparisonExpression(ComparisonExpression.Operator.EQ,
					call,
					new BooleanLiteralExpression(true, this.Location),
					this.Location);

				comp.ExpressionType = typeof(bool);

				return comp;
			}

				/* 
				 works, but disabled for now. dynamic op should be implemented in every operation...
			else if (m_left.Type == typeof(UnknownType) && m_right.Type == typeof(UnknownType))
			{
				List<Expression> args = new List<Expression>();
				args.Add(m_left);
				args.Add(m_right);
				MethodCall call = new MethodCall(typeof(Runtime).GetMethod("DynamicCompare"),
					null, args, this.Location);

				call.Type = typeof(bool);

				ComparisonExpression comp = new ComparisonExpression(ComparisonExpression.Operator.EQ,
					call,
					new BooleanLiteralExpression(true, this.Location),
					this.Location);

				comp.Type = typeof(bool);

				return comp;
			}
				 */
			else
			{
				throw new CompileException("Comparison not implemented", this.Location);
			}

		}

		public override void EmitConditionalStatement(EmitContext ec, 
			Nullable<Label> trueLabel, Nullable<Label> falseLabel)
		{
			m_left.EmitExpression(ec);
			m_right.EmitExpression(ec);

			if (trueLabel.HasValue)
			{
				EmitBranch(ec, m_op, trueLabel.Value);

				if (falseLabel.HasValue)
				{
					ec.IlGen.Emit(OpCodes.Br, falseLabel.Value);
				}
			}
			else if (falseLabel.HasValue)
			{
				EmitBranch(ec, NegateOperator(m_op), falseLabel.Value);
			}
		}

		private void EmitBranch(EmitContext ec, Operator op, Label label)
		{
			switch (op)
			{
				case Operator.EQ:
					{
						ec.IlGen.Emit(OpCodes.Beq, label);
						break;
					}

				case Operator.NE:
					{
						ec.IlGen.Emit(OpCodes.Bne_Un, label);
						break;
					}

				case Operator.LT:
					{
						ec.IlGen.Emit(OpCodes.Blt, label);
						break;
					}

				case Operator.GT:
					{
						ec.IlGen.Emit(OpCodes.Bgt, label);
						break;
					}

				case Operator.LE:
					{
						ec.IlGen.Emit(OpCodes.Ble, label);
						break;
					}

				case Operator.GE:
					{
						ec.IlGen.Emit(OpCodes.Bge, label);
						break;
					}

				default:
					{
						throw new Exception("asda");
					}
			}
		}

	}
}
