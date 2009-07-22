#region Using directives

using System;
using System.Text;

#endregion

namespace Gel.Compiler.Ast
{
	class LocalConst
	{
		private string m_name;
		private Location m_location;
		private Type m_type;
		Expression m_valueExpression;

		public LocalConst(string name, Type type, Expression valueExpr, Location loc)
		{
			m_name = name;
			m_type = type;
			m_valueExpression = valueExpr;
			m_location = loc;
		}

		public string Name
		{
			get { return m_name; }
		}

		public Type VariableType
		{
			get { return m_type; }
			set { m_type = value; }
		}

		public Expression ValueExpression
		{
			get { return m_valueExpression; }
			set { m_valueExpression = value; }
		}

		public override string ToString()
		{
			return String.Format("const {0} {1}", m_type, m_name);
		}

	}
}