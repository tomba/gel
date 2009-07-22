namespace Gel.Compiler
{
	class Output
	{
		public static void WriteLine(string str, params object[] args)
		{
			Dbg.WriteLine(str, args);
		}

		public static void WriteLine(int indent, string str, params object[] args)
		{
			System.Console.Write(new string(' ',indent));
			Dbg.WriteLine(str, args);
		}
	}
}

