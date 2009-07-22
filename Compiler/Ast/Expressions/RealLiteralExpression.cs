namespace Gel.Compiler.Ast
{
	class RealLiteralExpression : Expression
	{
		private double	m_number;

		public RealLiteralExpression(double num, Location loc)
		{
			m_number = num;
			Location = loc;
		}

		// parse real literal
		public RealLiteralExpression(string num, Location loc)
		{
			m_number = double.Parse(num);
			Location = loc;
		}

		public override void PrintTree(int indent)
		{
			Output(indent, m_number.ToString() + " {0}", this.ExpressionType);
		}
	}
}
