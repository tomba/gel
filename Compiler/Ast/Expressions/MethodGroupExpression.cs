#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Gel.Core;

#endregion

namespace Gel.Compiler.Ast
{
	class MethodGroupExpression : Expression
	{
		MethodInfo[] m_methodInfos;
		Expression m_instanceExp;

		public MethodGroupExpression(MethodInfo[] mi, Expression instanceExp, Location loc)
		{
			m_methodInfos = mi;
			m_instanceExp = instanceExp;
			this.Location = loc;
			this.ExpressionClass = ExpressionClass.MethodGroup;
			this.ExpressionType = typeof(MethodGroup);
		}

		public MethodInfo[] MethodInfoArray
		{
			get { return m_methodInfos; }
		}

		public Expression InstanceExpression
		{
			get { return m_instanceExp; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "MethodGroup ({0} members)", m_methodInfos.Length);

			int i = 0;
			foreach (MethodInfo mi in m_methodInfos)
			{
				Output(indent+2, "{0}: {1}", i, mi.ToString());
			}

			if(m_instanceExp != null)
				m_instanceExp.PrintTree(indent+2);
		}
	}
}
