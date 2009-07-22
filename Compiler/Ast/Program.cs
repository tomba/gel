using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Gel.Core;

namespace Gel.Compiler.Ast
{
	class Program : AstNode
	{
		private string m_name;
		private Dictionary<string, MemberDeclaration> m_members = new Dictionary<string, MemberDeclaration>();
		int m_staticFields = 0;
		int m_instanceFields = 0;

		public Program(string name)
		{
			m_name = name;
		}

		public string Name
		{
			get { return m_name; }
		}

		public void CreateField(string name, Modifiers mods, Type fieldType, Expression initExpr, Location loc)
		{
			if (m_members.ContainsKey(name))
			{
				throw new CompileException("duplicate member", loc);
			}

			int ordinal;

			if ((mods & Modifiers.Static) != 0)
			{
				ordinal = m_staticFields++;
			}
			else
			{
				ordinal = m_instanceFields++;
			}

			m_members[name] = new FieldDeclaration(name, mods, fieldType, ordinal, initExpr, loc);
		}

		public FieldDeclaration FindField(string name)
		{
			if (m_members.ContainsKey(name) && m_members[name].MemberType == MemberType.Field)
			{
				return (FieldDeclaration)m_members[name];
			}
			else
			{
				return null;
			}
		}

		public FieldDeclaration[] GetFields()
		{
			List<FieldDeclaration> fields = new List<FieldDeclaration>();

			foreach (KeyValuePair<string, MemberDeclaration> kvp in m_members)
			{
				MemberDeclaration member = kvp.Value;

				if (member.MemberType == MemberType.Field)
				{
					fields.Add((FieldDeclaration)member);
				}
			}

			return fields.ToArray();
		}

		public void AddMethod(MethodDeclaration method)
		{
			if (m_members.ContainsKey(method.Name))
			{
				throw new CompileException("duplicate method", method.Location);
			}

			m_members[method.Name] = method;
		}

		public MethodDeclaration FindMethod(string name)
		{
			if (m_members.ContainsKey(name) && m_members[name].MemberType == MemberType.Method)
			{
				return (MethodDeclaration)m_members[name];
			}
			else
			{
				return null;
			}
		}

		public MethodDeclaration[] GetMethods()
		{
			List<MethodDeclaration> methods = new List<MethodDeclaration>();

			foreach(KeyValuePair<string, MemberDeclaration> kvp in m_members)
			{
				MemberDeclaration member = kvp.Value;

				if (member.MemberType == MemberType.Method)
				{
					methods.Add((MethodDeclaration)member);
				}
			}

			return methods.ToArray();
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "Program {0}", m_name);
			Output(indent, "{{");

			foreach (KeyValuePair<string, MemberDeclaration> kvp in m_members)
			{
				kvp.Value.PrintTree(indent + 2);
			}

			Output(indent, "}}");
		}

		public void Resolve()
		{
			// TODO: make ResolveContext an instance
			ResolveContext.CurrentProgram = this;

			foreach (KeyValuePair<string, MemberDeclaration> kvp in m_members)
			{
				kvp.Value.Resolve();
			}
		}

		public void Emit(EmitContext ec)
		{
			foreach (KeyValuePair<string, MemberDeclaration> kvp in m_members)
			{
				if (kvp.Value.MemberType == MemberType.Method)
				{
					((MethodDeclaration)kvp.Value).CreateMethodInfo();
				}
			}

			foreach (KeyValuePair<string, MemberDeclaration> kvp in m_members)
			{
				if (kvp.Value.MemberType == MemberType.Method)
				{
					((MethodDeclaration)kvp.Value).Emit(ec);
				}
			}
		}
	}
}
