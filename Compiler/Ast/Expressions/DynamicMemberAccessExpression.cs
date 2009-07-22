using System;
using System.Reflection;
using Gel.Core;

namespace Gel.Compiler.Ast
{
	/* 
	 * Member access in UnknownType
	 */
	class DynamicMemberAccessExpression : Expression
	{
		Expression m_instanceExpr;
		string m_memberName;

		public DynamicMemberAccessExpression(Expression instanceExpr, string memberName, Location loc)
		{
			m_instanceExpr = instanceExpr;
			m_memberName = memberName;
			Location = loc;
		}

		public Expression InstanceExpression
		{
			get { return m_instanceExpr; }
			set { m_instanceExpr = value; }
		}

		public string MemberName
		{
			get { return m_memberName; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "DynamicMemberAccessExpression {0}", m_memberName);
			m_instanceExpr.PrintTree(indent + 2);
		}
	}
}
