using System;
using System.Collections.Generic;

namespace Gel.Compiler.Ast
{
	class StatementList : Statement
	{
		private List<Statement> m_list = new List<Statement>();

		public StatementList()
		{
		}

		public List<Statement> Statements
		{
			get
			{
				return m_list;
			}
		}

		public void Add(Statement stm)
		{
			m_list.Add(stm);
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "StatementList");

			foreach(Statement stm in m_list)
			{
				if(stm == null)
				{
					Output(indent + 2, "<null statement>");
				}
				else
				{
					stm.PrintTree(indent + 2);
				}
			}
		}

		public override Statement ResolveStatement()
		{
			for (int i = 0; i < m_list.Count; i++)
			{
				m_list[i] = m_list[i].ResolveStatement();
			}

			return this;
		}

		public override void EmitStatement(EmitContext ec)
		{
			foreach (Statement stm in m_list)
			{
				stm.EmitStatement(ec);
			}
		}
	}
}
