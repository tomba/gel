using System;
using System.Collections.Generic;
using System.Text;

namespace Gel.Compiler.Ast
{
	class LocalDeclaration : Statement
	{
		Type m_type;
		string m_name;
		Expression m_initializer;

		public LocalDeclaration(Type type, string name, Expression initializer, Location loc)
		{
			m_type = type;
			m_name = name;
			m_initializer = initializer;
			this.Location = loc;
		}

		public override Statement ResolveStatement()
		{
			LocalVariable var = ResolveContext.CurrentMethod.CreateLocalVariable(m_name, m_type, this.Location);
			ResolveContext.CurrentBlock.AddLocalVariable(var);
			if (m_initializer != null)
			{
				Statement stm = new ExpressionStatement(new AssignmentExpression(
					new LocalAccessExpression(var), m_initializer, this.Location));
				stm = stm.ResolveStatement();
				return stm;
			}
			else
				return new NopStatement();
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "Local {0} {1}", m_type.Name, m_name);
			if(m_initializer != null)
				m_initializer.PrintTree(indent + 2);
		}
	}
}
