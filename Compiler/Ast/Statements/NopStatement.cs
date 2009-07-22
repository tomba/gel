using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class NopStatement : Statement
	{
		public NopStatement()
		{
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "Nop");
		}

		public override Statement ResolveStatement()
		{
			return this;
		}

		public override void EmitStatement(EmitContext ec)
		{
		}
	}
}
