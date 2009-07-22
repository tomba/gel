using System;
using System.Collections.Generic;
using System.Text;

namespace Gel.Core
{
	public class GelField
	{
		string m_name;
		Modifiers m_modifiers;
		Type m_fieldType;
		int m_ordinal;

		public GelField(string name, Modifiers modifiers, Type fieldType, int ordinal)
		{
			m_name = name;
			m_modifiers = modifiers;
			m_fieldType = fieldType;
			m_ordinal = ordinal;
		}

		public string Name
		{
			get { return m_name; }
		}

		public Modifiers Modifiers
		{
			get { return m_modifiers; }
		}

		public Type FieldType
		{
			get { return m_fieldType; }
		}

		public int Ordinal
		{
			get { return m_ordinal; }
		}

		public bool IsStatic
		{
			get { return (m_modifiers & Modifiers.Static) != 0; }
		}

		public bool IsInstance
		{
			get { return (m_modifiers & Modifiers.Static) == 0; }
		}
	}
}
