using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class ClosureExpression : Expression
	{
		Type m_type;
		string m_name;
		Statement m_body;
		LocalVariable m_var;

		public ClosureExpression(Type type, string name, StatementList body, Location loc)
		{
			m_type = type;
			m_name = name;
			m_body = body;
			this.Location = loc;
		}

		public override Expression ResolveExpression()
		{
			ResolveContext.PushExitType(m_type);

			m_var = ResolveContext.CurrentBlock.FindLocal(m_name);

			if (m_body != null)
				m_body = m_body.ResolveStatement();

			ResolveContext.PopExitType();

			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			Label retLabel = ec.IlGen.DefineLabel();
			ResolveContext.PushExitLabel(new GelLabel(retLabel));

			m_body.EmitStatement(ec);

			// fallthrough
			ec.IlGen.Emit(OpCodes.Ldloc, m_var.LocalBuilder);

			ec.IlGen.MarkLabel(retLabel);

			ResolveContext.PopExitLabel();
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "Closure({0})", m_type.Name);

			if (m_body != null)
				m_body.PrintTree(indent + 2);
			
		}
	}
}
