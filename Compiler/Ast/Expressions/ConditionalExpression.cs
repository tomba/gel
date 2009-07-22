namespace Gel.Compiler.Ast
{
	class ConditionalExpression : Expression
	{
		private Expression	m_compare;
		private	Expression	m_e1;
		private	Expression	m_e2;

		public ConditionalExpression(Expression comp, Expression e1, Expression e2, Location loc)
		{
			m_compare = comp;
			m_e1 = e1;
			m_e2 = e2;
			Location = loc;
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "? {0}", this.ExpressionType);
			m_compare.PrintTree(indent+2);
			m_e1.PrintTree(indent+2);
			m_e2.PrintTree(indent+2);
		}

	}
}
