using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Gel.Core
{
	public class GelProgram
	{
		// public so that dynamic methods can access this. dynmethods could also be made to skip visibility checks
		public static GelProgram s_currentProgram = null; 

		Dictionary<string, GelMethod> m_methodTable = new Dictionary<string, GelMethod>();
		Dictionary<string, GelField> m_fieldTable = new Dictionary<string, GelField>();

		// public so that dynamic methods can access this. dynmethods could also be made to skip visibility checks
		public object[] m_staticFieldValues; 

		public GelProgram(GelMethod[] methods, GelField[] fields)
		{
			foreach (GelMethod mi in methods)
			{
				m_methodTable[mi.Name] = mi;
			}

			int count = 0;

			foreach (GelField fi in fields)
			{
				m_fieldTable[fi.Name] = fi;

				if (fi.IsStatic)
				{
					count++;
				}
			}

			m_staticFieldValues = new object[count];
		}

		public GelMethod FindMethod(string name)
		{
			if (m_methodTable.ContainsKey(name))
			{
				return m_methodTable[name];
			}

			throw new Exception("method not found");
		}
		/*
		public void SetFieldValue(string name, object value)
		{
			if (m_staticFieldData.ContainsKey(name))
			{
				m_staticFieldData[name] = value;
			}
			else
			{
				throw new Exception("undefined field");
			}
		}

		public object GetFieldValue(string name)
		{
			if (m_staticFieldData.ContainsKey(name))
			{
				return m_staticFieldData[name];
			}
			else
			{
				throw new Exception("undefined field");
			}
		}
		*/
		public Dictionary<string, GelField> FieldTable
		{
			get { return m_fieldTable; }
		}

		/*
		public object Invoke(string methodName, params object[] args)
		{
			s_currentProgram = this;
			MethodInfo mi = FindMethod(methodName);
			return mi.Invoke(null, args);
		}
		 */

		public object Invoke(string methodName, object obj, params object[] args)
		{
			GelMethod method = FindMethod(methodName);

			GelProgram oldProgram = s_currentProgram;
			s_currentProgram = this;
			object ret = method.Invoke(obj, args);
			s_currentProgram = oldProgram;

			return ret;
		}

		public object InvokeSlow(string methodName, object obj, params object[] args)
		{
			GelMethod method = FindMethod(methodName);

			object ret;

			GelProgram oldProgram = s_currentProgram;
			s_currentProgram = this;

			if (!method.IsStatic)
			{
				object[] newArgs = new object[args.Length + 1];
				newArgs[0] = obj;
				Array.Copy(args, 0, newArgs, 1, args.Length);
				ret = method.MethodInfo.Invoke(null, newArgs);
			}
			else
			{
				ret = method.MethodInfo.Invoke(null, args);
			}

			s_currentProgram = oldProgram;

			return ret;
		}

		public GelObject CreateInstance()
		{
			return new GelObject(this);
		}
	}
}
