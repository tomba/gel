using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using Gel.Core;

namespace Gel.Compiler.Ast
{
	class FieldDeclaration : MemberDeclaration
	{
		Type m_fieldType;
		int m_ordinal;
		Expression m_initExpression;

		public FieldDeclaration(string name, Modifiers mods, Type fieldType, int ordinal, 
			Expression initExpr, Location loc) 
			: base(name, mods, loc)
		{
			m_fieldType = fieldType;
			m_ordinal = ordinal;
			m_initExpression = initExpr;
		}

		public Type FieldType
		{
			get { return m_fieldType; }
		}

		public int Ordinal
		{
			get { return m_ordinal; }
		}

		public Expression InitExpression
		{
			get { return m_initExpression; }
		}

		public override MemberType MemberType
		{
			get { return MemberType.Field; }
		}
		public override void PrintTree(int indent)
		{
			Output(indent, "Field {0} {1}", this.Name, this.FieldType);
		}

		public override void Resolve()
		{
			m_initExpression = m_initExpression.ResolveExpression();

			if (this.IsConst)
			{
				if (!(m_initExpression is IntegerLiteralExpression))
				{
					throw new CompileException("const not literal", m_initExpression.Location);
				}
			}
		}

		public void Emit(EmitContext ec)
		{
		}
	}
}
