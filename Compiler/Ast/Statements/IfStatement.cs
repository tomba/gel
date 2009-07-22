using System;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class IfStatement : Statement
	{
		Expression m_cond;
		Statement m_trueStm, m_falseStm;

		public IfStatement(Expression condition, Statement trueStm, Statement falseStm, Location loc)
		{
			m_cond = condition;
			m_trueStm = trueStm;
			m_falseStm = falseStm;
			Location = loc;
		}

		public Expression Condition
		{
			get { return m_cond; }
			set { m_cond = value; }
		}

		public Statement TrueStatement
		{
			get { return m_trueStm; }
			set { m_trueStm = value; }
		}

		public Statement FalseStatement
		{
			get { return m_falseStm; }
			set { m_falseStm = value; }
		}

		public override string ToString()
		{
			if (m_falseStm != null)
				return "If-Else";
			else
				return "If";
		}

		public override void PrintTree(int indent)
		{
			Output(indent, ToString());

			m_cond.PrintTree(indent+2);
			m_trueStm.PrintTree(indent+2);
			if(m_falseStm != null)
				m_falseStm.PrintTree(indent+2);
		}

		public override Statement ResolveStatement()
		{
			m_cond = m_cond.ResolveExpression();

			if (m_cond.ExpressionType != typeof(bool))
			{
				throw new CompileException("Condition for if is not boolean", m_cond.Location);
			}

			m_trueStm = m_trueStm.ResolveStatement();
			if (m_falseStm != null)
				m_falseStm = m_falseStm.ResolveStatement();

			return this;
		}

		public override void EmitStatement(EmitContext ec)
		{
			if (m_falseStm != null)
			{
				Label falseLabel = ec.IlGen.DefineLabel();
				Label endLabel = ec.IlGen.DefineLabel();

				m_cond.EmitConditionalStatement(ec, null, falseLabel);

				m_trueStm.EmitStatement(ec);
				ec.IlGen.Emit(OpCodes.Br, endLabel);

				ec.IlGen.MarkLabel(falseLabel);
				m_falseStm.EmitStatement(ec);

				ec.IlGen.MarkLabel(endLabel);
			}
			else
			{
				Label endLabel = ec.IlGen.DefineLabel();

				m_cond.EmitConditionalStatement(ec, null, endLabel);
				m_trueStm.EmitStatement(ec);

				ec.IlGen.MarkLabel(endLabel);
			}
		}
	}
}
