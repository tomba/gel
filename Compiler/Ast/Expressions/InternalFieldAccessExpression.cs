#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Gel.Compiler.Ast
{
	class InternalFieldAccessExpression : Expression, IAssignable
	{
		FieldDeclaration m_field;
		Expression m_instanceExp;

		public InternalFieldAccessExpression(FieldDeclaration field, Expression instanceExp, Location loc)
		{
			m_field = field;
			m_instanceExp = instanceExp;
			this.ExpressionType = m_field.FieldType;
			this.Location = loc;
			this.ExpressionClass = ExpressionClass.Variable;
		}

		public FieldDeclaration Field
		{
			get { return m_field; }
		}

		public Expression InstanceExpression
		{
			get { return m_instanceExp; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "InternalFieldAccess {0} {1}", m_field.Name, m_field.FieldType);

			if(m_instanceExp != null)
				m_instanceExp.PrintTree(indent+2);
		}

		public override void EmitExpression(EmitContext ec)
		{
			if (m_field.IsStatic)
			{
				Type programType = typeof(Gel.Core.GelProgram);

				FieldInfo fi = programType.GetField("s_currentProgram");

				FieldInfo fieldInfo = programType.GetField("m_staticFieldValues",
					BindingFlags.Public | BindingFlags.Instance);

				ec.IlGen.Emit(OpCodes.Ldsfld, fi);
				ec.IlGen.Emit(OpCodes.Ldfld, fieldInfo);

				ec.IlGen.Emit(OpCodes.Ldc_I4, m_field.Ordinal);
				ec.IlGen.Emit(OpCodes.Ldelem, typeof(object));

				ec.IlGen.Emit(OpCodes.Unbox_Any, m_field.FieldType);

			}
			else
			{
				Type objectType = typeof(Gel.Core.GelObject);

				FieldInfo fieldInfo = objectType.GetField("m_instanceFieldValues",
					BindingFlags.Public | BindingFlags.Instance);

				m_instanceExp.EmitExpression(ec);
				ec.IlGen.Emit(OpCodes.Ldfld, fieldInfo);

				ec.IlGen.Emit(OpCodes.Ldc_I4, m_field.Ordinal);
				ec.IlGen.Emit(OpCodes.Ldelem, typeof(object));

				ec.IlGen.Emit(OpCodes.Unbox_Any, m_field.FieldType);
			}
		}

		#region IAssignable Members

		public void EmitAssignment(EmitContext ec, bool isExpression, Expression rvalue)
		{
			if (m_field.IsStatic)
			{
				Type programType = typeof(Gel.Core.GelProgram);

				FieldInfo fi = programType.GetField("s_currentProgram");

				FieldInfo fieldInfo = programType.GetField("m_staticFieldValues",
					BindingFlags.Public | BindingFlags.Instance);

				LocalBuilder tempLocal = ec.IlGen.DeclareLocal(rvalue.ExpressionType);

				rvalue.EmitExpression(ec);
				ec.IlGen.Emit(OpCodes.Stloc, tempLocal);


				ec.IlGen.Emit(OpCodes.Ldsfld, fi);
				ec.IlGen.Emit(OpCodes.Ldfld, fieldInfo);

				ec.IlGen.Emit(OpCodes.Ldc_I4, m_field.Ordinal);

				ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
				if (rvalue.ExpressionType.IsValueType)
				{
					ec.IlGen.Emit(OpCodes.Box, rvalue.ExpressionType);
				}

				ec.IlGen.Emit(OpCodes.Stelem, typeof(object));
				
				if (isExpression)
				{
					ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
				}
			}
			else
			{
				Type objectType = typeof(Gel.Core.GelObject);

				FieldInfo fieldInfo = objectType.GetField("m_instanceFieldValues",
					BindingFlags.Public | BindingFlags.Instance);

				LocalBuilder tempLocal = ec.IlGen.DeclareLocal(rvalue.ExpressionType);

				rvalue.EmitExpression(ec);
				ec.IlGen.Emit(OpCodes.Stloc, tempLocal);


				m_instanceExp.EmitExpression(ec);
				ec.IlGen.Emit(OpCodes.Ldfld, fieldInfo);

				ec.IlGen.Emit(OpCodes.Ldc_I4, m_field.Ordinal);

				ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
				if (rvalue.ExpressionType.IsValueType)
				{
					ec.IlGen.Emit(OpCodes.Box, rvalue.ExpressionType);
				}

				ec.IlGen.Emit(OpCodes.Stelem, typeof(object));
				
				if (isExpression)
				{
					ec.IlGen.Emit(OpCodes.Ldloc, tempLocal);
				}
			}
		}

		#endregion
	}
}
