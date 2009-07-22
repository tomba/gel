#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Gel.Compiler.Ast
{
	class PropertyAccessExpression : Expression, IAssignable
	{
		PropertyInfo m_propertyInfo;
		Expression m_instanceExpression;

		public PropertyAccessExpression(PropertyInfo propertyInfo, Expression instanceExpression, Location loc)
		{
			m_propertyInfo = propertyInfo;
			m_instanceExpression = instanceExpression;
			this.ExpressionType = m_propertyInfo.PropertyType;
			this.Location = loc;
			this.ExpressionClass = ExpressionClass.PropertyAccess;
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "PropertyAccess ({0})", m_propertyInfo);
			if(m_instanceExpression != null)
				m_instanceExpression.PrintTree(indent + 2);
		}

		public override void EmitExpression(EmitContext ec)
		{
			MethodInfo getMethod = m_propertyInfo.GetGetMethod();

			if (getMethod == null)
			{
				throw new CompileException("Property has no get method", this);
			}

			if (m_instanceExpression != null)
			{
				m_instanceExpression.EmitExpression(ec);
			}

			ec.IlGen.EmitCall(OpCodes.Call, getMethod, null);
		}

		public void EmitAssignment(EmitContext ec, bool isExpression, Expression rvalue)
		{
			MethodInfo setMethod = m_propertyInfo.GetSetMethod();

			if (setMethod == null)
			{
				throw new CompileException("Property has no set method", this);
			}

			if (m_instanceExpression != null)
			{
				m_instanceExpression.EmitExpression(ec);
			}

			rvalue.EmitExpression(ec);

			ec.IlGen.EmitCall(OpCodes.Call, setMethod, null);

			if (isExpression)
				throw new NotImplementedException();
		}
	}
}
