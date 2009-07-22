namespace Gel.Compiler.Ast
{
	class BaseExpression : Expression
	{
		public BaseExpression(Location loc)
		{
			Location = loc;
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "base {0}", this.ExpressionType);
		}
	}
}
