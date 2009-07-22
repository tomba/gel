using System;
using System.Collections.Generic;
using System.Reflection;

namespace Gel.Core
{
	public class GelObject
	{
		GelProgram m_program;

		// public so that dynamic methods can access this. dynmethods could also be made to skip visibility checks
		public object[] m_instanceFieldValues; 

		public GelObject(GelProgram program)
		{
			m_program = program;

			int count = 0;

			foreach (KeyValuePair<string, GelField> kvp in m_program.FieldTable)
			{
				GelField field = kvp.Value;

				if (field.IsInstance)
				{
					count++;
				}
			}

			m_instanceFieldValues = new object[count];
		}


		public void SetFieldValue(string name, object value)
		{
			if (m_program.FieldTable.ContainsKey(name))
			{
				m_instanceFieldValues[m_program.FieldTable[name].Ordinal] = value;
			}
			else
			{
				throw new Exception("undefined field");
			}
		}

		public object GetFieldValue(string name)
		{
			if (m_program.FieldTable.ContainsKey(name))
			{
				return m_instanceFieldValues[m_program.FieldTable[name].Ordinal];
			}
			else
			{
				throw new Exception("undefined field");
			}
		}


		public object Invoke(string methodName, params object[] args)
		{
			return m_program.Invoke(methodName, this, args);
		}

		public object InvokeSlow(string methodName, params object[] args)
		{
			return m_program.InvokeSlow(methodName, this, args);
		}

		public GelProgram Program
		{
			get { return m_program; }
		}
	}
}
