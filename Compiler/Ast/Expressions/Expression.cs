using System;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	enum ExpressionClass
	{
		Value,
		Variable,
		Namespace,
		Type,
		MethodGroup,
		PropertyAccess,
		//EventAccess,
		IndexerAccess,
		//Nothing
	}

	abstract class Expression : AstNode
	{
		Type m_expressionType;
		ExpressionClass m_expressionClass;

		public Expression()
		{
			// Default to Value, because most expressions are values
			m_expressionClass = ExpressionClass.Value;
		}

		public Type ExpressionType
		{
			get { return m_expressionType; }
			set { m_expressionType = value; }
		}

		public ExpressionClass ExpressionClass
		{
			get { return m_expressionClass; }
			set { m_expressionClass = value; }
		}

		public virtual Expression ResolveExpression()
		{
			throw new NotImplementedException(String.Format("Resolve not implemented in {0}", this.GetType().Name));
		}

		public virtual void EmitExpression(EmitContext ec)
		{
			throw new NotImplementedException(String.Format("Emit not implemented in {0}", this.GetType().Name));
		}

		public virtual void EmitStatement(EmitContext ec)
		{
			EmitExpression(ec);
			ec.IlGen.Emit(OpCodes.Pop);
		}

		public virtual void EmitConditionalStatement(EmitContext ec, 
			Nullable<Label> trueLabel, Nullable<Label> falseLabel)
		{
			EmitExpression(ec);

			if (trueLabel.HasValue)
			{
				ec.IlGen.Emit(OpCodes.Brtrue, trueLabel.Value);

				if (falseLabel.HasValue)
				{
					ec.IlGen.Emit(OpCodes.Br, falseLabel.Value);
				}
			}
			else if (falseLabel.HasValue)
			{
				ec.IlGen.Emit(OpCodes.Brfalse, falseLabel.Value);
			}
		}
	}
}