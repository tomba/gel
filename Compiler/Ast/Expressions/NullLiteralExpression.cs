using System.Reflection.Emit;
namespace Gel.Compiler.Ast
{
	class NullLiteralExpression : Expression
	{
		public NullLiteralExpression(Location loc)
		{
			Location = loc;
			this.ExpressionType = typeof(NullType);
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "null {0}", this.ExpressionType);
		}

		public override Expression ResolveExpression()
		{
			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			ec.IlGen.Emit(OpCodes.Ldnull);
		}
	}
}
