using System;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	/// <summary>
	/// Summary description for WhileStatement.
	/// </summary>
	class ForStatement : Statement
	{
		private Statement m_initializer;
		private Expression m_condition;
		private Statement m_iterator;
		private Statement m_body;

		public ForStatement(Statement initializer, Expression condition, Statement iterator, 
			Statement body, Location loc)
		{
			m_initializer = initializer;
			m_condition = condition;
			m_iterator = iterator;
			m_body = body;
			Location = loc;

			if (m_body == null)
			{
				m_body = new NopStatement();
			}
		}

		public Statement InitializerList
		{
			get { return m_initializer; }
			set { m_initializer = value; }
		}

		public Expression Condition
		{
			get { return m_condition; }
			set { m_condition = value; }
		}

		public Statement LoopStatementList
		{
			get { return m_iterator; }
			set { m_iterator = value; }
		}

		public Statement Body
		{
			get { return m_body; }
			set { m_body = value; }
		}

		public override Statement ResolveStatement()
		{
			if (m_initializer != null)
				m_initializer = m_initializer.ResolveStatement();

			if (m_condition != null)
			{
				m_condition = m_condition.ResolveExpression();

				if (m_condition.ExpressionType != typeof(bool))
				{
					throw new CompileException("Condition for for is not boolean", m_condition.Location);
				}
			}

			if (m_iterator != null)
				m_iterator = m_iterator.ResolveStatement();

			m_body = m_body.ResolveStatement();

			return this;
		}

		public override void EmitStatement(EmitContext ec)
		{
			if (m_initializer != null)
			{
				m_initializer.EmitStatement(ec);
			}

			Label loopLabel = ec.IlGen.DefineLabel();
			Label endLabel = ec.IlGen.DefineLabel();

			ec.IlGen.MarkLabel(loopLabel);

			if (m_condition != null)
			{
				m_condition.EmitConditionalStatement(ec, null, endLabel);
			}

			m_body.EmitStatement(ec);

			if (m_iterator != null)
			{
				m_iterator.EmitStatement(ec);
			}

			ec.IlGen.Emit(OpCodes.Br, loopLabel);

			ec.IlGen.MarkLabel(endLabel);
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "for");

			if (m_initializer != null)
				m_initializer.PrintTree(indent + 2);
			else
				Output(indent + 2, "null");

			if (m_condition != null)
				m_condition.PrintTree(indent + 2);
			else
				Output(indent + 2, "null");

			if (m_iterator != null)
				m_iterator.PrintTree(indent + 2);
			else
				Output(indent + 2, "null");

			m_body.PrintTree(indent + 2);
		}

	}
}
