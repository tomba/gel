using System;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class WhileStatement : Statement
	{
		private Expression m_condition;
		private Statement m_stm;

		public WhileStatement(Expression condition, Statement stm, Location loc)
		{
			m_condition = condition;
			m_stm = stm;
			Location = loc;

			if (m_stm == null)
				m_stm = new NopStatement();
		}

		public override Statement ResolveStatement()
		{
			m_condition = m_condition.ResolveExpression();

			if (m_condition.ExpressionType != typeof(bool))
				throw new CompileException("While condition is not boolean", this.Location);

			m_stm = m_stm.ResolveStatement();

			return this;
		}

		public override void EmitStatement(EmitContext ec)
		{
			Label endLabel = ec.IlGen.DefineLabel();
			Label loopLabel = ec.IlGen.DefineLabel();

			ec.IlGen.MarkLabel(loopLabel);

			m_condition.EmitConditionalStatement(ec, null, endLabel);

			m_stm.EmitStatement(ec);

			ec.IlGen.Emit(OpCodes.Br_S, loopLabel);

			ec.IlGen.MarkLabel(endLabel);
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "while");
			m_condition.PrintTree(indent + 2);
			m_stm.PrintTree(indent + 2);
		}

	}
}
