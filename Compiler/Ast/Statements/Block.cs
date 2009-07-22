using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class Block : Statement
	{
		private Block m_parent = null;
		private StatementList m_stmList = new StatementList();
		private Dictionary<string, LocalVariable> m_localTable = new Dictionary<string, LocalVariable>();
		Dictionary<string, LocalConst> m_constTable = new Dictionary<string, LocalConst>();

		public Block()
		{
		}

		public Block(Block parent)
		{
			m_parent = parent;
		}

		public Block Parent
		{
			get { return m_parent; }
			set { m_parent = value; }
		}

		public StatementList StatementList
		{
			get { return m_stmList; }
			set { m_stmList = value; }
		}

		public void AddStatement(Statement stmt)
		{
			m_stmList.Add(stmt);
		}

		public void AddLocalVariable(LocalVariable local)
		{
			LocalVariable tmpLocal = FindLocal(local.Name);

			if(tmpLocal != null)
			{
				throw new Exception("local variable already defined");
			}

			m_localTable[local.Name] = local;
		}

		public LocalVariable FindLocal(string name)
		{
			if (m_localTable.ContainsKey(name))
			{
				return m_localTable[name];
			}
			else if (m_parent != null)
			{
				return m_parent.FindLocal(name);
			}
			else
			{
				return null;
			}
		}

		public List<LocalVariable> Locals
		{
			get
			{
				List<LocalVariable> locals = new List<LocalVariable>();
				foreach (KeyValuePair<string, LocalVariable> kvp in m_localTable)
				{
					locals.Add(kvp.Value);
				}

				return locals;
			}
		}

		public void AddLocalConst(LocalConst var)
		{
			m_constTable[var.Name] = var;
		}

		public LocalConst FindLocalConst(string name)
		{
			if (m_constTable.ContainsKey(name))
			{
				return m_constTable[name];
			}
			else if (m_parent != null)
			{
				return m_parent.FindLocalConst(name);
			}
			else
			{
				return null;
			}
		}


		public override void PrintTree(int indent)
		{
			Output(indent, "Block");

			foreach(string name in m_localTable.Keys)
			{
				Output(indent + 2, m_localTable[name].ToString());
			}
			
			m_stmList.PrintTree(indent + 2);
		}

		public override Statement ResolveStatement()
		{
			this.Parent = ResolveContext.CurrentBlock;
			ResolveContext.CurrentBlock = this;

			foreach (KeyValuePair<string, LocalConst> kvp in m_constTable)
			{
				Expression exp = kvp.Value.ValueExpression;
				exp = exp.ResolveExpression();
				kvp.Value.ValueExpression = exp;
				if (!(exp is IntegerLiteralExpression))
				{
					throw new CompileException("const is not literal", exp.Location);
				}
			}

			m_stmList = (StatementList)m_stmList.ResolveStatement();

			ResolveContext.CurrentBlock = this.Parent;

			return this;
		}

		public override void EmitStatement(EmitContext ec)
		{
			// Should this be in methoddeclaration? it knows all the vars too.
			foreach (LocalVariable var in m_localTable.Values)
			{
				if (!var.IsParameter)
				{
					LocalBuilder lb = ec.IlGen.DeclareLocal(var.VariableType);
					if (ec.EmitSymbols)
						lb.SetLocalSymInfo(var.Name);
					var.LocalBuilder = lb;
				}
			}

			m_stmList.EmitStatement(ec);
		}
	}
}
