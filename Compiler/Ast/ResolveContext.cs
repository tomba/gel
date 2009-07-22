using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	static class ResolveContext
	{
		static Program s_currentProgram = null;
		static MethodDeclaration s_currentMethod = null;
		static Block s_currentBlock = null;
		static List<string> s_namespaces = new List<string>();
		static List<string> s_usingList = new List<string>();
		static Type s_typeHint;
		static Stack<GelLabel> s_exitLabel = new Stack<GelLabel>();
		static Stack<Type> s_exitType = new Stack<Type>();

		public static void Clear()
		{
			s_currentMethod = null;
			s_currentBlock = null;
			s_exitLabel.Clear();
			s_exitType.Clear();
			s_usingList.Clear();
		}

		internal static Program CurrentProgram
		{
			get { return ResolveContext.s_currentProgram; }
			set { ResolveContext.s_currentProgram = value; }
		}

		public static MethodDeclaration CurrentMethod
		{
			get { return s_currentMethod; }
			set { s_currentMethod = value; }
		}

		// Typehint is used for local variable type inference
		public static Type TypeHint
		{
			get { return s_typeHint; }
			set { s_typeHint = value; }
		}

		public static void PushExitLabel(GelLabel label)
		{
			s_exitLabel.Push(label);
		}

		public static void PopExitLabel()
		{
			s_exitLabel.Pop();
		}

		// Inline func: if this is defined, return statement should jump to this label, instead of ret
		public static GelLabel ExitLabel
		{
			get { return s_exitLabel.Count > 0 ? s_exitLabel.Peek() : null; }
		}

		public static void PushExitType(Type type)
		{
			s_exitType.Push(type);
		}

		public static void PopExitType()
		{
			s_exitType.Pop();
		}

		// Inline func: This is the expected return type
		public static Type ExitType
		{
			get { return s_exitType.Count > 0 ? s_exitType.Peek() : null; }
		}



		public static void AddUsingNamespace(string ns)
		{
			s_usingList.Add(ns);
		}

		public static Block CurrentBlock
		{
			get { return s_currentBlock; }
			set { s_currentBlock = value; }
		}

		public static void SetNamespaces(List<string> namespaces)
		{
			s_namespaces = namespaces;
		}

		public static bool FindNamespace(string @namespace)
		{
			System.Console.WriteLine("looking for {0}", @namespace);
			return s_namespaces.Contains(@namespace);
		}

		public static Type FindType(string name)
		{
			Console.WriteLine("finding " + name);
			Type ret = null;

			// TODO: understand how to correctly look for types
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly assembly in assemblies)
			{
				//Console.WriteLine("searching from {0}", assembly.FullName);

				// should only look for exported types
				ret = assembly.GetType(name);

				if (ret != null)
				{
					return ret;
				}

				foreach (string ns in s_usingList)
				{
					ret = assembly.GetType(ns + "." + name);

					if (ret != null)
					{
						return ret;
					}
				}
			}

			return null;
		}
	}
}
