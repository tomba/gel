using System;
using System.Collections.Generic;
using System.Text;

namespace Gel.Compiler
{
	public static class Runtime
	{
		static Dictionary<string, object> s_varMap = new Dictionary<string, object>();

		static public object GetVariable(string name)
		{
			if (s_varMap.ContainsKey(name))
				return s_varMap[name];
			else
				return null;
		}

		static public void SetVariable(string name, object value)
		{
			s_varMap[name] = value;
		}

		// Arguments reversed, to help the implementation of assign expression
		// See AssignExpression.cs::EmitAssignment
		static public void SetVariableReverse(object value, string name)
		{
			s_varMap[name] = value;
		}

		static public bool DynamicCompare(object val1, object val2)
		{
			Console.WriteLine("coparing {0}, {1}", val1, val2);
			if (val1.GetType() != val2.GetType())
			{
				Console.WriteLine("typemismatch");
				return false;
			}

			return val1.Equals(val2);
		}

		static public void Dump()
		{
			foreach (string var in s_varMap.Keys)
			{
				Console.WriteLine("{0}: {1}", var, s_varMap[var]);
			}
		}

	}
}
