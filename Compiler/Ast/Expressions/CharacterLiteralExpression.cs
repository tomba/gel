using System.Reflection.Emit;
namespace Gel.Compiler.Ast
{
	class CharacterLiteralExpression : Expression
	{
		char m_char;
		
		public CharacterLiteralExpression(char ch, Location loc)
		{
			m_char = ch;
			Location = loc;
			this.ExpressionType = typeof(char);
		}

		public override Expression ResolveExpression()
		{
			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			ec.IlGen.Emit(OpCodes.Ldc_I4, m_char);
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "Char '{0}' {1}", m_char, this.ExpressionType);
		}
	}
}
