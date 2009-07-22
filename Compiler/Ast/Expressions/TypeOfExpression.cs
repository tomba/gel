using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class TypeOfExpression : Expression
	{
		Type m_type;

		public TypeOfExpression(Type type, Location loc)
		{
			m_type = type;
			this.ExpressionType = typeof(Type);
			Location = loc;
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "TypeOf {0}", this.ExpressionType);
			Output(indent + 2, m_type.ToString());
		}

		public override Expression ResolveExpression()
		{
			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			ec.IlGen.Emit(OpCodes.Ldtoken, m_type);
			ec.IlGen.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), null);
		}
	}
}
