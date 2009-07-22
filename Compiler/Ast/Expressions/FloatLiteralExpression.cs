using System;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class FloatLiteralExpression : Expression
	{
		float m_number;

		public FloatLiteralExpression(string num, Location loc)
		{
			m_number = float.Parse(num);
			this.Location = loc;
			this.ExpressionType = typeof(float);
		}

		public float Number
		{
			get { return m_number; }
		}


		public override string ToString()
		{
			return String.Format("Int {0} {1}", m_number.ToString(), this.ExpressionType);
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
			ec.IlGen.Emit(OpCodes.Ldc_R4, m_number);
		}
	}
}
