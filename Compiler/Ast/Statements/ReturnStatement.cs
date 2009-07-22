using System;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class ReturnStatement : Statement
	{
		Expression m_exp;

		public ReturnStatement(Expression exp, Location loc)
		{
			m_exp = exp;
			Location = loc;
		}

		public Expression Expr
		{
			get { return m_exp; }
			set { m_exp = value; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "Return");

			if(m_exp != null)
				m_exp.PrintTree(indent+2);
		}

		public override Statement ResolveStatement()
		{
			if (m_exp == null)
				return this;

			m_exp = m_exp.ResolveExpression();

			if (ResolveContext.ExitType == null)
			{
				if (!TypeCheckImplicit(ResolveContext.CurrentMethod.ReturnType, ref m_exp))
					throw new CompileException("Type mismatch {0}, {1}", this, ResolveContext.CurrentMethod.ReturnType, m_exp.ExpressionType);
			}
			else
			{
				if (!TypeCheckImplicit(ResolveContext.ExitType, ref m_exp))
					throw new CompileException("Type mismatch {0}, {1}", this, ResolveContext.ExitType, m_exp.ExpressionType);
			}

			return this;
		}

		public override void EmitStatement(EmitContext ec)
		{
			if(m_exp != null)
				m_exp.EmitExpression(ec);

			// Not sure what happens here, if this return is inside catch
			if (ResolveContext.ExitLabel == null)
				ec.IlGen.Emit(OpCodes.Ret);
			else
				ec.IlGen.Emit(OpCodes.Br, ResolveContext.ExitLabel.Label);
		}
	}
}
