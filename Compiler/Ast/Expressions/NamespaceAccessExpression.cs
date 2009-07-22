#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Gel.Compiler.Ast
{
	class NamespaceAccessExpression : Expression
	{
		string m_namespace;

		public NamespaceAccessExpression(string @namespace)
		{
			m_namespace = @namespace;
			this.ExpressionClass = ExpressionClass.Namespace;
		}

		public string Namespace
		{
			get { return m_namespace; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "NamespaceAccess {0}", m_namespace);
		}
	}
}
