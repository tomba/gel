using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace Gel.Compiler.Ast
{
	class MappingCreationExpression : Expression
	{
		List<Expression> m_keyExpressionList;
		List<Expression> m_valueExpressionList;
		Type m_keyType;
		Type m_valueType;

		public MappingCreationExpression(List<Expression> keyExpressionList, List<Expression> valueExpressionList, Location loc)
		{
			m_keyExpressionList = keyExpressionList;
			m_valueExpressionList = valueExpressionList;
			Location = loc;
		}

/*		public MappingCreationExpression(Type elementType, List<Expression> expressionList, Location loc)
		{
			m_elementType = elementType;
			m_expressionList = expressionList;
			Location = loc;
		}
*/
		public override void PrintTree(int indent)
		{
			Output(indent, "MappingCreationExpression {0}:{1}", m_keyType, m_valueType);

			for(int i = 0; i < m_keyExpressionList.Count; i++)
			{
				m_keyExpressionList[i].PrintTree(indent + 2);
				m_valueExpressionList[i].PrintTree(indent + 2);
			}
		}

		public override Expression ResolveExpression()
		{
			Type keyType = null;
			Type valueType = null;

			for (int i = 0; i < m_keyExpressionList.Count; i++)
			{
				m_keyExpressionList[i] = m_keyExpressionList[i].ResolveExpression();
				m_valueExpressionList[i] = m_valueExpressionList[i].ResolveExpression();

				if (i == 0)
				{
					keyType = m_keyExpressionList[i].ExpressionType;
					valueType = m_valueExpressionList[i].ExpressionType;
				}
				else if (m_keyExpressionList[i].ExpressionType != keyType)
				{
					throw new CompileException("Key type mismatch {0}, {1}", this, keyType, m_keyExpressionList[i].ExpressionType);
				}
				else if (m_valueExpressionList[i].ExpressionType != valueType)
				{
					throw new CompileException("Value type mismatch {0}, {1}", this, valueType, m_valueExpressionList[i].ExpressionType);
				}
			}

			m_keyType = keyType;
			m_valueType = valueType;

			this.ExpressionType = Type.GetType("System.Collections.Generic.Dictionary`2[" + m_keyType.FullName + "," + m_valueType.FullName + "]");

			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			ConstructorInfo constructorInfo = this.ExpressionType.GetConstructor(new Type[] { typeof(int) });
			ec.IlGen.Emit(OpCodes.Ldc_I4, m_keyExpressionList.Count);
			ec.IlGen.Emit(OpCodes.Newobj, constructorInfo);

			LocalBuilder tempLocal = ec.IlGen.DeclareLocal(this.ExpressionType);
			ec.IlGen.Emit(OpCodes.Stloc, tempLocal);

			MethodInfo addMethod = this.ExpressionType.GetMethod("Add");
			for(int i = 0; i < m_keyExpressionList.Count; i++)
			{
				ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
				m_keyExpressionList[i].EmitExpression(ec);
				m_valueExpressionList[i].EmitExpression(ec);
				ec.IlGen.EmitCall(OpCodes.Call, addMethod, null);
			}
			ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
		}
	}
}
