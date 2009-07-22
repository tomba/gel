using System;
using System.Collections.Generic;
using System.Text;

namespace Gel.Compiler.Ast
{
	interface IAssignable
	{
		void EmitAssignment(EmitContext ec, bool isExpression, Expression rvalue);
	}
}
