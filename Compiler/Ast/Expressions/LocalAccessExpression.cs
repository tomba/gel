#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;

#endregion

namespace Gel.Compiler.Ast
{
	class LocalAccessExpression : Expression, IAssignable
	{
		private LocalVariable m_local;

		public LocalAccessExpression(LocalVariable local)
		{
			m_local = local;
			this.ExpressionType = m_local.VariableType;
			this.ExpressionClass = ExpressionClass.Variable;
		}

		public override string ToString()
		{
			return String.Format("LocalAccess {0} {1}", m_local.Name, this.ExpressionType);
		}

		public override void PrintTree(int indent)
		{
			Output(indent, ToString());
		}

		public override Expression ResolveExpression()
		{
			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			if (m_local.IsParameter)
			{
				if (m_local.Ordinal == 0)
					ec.IlGen.Emit(OpCodes.Ldarg_0);
				else if (m_local.Ordinal == 1)
					ec.IlGen.Emit(OpCodes.Ldarg_1);
				else if (m_local.Ordinal == 2)
					ec.IlGen.Emit(OpCodes.Ldarg_2);
				else if (m_local.Ordinal == 3)
					ec.IlGen.Emit(OpCodes.Ldarg_3);
				else if (m_local.Ordinal <= 255)
					ec.IlGen.Emit(OpCodes.Ldarg_S, m_local.Ordinal);
				else
					ec.IlGen.Emit(OpCodes.Ldarg, m_local.Ordinal);
			}
			else
			{
				ec.IlGen.Emit(OpCodes.Ldloc, m_local.LocalBuilder);
			}
		}

		public void EmitAssignment(EmitContext ec, bool isExpression, Expression rvalue)
		{
			rvalue.EmitExpression(ec);

			if (isExpression)
			{
				ec.IlGen.Emit(OpCodes.Dup);
			}

			if (m_local.IsParameter)
			{
				if (m_local.Ordinal <= 255)
					ec.IlGen.Emit(OpCodes.Starg_S, m_local.Ordinal);
				else
					ec.IlGen.Emit(OpCodes.Starg, m_local.Ordinal);
			}
			else
			{
				ec.IlGen.Emit(OpCodes.Stloc, m_local.LocalBuilder);
			}
		}
	}
}
