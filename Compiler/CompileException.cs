#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

using Gel.Compiler.Ast;

#endregion

namespace Gel.Compiler
{
	class CompileException : ApplicationException
	{
		string m_message;
		Location m_location;

		public CompileException(string message, Location loc)
		{
			m_message = message;
			m_location = loc;
		}

		public CompileException(string message, AstNode context, params object[] args)
		{
			m_message = String.Format(message, args);
			m_location = context.Location;
		}

		public override string Message
		{
			get
			{
				return String.Format("{0}: {1}", m_location, m_message);
			}
		}

		public int Line
		{
			get 
			{
				if (m_location != null)
					return m_location.LineNumber;
				else
					return -1;
			}
		}

		public int Column
		{
			get 
			{
				if (m_location != null)
					return m_location.Column;
				else
					return -1;
			}
		}


	}
}
