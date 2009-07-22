using System;

namespace Gel.Compiler.Ast
{
	/// <summary>
	/// Summary description for ExprStatement.
	/// </summary>
	class ExpressionStatement : Statement
	{
		Expression m_exp;

		public ExpressionStatement(Expression exp)
		{
			m_exp = exp;
			Location = m_exp.Location;
		}

		public Expression Expr
		{
			get { return m_exp; }
			set { m_exp = value; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "ExprStatement");

			m_exp.PrintTree(indent + 2);
		}

		public override Statement ResolveStatement()
		{
			m_exp = m_exp.ResolveExpression();
			return this;
		}

		public override void EmitStatement(EmitContext ec)
		{
			m_exp.EmitStatement(ec);
		}
	}
}
