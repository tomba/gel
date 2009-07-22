#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;

using Gel.Compiler;
using Gel.Core;

using Mono.GetOptions;

#endregion

public class test
{
	static public void testi()
	{
		byte b = 1;
		if (b == 2)
			b = 2;

		if (b == 3)
			b = 3;
	}
}

namespace Gel
{
	public class GelOptions : Options
	{
		public GelOptions()
		{
			base.ParsingMode = OptionsParsingMode.Both;
		}

		[Option("Evaluate an expression and print its return value", 'e')]
		public bool expr;

		[Option("Execute statement(s) and print return value, if any", 's')]
		public bool stmt;

		[Option("Print execution time", 't')]
		public bool time;

		[Option("Return value formatting", 'f')]
		public char format;
	}

	class Program
	{
		static void Main(string[] args)
		{
			test.testi();

			GelOptions options = new GelOptions();
			options.ProcessArgs(args);

			string code = string.Join(" ", options.RemainingArguments);

			bool timing = options.time;

			GelCompiler compiler = new GelCompiler();

			CompilerResults res = null;

			if (options.stmt == true)
			{
				res = compiler.CompileText(code, CompileMode.Statement);
			}
			else if (options.expr == true)
			{
				res = compiler.CompileText(code, CompileMode.Expression);
			}
			else
			{
				string file;

				if (code.Length == 0)
				{
					file = @"../../../Tests/test.bs";
					if (!File.Exists(file))
					{
						file = @"../Tests/test.bs";
					}
				}
				else
					file = code;

				if (File.Exists(file) == false)
				{
					Console.WriteLine("File not found: {0}", file);
					return;
				}

				res = compiler.CompileFile(file, CompileMode.Program);
			}

			if (res == null)
			{
				Console.WriteLine("Compiler aborted");
				return;
			}

			if (res.Errors.Count > 0)
			{
				foreach (CompilerError error in res.Errors)
				{
					Console.WriteLine(error.Text);
				}

				return;
			}

			GelProgram program1 = res.Program;

			Console.WriteLine("Invoking");

			object ret = null;

			//GelObject ob1 = program1.CreateInstance();

			Stopwatch watch = new Stopwatch();

			try
			{
				if(timing)
					watch.Start();

				for (int i = 0; i < 1; i++)
				{
					//ret = ob1.Invoke("main");
					ret = program1.InvokeSlow("main", null);
				}

				if(timing)
					watch.Stop();
			}
			catch (TargetInvocationException e)
			{
				Console.WriteLine(e.ToString());
				Console.WriteLine("----------");
				Console.WriteLine(e.InnerException.ToString());
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.WriteLine("----------");
			}

			try
			{
				if (options.format == 'x')
					Console.WriteLine("ret = 0x{0:x}", (int)ret);
				else if (options.format == 'b')
					Console.WriteLine("ret = 0b{0}", NumberToBinary((int)ret));
				else
					Console.WriteLine("ret = {0}", ret == null ? "<null>" : ret);
			}
			catch (Exception e)
			{
				Console.WriteLine("Unable to show the return value with specified formatting: {0}", e.Message);
				Console.WriteLine("ret = {0}", ret == null ? "<null>" : ret);
			}

			if(timing)
				Console.WriteLine("Time = {0} ms", watch.ElapsedMilliseconds);

			/*
			if (Debugger.IsAttached)
			{
				System.Console.WriteLine("\nPress enter...");
				System.Console.ReadLine(); // pause at end for debug
			}
			 */

		}

		static string NumberToBinary(int number)
		{
			StringBuilder sb = new StringBuilder();

			while (number > 0)
			{
				if ((number & 1) != 0)
					sb.Insert(0, "1");
				else
					sb.Insert(0, "0");

				number = number >> 1;
			}

			return sb.ToString();
		}
	}
}
