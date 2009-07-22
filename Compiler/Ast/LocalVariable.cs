#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;

#endregion

namespace Gel.Compiler.Ast
{
	class LocalVariable
	{
		private string m_name;
		private Location m_location;
		private Type m_type;
		private int m_ordinal;
		private bool m_isParameter;

		LocalBuilder m_localBuilder;

		public LocalVariable(string name, Type type, int ordinal, bool isParameter, Location loc)
		{
			m_name = name;
			m_type = type;
			m_ordinal = ordinal;
			m_isParameter = isParameter;
			m_location = loc;
		}

		public string Name
		{
			get { return m_name; }
		}

		public int Ordinal
		{
			get { return m_ordinal; }
		}

		public Type VariableType
		{
			get { return m_type; }
			set { m_type = value; }
		}

		public bool IsParameter
		{
			get { return m_isParameter; }
		}

		public LocalBuilder LocalBuilder
		{
			get { return m_localBuilder; }
			set { m_localBuilder = value; }
		}

		public override string ToString()
		{
			return String.Format("{0}: {1} {2}", m_ordinal, m_type, m_name);
		}

	}
}
