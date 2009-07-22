#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Gel.Compiler
{
	static class Dbg
	{
		static Dbg()
		{
			System.Diagnostics.Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
			System.Diagnostics.Debug.AutoFlush = true;
		}

		[System.Diagnostics.ConditionalAttribute("DEBUG")]
		public static void WriteLine(object obj)
		{
			System.Diagnostics.Debug.WriteLine(obj);
		}

		[System.Diagnostics.ConditionalAttribute("DEBUG")]
		public static void WriteLine(string format, params object[] obs)
		{
			System.Diagnostics.Debug.WriteLine(String.Format(format, obs));
		}
	}
}
