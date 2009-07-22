using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	// Dummy class. Exists because Label is a struct, and it's handy to pass a null label,
	// thus we need a class
	public class GelLabel
	{
		Label m_label;

		public GelLabel(Label l)
		{
			m_label = l;
		}

		public Label Label
		{
			get { return m_label; }
		}
	}

}
