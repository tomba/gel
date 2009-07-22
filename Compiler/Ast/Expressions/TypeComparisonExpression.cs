using System;

namespace Gel.Compiler.Ast
{
	class TypeComparisonExpression : Expression
	{
		private Operator	m_op;
		private Expression	m_expr;
		private Type m_type;

		public enum Operator
		{
			None,
			IS,
			AS
		}
		
		public TypeComparisonExpression(Operator op, Expression exp, Type type, Location loc)
		{
			m_op = op;
			m_expr = exp;
			m_type = type;
			Location = loc;
		}

		public override void PrintTree(int indent)
		{
			Output(indent, m_op.ToString() + " {0}", this.ExpressionType);
			m_expr.PrintTree(indent+2);
			Output(indent+2, m_type.ToString());
		}
	}
}
