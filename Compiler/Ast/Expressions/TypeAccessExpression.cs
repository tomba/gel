#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Gel.Compiler.Ast
{
	class TypeAccessExpression : Expression
	{
		public TypeAccessExpression(Type type)
		{
			this.ExpressionType = type;
			this.ExpressionClass = ExpressionClass.Type;
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "TypeAccess {0}", this.ExpressionType);
		}
	}
}
