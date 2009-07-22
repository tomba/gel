#define DYNAMIC_METHOD

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using Gel.Core;

namespace Gel.Compiler.Ast
{
	class MethodDeclaration : MemberDeclaration
	{
		Type m_returnType;
		private List<LocalVariable> m_locals = new List<LocalVariable>();
		private List<LocalVariable> m_params = new List<LocalVariable>();
		private Block m_block = new Block();
		private DynamicMethod m_dynamicMethod;

		public MethodDeclaration(string name, Modifiers mods, Type returnType, Location loc)
			: base(name, mods, loc)
		{
			m_returnType = returnType;
		}

		public override MemberType MemberType
		{
			get { return MemberType.Method; }
		}

		public Type ReturnType
		{
			get { return m_returnType; }
		}

		public Block Block
		{
			get { return m_block; }
			set { m_block = value; }
		}

		public List<LocalVariable> Locals
		{
			get { return m_locals; }
		}

		public List<LocalVariable> Parameters
		{
			get { return m_params; }
		}

		public MethodInfo MethodInfo
		{
			get { return m_dynamicMethod; }
		}

		public LocalVariable CreateParameter(string name, Type type, Location loc)
		{
			int ordinal = m_params.Count;

			if (IsInstance)
			{
				ordinal++;
			}

			LocalVariable local = new LocalVariable(name, type, ordinal, true, loc);
			m_params.Add(local);
			return local;
		}

		public LocalVariable CreateLocalVariable(string name, Type type, Location loc)
		{
			LocalVariable local = new LocalVariable(name, type, m_locals.Count, false, loc);
			m_locals.Add(local);
			return local;
		}

		public void CreateMethodInfo()
		{
			List<Type> paramTypesList = new List<Type>();
		
			if (IsInstance)
			{
				paramTypesList.Add(typeof(GelObject));
			}

			foreach (LocalVariable l in m_params)
			{
				paramTypesList.Add(l.VariableType);
			}
			
			m_dynamicMethod = new DynamicMethod(this.Name, this.ReturnType, paramTypesList.ToArray(), typeof(GelCompiler), true);
			m_dynamicMethod.InitLocals = true;
		}

		public LocalVariable FindParam(string name)
		{
			foreach (LocalVariable l in m_params)
			{
				if (l.Name == name)
				{
					return l;
				}
			}

			return null;
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "Method {0}:{1} ({2})", this.Name, this.ReturnType, this.Modifiers);
			Output(indent, "{{");

			foreach (LocalVariable l in m_params)
			{
				Output(indent + 2, "param {0} {1}", l.VariableType.FullName, l.Name);
			}

			if (m_block != null)
				m_block.PrintTree(indent + 2);

			Output(indent, "}}");
		}

		public override void Resolve()
		{
			Console.WriteLine("Resolving {0}", this.Name);

			ResolveContext.CurrentMethod = this;

			if (m_block.StatementList.Statements.Count == 0 ||
				!(m_block.StatementList.Statements[m_block.StatementList.Statements.Count - 1] is ReturnStatement))
			{
				/* add return. should really do control flow analysis */
				ReturnStatement retStm;

				if (this.ReturnType == typeof(void))
				{
					retStm = new ReturnStatement(null, null);
				}
				else if (this.ReturnType.IsPrimitive)
				{
					retStm = new ReturnStatement(new IntegerLiteralExpression(0, null), null);
				}
				else if (this.ReturnType.IsValueType)
				{
					throw new NotImplementedException();
				}
				else
				{
					retStm = new ReturnStatement(new NullLiteralExpression(null), null);
				}

				m_block.StatementList.Add(retStm);
			}

			m_block = (Block)m_block.ResolveStatement();
		}

		public void Emit(EmitContext ec)
		{
			ec.IlGen = m_dynamicMethod.GetILGenerator();
			m_block.EmitStatement(ec);

			ec.MethodInfo = m_dynamicMethod;
		}
	}
}
