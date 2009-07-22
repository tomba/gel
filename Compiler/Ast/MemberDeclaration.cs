using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using Gel.Core;

namespace Gel.Compiler.Ast
{
	enum MemberType
	{
		Field,
		Method
	}

	abstract class MemberDeclaration : AstNode
	{
		private string m_name;
		private Modifiers m_mods;

		protected MemberDeclaration(string name, Modifiers mods, Location loc)
		{
			m_name = name;
			m_mods = mods;
			Location = loc;
		}

		public string Name
		{
			get { return m_name; }
		}

		public Modifiers Modifiers
		{
			get { return m_mods; }
		}

		public abstract MemberType MemberType
		{
			get;
		}

		public bool IsStatic
		{
			get { return (m_mods & Modifiers.Static) != 0; }
		}

		public bool IsInstance
		{
			get { return (m_mods & Modifiers.Static) == 0; }
		}

		public bool IsConst
		{
			get { return (m_mods & Modifiers.Const) != 0; }
		}

		public abstract void Resolve();
	}
}
