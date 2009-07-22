#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Gel.Compiler.Ast
{
	/* Method call between objects. Dynamic typing */
	class DynamicMethodCall : Expression
	{
		string m_methodName;
		Expression m_instanceExpr;
		List<Expression> m_arguments;

		public DynamicMethodCall(string methodName, Expression instanceExpr, List<Expression> arguments, Location loc)
		{
			m_methodName = methodName;
			m_instanceExpr = instanceExpr;
			m_arguments = arguments;
			this.Location = loc;
			this.ExpressionType = typeof(object);
		}

		public string TargetMethodName
		{
			get { return m_methodName; }
		}

		public Expression InstanceExpression
		{
			get { return m_instanceExpr; }
		}

		public List<Expression> Arguments
		{
			get { return m_arguments; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "DynamicMethodCall {0} {1}", m_methodName, this.ExpressionType);
			if (m_instanceExpr != null)
				m_instanceExpr.PrintTree(indent + 2);
			foreach (Expression arg in m_arguments)
			{
				arg.PrintTree(indent + 2);
			}
		}

		public override void EmitExpression(EmitContext ec)
		{
			m_instanceExpr.EmitExpression(ec);

			ec.IlGen.Emit(OpCodes.Ldstr, m_methodName);

			// Create arg array
			LocalBuilder tempLocal = ec.IlGen.DeclareLocal(typeof(object[]));
			ec.IlGen.Emit(OpCodes.Ldc_I4, m_arguments.Count);
			ec.IlGen.Emit(OpCodes.Newarr, typeof(object));
			ec.IlGen.Emit(OpCodes.Stloc, tempLocal);
			for(int i = 0; i < m_arguments.Count; i++)
			{
				ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
				ec.IlGen.Emit(OpCodes.Ldc_I4, i);
				m_arguments[i].EmitExpression(ec);
				if (m_arguments[i].ExpressionType.IsValueType)
				{
					ec.IlGen.Emit(OpCodes.Box, m_arguments[i].ExpressionType);
				}
				ec.IlGen.Emit(OpCodes.Stelem, typeof(object));
			}
			ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);

			ec.IlGen.EmitCall(OpCodes.Call, typeof(Gel.Core.GelObject).GetMethod("Invoke"), null);
		}
	}
}
