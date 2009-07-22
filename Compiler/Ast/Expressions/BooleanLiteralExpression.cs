using System.Reflection.Emit;
namespace Gel.Compiler.Ast
{
	class BooleanLiteralExpression : Expression
	{
		private bool	m_bool;
		
		public BooleanLiteralExpression(bool b, Location loc)
		{
			m_bool = b;
			Location = loc;
			this.ExpressionType = typeof(bool);
		}

		public bool BooleanValue
		{
			get { return m_bool; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, m_bool.ToString() + " {0}", this.ExpressionType);
		}

		public override Expression ResolveExpression()
		{
			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			if (m_bool == true)
			{
				ec.IlGen.Emit(OpCodes.Ldc_I4_1);
			}
			else
			{
				ec.IlGen.Emit(OpCodes.Ldc_I4_0);
			}
		}
	}
}
