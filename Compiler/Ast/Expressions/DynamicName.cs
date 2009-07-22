using System;
using System.Reflection.Emit;

/*
 * $var style variables. is this needed? (parser.g, MemberAccess...)
 */
namespace Gel.Compiler.Ast
{
	class DynamicName : Expression, IAssignable
	{
		string m_name;

		public DynamicName(string name, Location loc)
		{
			m_name = name;
			Location = loc;
			this.ExpressionType = typeof(object);
		}

		public string Name
		{
			get
			{
				return m_name;
			}
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "DynamicName '{0}' {1}", m_name, this.ExpressionType);
		}

		public override Expression ResolveExpression()
		{
			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			ec.IlGen.Emit(OpCodes.Ldstr, m_name);
			ec.IlGen.EmitCall(OpCodes.Call, typeof(Runtime).GetMethod("GetVariable"), null);

		}

		public void EmitAssignment(EmitContext ec, bool isExpression, Expression rvalue)
		{
			rvalue.EmitExpression(ec);

			if (isExpression)
			{
				ec.IlGen.Emit(OpCodes.Dup);
			}

			ec.IlGen.Emit(OpCodes.Ldstr, m_name);

			ec.IlGen.EmitCall(OpCodes.Call, typeof(Runtime).GetMethod("SetVariableReverse"), null);
		}
	}
}
