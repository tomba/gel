using System;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class ThisExpression : Expression
	{
		public ThisExpression(Location loc)
		{
			Location = loc;
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "this {0}", this.ExpressionType);
		}

		public override void EmitExpression(EmitContext ec)
		{
			ec.IlGen.Emit(OpCodes.Ldarg_0);			
		}
	}
}
