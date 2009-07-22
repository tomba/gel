using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class ArrayCreationExpression : Expression
	{
		List<Expression> m_initExpressionList;
		Expression m_lengthExpression;
		
		public ArrayCreationExpression(Type arrayType, Expression lengthExpression,
			List<Expression> initExpressionList, Location loc)
		{
			this.ExpressionType = arrayType;
			m_initExpressionList = initExpressionList;
			m_lengthExpression = lengthExpression;
			Location = loc;

			if (m_lengthExpression == null)
			{
				m_lengthExpression = new IntegerLiteralExpression(m_initExpressionList.Count, this.Location);
			}

			if (m_initExpressionList == null)
			{
				m_initExpressionList = new List<Expression>();
			}
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "ArrayCreation {0}", this.ExpressionType);

			Output(indent, "[");
			m_lengthExpression.PrintTree(indent + 2);
			Output(indent, "]");

			foreach (Expression exp in m_initExpressionList)
			{
				exp.PrintTree(indent + 2);
			}
		}

		public override Expression ResolveExpression()
		{
			Type elementType = this.ExpressionType.GetElementType();

			for (int i = 0; i < m_initExpressionList.Count; i++)
			{
				m_initExpressionList[i] = m_initExpressionList[i].ResolveExpression();

				m_initExpressionList[i] = ImplicitConversion(elementType, m_initExpressionList[i]);

				if(elementType != m_initExpressionList[i].ExpressionType)
				{
					throw new CompileException("Type mismatch {0}, {1}", this, elementType, m_initExpressionList[i].ExpressionType);
				}
			}

			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			m_lengthExpression.EmitExpression(ec);
			ec.IlGen.Emit(OpCodes.Newarr, this.ExpressionType.GetElementType());

			LocalBuilder tempLocal = ec.IlGen.DeclareLocal(this.ExpressionType);
			ec.IlGen.Emit(OpCodes.Stloc, tempLocal);

			for(int index = 0; index < m_initExpressionList.Count; index++)
			{
				ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
				ec.IlGen.Emit(OpCodes.Ldc_I4, index);
				m_initExpressionList[index].EmitExpression(ec);
				ec.IlGen.Emit(OpCodes.Stelem, this.ExpressionType.GetElementType());
			}

			ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
		}
	}
}
