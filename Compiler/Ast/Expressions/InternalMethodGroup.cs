#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using Gel.Core;

#endregion

namespace Gel.Compiler.Ast
{
	class InternalMethodGroup : Expression
	{
		MethodDeclaration[] m_methods;
		Expression m_instanceExp;

		public InternalMethodGroup(MethodDeclaration[] methods, Expression instanceExp, Location loc)
		{
			m_methods = methods;
			m_instanceExp = instanceExp;
			this.Location = loc;
			this.ExpressionClass = ExpressionClass.MethodGroup;
			this.ExpressionType = typeof(MethodGroup);
		}

		public MethodDeclaration[] MethodArray
		{
			get { return m_methods; }
		}

		public Expression InstanceExpression
		{
			get { return m_instanceExp; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "InternalMethodGroup ({0} members)", m_methods.Length);

			int i = 0;
			foreach (MethodDeclaration mi in m_methods)
			{
				Output(indent+2, "{0}: {1}", i, mi.Name);
			}

			if(m_instanceExp != null)
				m_instanceExp.PrintTree(indent+2);
		}
	}
}
