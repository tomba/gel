using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace Gel.Compiler.Ast
{
	class ListCreationExpression : Expression
	{
		List<Expression> m_expressionList;
		Type m_elementType;

		public ListCreationExpression(List<Expression> expressionList, Location loc)
		{
			m_expressionList = expressionList;
			Location = loc;
		}

		public ListCreationExpression(Type elementType, List<Expression> expressionList, Location loc)
		{
			m_elementType = elementType;
			m_expressionList = expressionList;
			Location = loc;
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "ListCreation {0}", m_elementType);

			foreach (Expression exp in m_expressionList)
			{
				exp.PrintTree(indent + 2);
			}
		}

		public override Expression ResolveExpression()
		{
			if (m_elementType != null)
			{
				for (int i = 0; i < m_expressionList.Count; i++)
				{
					m_expressionList[i] = m_expressionList[i].ResolveExpression();

					m_expressionList[i] = ImplicitConversion(m_elementType, m_expressionList[i]);

					if (m_elementType != m_expressionList[i].ExpressionType)
					{
						throw new CompileException("Type mismatch {0}, {1}", this, m_elementType, m_expressionList[i].ExpressionType);
					}
				}
			}
			else
			{
				Type t = null;

				for (int i = 0; i < m_expressionList.Count; i++)
				{
					m_expressionList[i] = m_expressionList[i].ResolveExpression();

					if (i == 0)
					{
						t = m_expressionList[i].ExpressionType;
					}
					else if(m_expressionList[i].ExpressionType != t)
					{
						throw new CompileException("Type mismatch {0}, {1}", this, t, m_expressionList[i].ExpressionType);
					}
				}

				m_elementType = t;
			}

			this.ExpressionType = Type.GetType("System.Collections.Generic.List`1[" + m_elementType.FullName + "]");

			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			ConstructorInfo constructorInfo = this.ExpressionType.GetConstructor(new Type[] { typeof(int) });
			ec.IlGen.Emit(OpCodes.Ldc_I4, m_expressionList.Count);
			ec.IlGen.Emit(OpCodes.Newobj, constructorInfo);

			LocalBuilder tempLocal = ec.IlGen.DeclareLocal(this.ExpressionType);
			ec.IlGen.Emit(OpCodes.Stloc, tempLocal);

			MethodInfo addMethod = this.ExpressionType.GetMethod("Add");
			foreach (Expression exp in m_expressionList)
			{
				ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
				exp.EmitExpression(ec);
				ec.IlGen.EmitCall(OpCodes.Call, addMethod, null);
			}
			ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
		}
	}
}
