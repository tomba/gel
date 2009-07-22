using System;
using System.Collections.Generic;
using System.Text;

namespace Gel.Compiler.Ast
{
	class Location
	{
		string m_fileName;
		int m_lineNumber;
		int m_column;

		public Location()
		{
			m_fileName = null;
			m_lineNumber = 0;
			m_column = 0;
		}

		public Location(string fileName, int lineNumber, int column)
		{
			m_fileName = fileName;
			m_lineNumber = lineNumber;
			m_column = column;
		}

		public Location(Antlr.Runtime.Tree.CommonTree node)
		{
			m_lineNumber = node.Line;
			m_column = node.CharPositionInLine;
		}

		public static implicit operator Location(Antlr.Runtime.Tree.CommonTree node)
		{
			return new Location(node);
		}

		public string FileName
		{
			set
			{
				m_fileName = value;
			}

			get
			{
				return m_fileName;
			}
		}

		public int LineNumber
		{
			set
			{
				m_lineNumber = value;
			}

			get
			{
				return m_lineNumber;
			}
		}

		public int Column
		{
			get { return m_column; }
		}

		public override string ToString()
		{
			return String.Format("{0}({1},{2})", m_fileName, m_lineNumber, m_column);
		}

	}

}
