using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace Gel.Compiler.Ast
{
	class ElementAccessExpression : Expression, IAssignable
	{
		Expression m_expr;
		List<Expression> m_expressionList;
		PropertyInfo m_itemPropertyInfo;

		public ElementAccessExpression(Expression expr, List<Expression> expressionList, Location loc)
		{
			m_expr = expr;
			m_expressionList = expressionList;
			Location = loc;
		}

		public List<Expression> ExprList
		{
			get { return m_expressionList; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "ElementAccess {0}", this.ExpressionType);

			m_expr.PrintTree(indent + 2);

			foreach (Expression exp in m_expressionList)
			{
				exp.PrintTree(indent + 2);
			}
		}

		public override Expression ResolveExpression()
		{
			m_expr = m_expr.ResolveExpression();

			if (m_expr.ExpressionType.IsArray)
			{
				int rank = m_expr.ExpressionType.GetArrayRank();

				if (m_expressionList.Count != rank)
				{
					throw new CompileException("Array rank mismatch", this);
				}

				if (rank != 1)
				{
					throw new NotImplementedException();
				}

				this.ExpressionType = m_expr.ExpressionType.GetElementType();

				for (int i = 0; i < m_expressionList.Count; i++)
				{
					m_expressionList[i] = m_expressionList[i].ResolveExpression();

					if (m_expressionList[i].ExpressionType != typeof(int))
					{
						throw new CompileException("Index is non-integer", this);
					}
				}
			}
			else
			{
				m_itemPropertyInfo = m_expr.ExpressionType.GetProperty("Item");

				if (m_itemPropertyInfo == null)
				{
					throw new CompileException("Indexing non indexable object", this);
				}

				MethodInfo getMethodInfo = m_itemPropertyInfo.GetGetMethod();
				this.ExpressionType = getMethodInfo.ReturnType;
				ParameterInfo[] parameters = getMethodInfo.GetParameters();

				if (m_expressionList.Count != parameters.Length)
				{
					throw new CompileException("Wrong number of indexes", this);
				}

				for (int i = 0; i < m_expressionList.Count; i++)
				{
					m_expressionList[i] = m_expressionList[i].ResolveExpression();

					if (m_expressionList[i].ExpressionType != parameters[i].ParameterType)
					{
						throw new CompileException("Wrong index type {0}, {1}", this,
							m_expressionList[i].ExpressionType, parameters[i].ParameterType);
					}
				}
			}

			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			if (m_expr.ExpressionType.IsArray)
			{
				m_expr.EmitExpression(ec);
				m_expressionList[0].EmitExpression(ec);
				ec.IlGen.Emit(OpCodes.Ldelem, this.ExpressionType);
			}
			else
			{
				MethodInfo getMethodInfo = m_itemPropertyInfo.GetGetMethod();
				m_expr.EmitExpression(ec);
				m_expressionList[0].EmitExpression(ec);
				ec.IlGen.EmitCall(OpCodes.Call, getMethodInfo, null);
			}
		}

		#region IAssignable Members

		public void EmitAssignment(EmitContext ec, bool isExpression, Expression rvalue)
		{
			LocalBuilder tempLocal = ec.IlGen.DeclareLocal(rvalue.ExpressionType);

			if (m_expr.ExpressionType.IsArray)
			{
				if (isExpression)
				{
					rvalue.EmitExpression(ec);
					ec.IlGen.Emit(OpCodes.Stloc, tempLocal);

					m_expr.EmitExpression(ec);
					m_expressionList[0].EmitExpression(ec);
					ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
					ec.IlGen.Emit(OpCodes.Stelem, this.ExpressionType);

					ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
				}
				else
				{
					m_expr.EmitExpression(ec);
					m_expressionList[0].EmitExpression(ec);
					rvalue.EmitExpression(ec);
					ec.IlGen.Emit(OpCodes.Stelem, this.ExpressionType);
				}
			}
			else
			{
				MethodInfo setMethodInfo = m_itemPropertyInfo.GetSetMethod();

				if (isExpression)
				{
					rvalue.EmitExpression(ec);
					ec.IlGen.Emit(OpCodes.Stloc, tempLocal);

					m_expr.EmitExpression(ec);
					m_expressionList[0].EmitExpression(ec);
					ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
					ec.IlGen.EmitCall(OpCodes.Call, setMethodInfo, null);

					ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
				}
				else
				{
					m_expr.EmitExpression(ec);
					m_expressionList[0].EmitExpression(ec);
					rvalue.EmitExpression(ec);
					ec.IlGen.EmitCall(OpCodes.Call, setMethodInfo, null);
				}
			}
		}

		#endregion
	}
}
