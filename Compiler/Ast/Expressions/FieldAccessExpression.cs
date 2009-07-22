#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Gel.Compiler.Ast
{
	class FieldAccessExpression : Expression, IAssignable
	{
		FieldInfo m_fieldInfo;
		Expression m_instanceExpression;

		public FieldAccessExpression(FieldInfo fieldInfo, Expression instanceExpression, Location loc)
		{
			m_fieldInfo = fieldInfo;
			m_instanceExpression = instanceExpression;
			this.ExpressionType = m_fieldInfo.FieldType;
			this.Location = loc;
			this.ExpressionClass = ExpressionClass.Variable;
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "FieldAccess ({0})", m_fieldInfo);
			if(m_instanceExpression != null)
				m_instanceExpression.PrintTree(indent + 2);
		}

		public override void EmitExpression(EmitContext ec)
		{
			if (m_instanceExpression != null)
			{
				m_instanceExpression.EmitExpression(ec);
			}

			ec.IlGen.Emit(OpCodes.Ldfld, m_fieldInfo);
		}

		public void EmitAssignment(EmitContext ec, bool isExpression, Expression rvalue)
		{
			if (m_instanceExpression != null)
			{
				m_instanceExpression.EmitExpression(ec);
			}

			rvalue.EmitExpression(ec);

			ec.IlGen.Emit(OpCodes.Stfld, m_fieldInfo);

			if (isExpression)
				throw new NotImplementedException();
		}
	}
}
