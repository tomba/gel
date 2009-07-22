using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class PostfixExpression : Expression
	{
		Operator m_op;
		Expression m_exp;

		public enum Operator
		{
			None,
			Inc,
			Dec
		}

		public PostfixExpression(Operator op, Expression exp, Location loc)
		{
			m_op = op;
			m_exp = exp;
			Location = loc;
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "Postfix {0} {1}", m_op, this.ExpressionType);
			m_exp.PrintTree(indent+2);
		}

		public override Expression ResolveExpression()
		{
			m_exp = m_exp.ResolveExpression();
			this.ExpressionType = m_exp.ExpressionType;

			if (!(m_exp is IAssignable))
			{
				throw new CompileException("Postfix operator on non-lvalue", this.Location);
			}

			if (m_exp.ExpressionType != typeof(int))
			{
				throw new NotImplementedException();
			}

			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			// XXX Ugly and broken. Evaluates m_exp two times
			LocalBuilder tempLocal = ec.IlGen.DeclareLocal(m_exp.ExpressionType);

			m_exp.EmitExpression(ec);

			IAssignable lvalue = (IAssignable)m_exp;
			lvalue.EmitAssignment(ec, false, 
				new ArithmeticExpression(m_op == Operator.Inc ? ArithmeticExpression.Operator.Plus : ArithmeticExpression.Operator.Minus, m_exp, 
						new IntegerLiteralExpression(1, this.Location), 
					this.Location));

		}
	}
}
