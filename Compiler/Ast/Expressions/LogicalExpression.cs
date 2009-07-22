using System;
using System.Reflection.Emit;
namespace Gel.Compiler.Ast
{
	class LogicalExpression : BinaryExpression
	{
		private Operator		m_op;
		
		public enum Operator
		{
			And,
			Or
		}

		public LogicalExpression(Operator op, Expression e1, Expression e2, Location loc)
		{
			m_op = op;
			this.Left = e1;
			this.Right = e2;
			Location = loc;
		}

		public Operator Oper
		{
			get
			{
				return m_op;
			}
		}

		public override void PrintTree(int indent)
		{
			switch(m_op)
			{
				case Operator.And:
					Output(indent, "&& {0}", this.ExpressionType);
					break;
				case Operator.Or:
					Output(indent, "|| {0}", this.ExpressionType);
					break;
			}

			this.Left.PrintTree(indent + 2);
			this.Right.PrintTree(indent + 2);
		}

		public override Expression ResolveExpression()
		{
			m_left = m_left.ResolveExpression();
			m_right = m_right.ResolveExpression();

			if (m_left.ExpressionType != typeof(bool))
			{
				throw new CompileException("Expected bool", m_left.Location);
			}

			if (m_right.ExpressionType != typeof(bool))
			{
				throw new CompileException("Expected bool", m_right.Location);
			}

			this.ExpressionType = typeof(bool);

			return this;
		}

		public override void EmitConditionalStatement(EmitContext ec, 
			Nullable<Label> trueLabel, Nullable<Label> falseLabel)
		{
			if (m_op == Operator.And)
			{
				m_left.EmitConditionalStatement(ec, null, falseLabel);
				m_right.EmitConditionalStatement(ec, trueLabel, falseLabel);
			}
			else // Or
			{
				m_left.EmitConditionalStatement(ec, trueLabel, null);
				m_right.EmitConditionalStatement(ec, trueLabel, falseLabel);
			}
		}
	}
}
