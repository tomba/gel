using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using Gel.Core;
using System.Text;
using Gel.Compiler.Parser;
using Antlr.Runtime;
using Antlr.Runtime.Tree;

namespace Gel.Compiler
{
	public enum CompileMode
	{
		Program,
		Statement,
		Expression
	}

	public class GelCompiler
	{
		List<string> m_namespaces = new List<string>();

		public GelCompiler()
		{
			// Collect namespace names
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes(); // GetExportedTypes();

				foreach (Type type in types)
				{
					if (type.Namespace == null)
						continue;

					string[] parts = type.Namespace.Split('.');

					StringBuilder sb = new StringBuilder(type.Name.Length);

					for (int i = 0; i < parts.Length; i++)
					{
						sb.Append(parts[i]);

						string ns = sb.ToString();

						if (!m_namespaces.Contains(ns))
						{
							m_namespaces.Add(ns);
						}

						sb.Append(".");
					}

				}
			}
		}

		string TreeToString(int level, ITree tree)
		{
			if (tree.ChildCount == 0)
				return tree.ToString();

			StringBuilder sb = new StringBuilder();

			if (!tree.Nil)
			{
				sb.Append("(");
				sb.Append(tree.ToString());
				sb.Append(" ");
			}

			for (int i = 0; i < tree.ChildCount; i++)
			{
				ITree t = tree.GetChild(i);
				if (i > 0)// && level > 0)
				{
					sb.Append(' ');
				}
				sb.Append(TreeToString(level + 1, t));
			}
			
			if (!tree.Nil)
				sb.Append(")");

			if (level == 1)
				sb.AppendLine();

			return sb.ToString();
		}

		public CompilerResults CompileFile(string file, CompileMode mode)
		{
			ICharStream stream = new ANTLRFileStream(file);

			return Compile(stream, mode);
		}

		public CompilerResults CompileText(string code, CompileMode mode)
		{
			ICharStream stream = new ANTLRStringStream(code);

			return Compile(stream, mode);
		}

		CompilerResults Compile(ICharStream input, CompileMode mode)
		{
			int phases = 0;

			GelProgram program = null;

			List<CompilerError> errors = new List<CompilerError>();

			try
			{
				//Console.WriteLine("Source:");
				//Console.WriteLine(expr);

				Console.WriteLine("Parsing:");

				Ast.ResolveContext.Clear();
				Ast.ResolveContext.SetNamespaces(m_namespaces);

				GelLexer lexer = new GelLexer(input);
				CommonTokenStream tokens = new CommonTokenStream(lexer);
				GelParser parser = new GelParser(tokens);

				ITree tree;

				if(mode == CompileMode.Program)
					tree = parser.ParseProgram();
				else if (mode == CompileMode.Statement)
					tree = parser.ParseStatement();
				else //(type == CompileType.Expression)
					tree = parser.ParseExpression();

				if (parser.m_numErrors > 0)
				{
					Console.WriteLine("Parsing failed");
					return null;
				}

				Console.WriteLine(tree.ToStringTree());
				//Console.WriteLine(TreeToString(0, t));

				if (phases == 1)
					return null;


				Console.WriteLine();
				Console.WriteLine("Tree Parsing:");

				CommonTreeNodeStream nodes = new CommonTreeNodeStream(tree);
				GelGenTreeParser walker = new GelGenTreeParser(nodes);

				Ast.Program root;

				if (mode == CompileMode.Program)
					root = walker.program();
				else if (mode == CompileMode.Statement)
					root = walker.statementProgram();
				else //(type == CompileType.Expression)
					root = walker.expressionProgram();

				if (walker.m_numErrors > 0)
				{
					Console.WriteLine("Tree parsing failed");
					return null;
				}

				Dbg.WriteLine("\n\n-- AST --\n");
				root.PrintTree(0);

				if (phases == 2)
					return null;

				program = CompileTree(root);
				
			}
			catch (CompileException e)
			{
				errors.Add(new CompilerError(null, e.Message, e.Line, e.Column));
			}
			catch (Antlr.Runtime.RecognitionException e)
			{
				errors.Add(new CompilerError(null, e.ToString(), 0, 0));
			}
			/*			catch (Exception e)
						{
							errors.Add(new CompilerError(null, e.ToString(), 0, 0));
						}
			*/

			CompilerResults result = new CompilerResults(program, errors);

			return result;
		}

		GelProgram CompileTree(Ast.Program root)
		{
			Dbg.WriteLine("\n\n-- Resolving --\n");
			try
			{
				root.Resolve();
			}
			catch (Exception)
			{
				root.PrintTree(0);
				throw;
			}
			root.PrintTree(0);

			Dbg.WriteLine("\n\n-- Emit --\n");
			Ast.EmitContext emitContext = new Ast.EmitContext();
			emitContext.EmitSymbols = false;
			root.Emit(emitContext);


			// Create Program
			List<GelMethod> methodList = new List<GelMethod>();
			foreach (Ast.MethodDeclaration method in root.GetMethods())
			{
				GelMethod m = new GelMethod(method.MethodInfo, method.Modifiers);
				methodList.Add(m);
			}

			List<GelField> fieldList = new List<GelField>();
			foreach (Ast.FieldDeclaration field in root.GetFields())
			{
				GelField f = new GelField(field.Name, field.Modifiers, field.FieldType, field.Ordinal);
				fieldList.Add(f);
			}

			GelProgram program = new GelProgram(methodList.ToArray(), fieldList.ToArray());

			// ILDasm
			foreach (GelMethod mi in methodList)
			{
				Console.WriteLine(ClrTest.Reflection.ILDasm.MethodToString(mi.MethodInfo));
			}

			return program;
		}


		/**
		 * Actual compiler methods
		 */

#if asda
		static Ast.MethodDeclaration CreateFlowGraph(Ast.MethodDeclaration root)
		{
			Dbg.WriteLine("\n\n-- constructing Flow control graph --\n");

			Graph.ControlFlowGraph controlFlowGraph = new Graph.ControlFlowGraph();
			Graph.Visitor.ControlFlowCreateVisitor graphVisitor = new Graph.Visitor.ControlFlowCreateVisitor(controlFlowGraph);

			try
			{
				graphVisitor.DoVisit(root);
			}
			catch (System.Reflection.TargetInvocationException e)
			{
				while (e.InnerException is System.Reflection.TargetInvocationException)
					e = (System.Reflection.TargetInvocationException)e.InnerException;

				Exception exc = e.InnerException;

				if (exc is CompileException)
				{
					Dbg.WriteLine(exc.Message);
				}
				else
				{
					Dbg.WriteLine("ERROR {0}", exc.ToString());
				}

				throw exc;
			}

			Dbg.WriteLine("\n\n-- Flow Control Graph --\n");
			controlFlowGraph.Print(0);

			return root;
		}
#endif
	}
}
