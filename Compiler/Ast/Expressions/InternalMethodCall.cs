#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Gel.Compiler.Ast
{
	/* Method call inside the same object. Strictly typed */
	class InternalMethodCall : Expression
	{
		MethodDeclaration m_methodInfo;
		Expression m_instanceExpr;
		List<Expression> m_arguments;

		public InternalMethodCall(MethodDeclaration methodInfo, Expression instanceExpr, List<Expression> arguments, Location loc)
		{
			m_methodInfo = methodInfo;
			m_instanceExpr = instanceExpr;
			m_arguments = arguments;
			this.Location = loc;
			this.ExpressionType = m_methodInfo.ReturnType;
		}

		public MethodDeclaration TargetMethodInfo
		{
			get { return m_methodInfo; }
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
			Output(indent, "InternalMethodCall {0} {1}", m_methodInfo.Name, this.ExpressionType);
			if (m_instanceExpr != null)
				m_instanceExpr.PrintTree(indent + 2);
			foreach (Expression arg in m_arguments)
			{
				arg.PrintTree(indent + 2);
			}
		}

		public override void EmitExpression(EmitContext ec)
		{
			System.Diagnostics.Debug.Assert(m_methodInfo.ReturnType != typeof(void));

			if (m_instanceExpr != null)
			{
				m_instanceExpr.EmitExpression(ec);
			}

			foreach (Expression arg in m_arguments)
			{
				arg.EmitExpression(ec);
			}

			ec.IlGen.EmitCall(OpCodes.Call, m_methodInfo.MethodInfo, null);
		}

		public override void EmitStatement(EmitContext ec)
		{
			if (m_instanceExpr != null)
			{
				m_instanceExpr.EmitExpression(ec);
			}

			foreach (Expression arg in m_arguments)
			{
				arg.EmitExpression(ec);
			}

			ec.IlGen.EmitCall(OpCodes.Call, m_methodInfo.MethodInfo, null);

			if (m_methodInfo.ReturnType != typeof(void))
			{
				ec.IlGen.Emit(OpCodes.Pop);
			}
		}
	}
}
