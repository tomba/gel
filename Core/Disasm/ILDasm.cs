using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace ClrTest.Reflection
{
	public static class ILDasm
	{
		public static string MethodToString(MethodInfo method)
		{
#if __MonoCS__
			return "ILDasm not supported on Mono";
#endif
			
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(method.Attributes.ToString());

			sb.AppendFormat("{0} {1}(", method.ReturnType, method.Name);

			ParameterInfo[] parameters = method.GetParameters();

			string[] paramStrings = Array.ConvertAll<ParameterInfo, string>(parameters,
				delegate(ParameterInfo pi)
				{
					return pi.ToString();
				}
				);

			sb.Append(String.Join(", ", paramStrings));

			sb.AppendLine(")");

			sb.AppendLine("{");

			if (method is DynamicMethod)
			{
				DynamicMethod dm = (DynamicMethod)method;
				ILGenerator gen = dm.GetILGenerator();
				// TODO: get the info via reflection
			}
			else
			{
				IList<LocalVariableInfo> locals = method.GetMethodBody().LocalVariables;
				List<string> localStrings = new List<string>();

				foreach (LocalVariableInfo l in locals)
				{
					sb.AppendFormat("\t{0}\n", l);
				}
			}

			ILReader reader = new ILReader(method);
			foreach (ILInstruction instr in reader)
			{
				sb.Append("\t");
				sb.AppendLine(instr.ToString());
			}

			sb.AppendLine("}");

			return sb.ToString();
		}
	}
}
