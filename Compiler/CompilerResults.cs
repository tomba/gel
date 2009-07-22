#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Gel.Core;

#endregion

namespace Gel.Compiler
{
	public class CompilerResults
	{
		List<CompilerError> m_errorList;
		GelProgram m_program;

		public CompilerResults(GelProgram program, List<CompilerError> errors)
		{
			m_program = program;
			m_errorList = errors;
		}

		public List<CompilerError> Errors
		{
			get { return m_errorList; }
		}

		public GelProgram Program
		{
			get { return m_program; }
		}
	}

	public class CompilerError
	{
		string m_fileName;
		string m_text;
		int m_line;
		int m_column;

		public CompilerError(string fileName, string text, int line, int column)
		{
			m_fileName = fileName;
			m_text = text;
			m_line = line;
			m_column = column;
		}

		public string Text
		{
			get { return m_text; }
		}

		public int Line
		{
			get { return m_line; }
		}

		public int Column
		{
			get { return m_column; }
		}

	}

}
