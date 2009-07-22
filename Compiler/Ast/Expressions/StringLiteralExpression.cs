using System.Reflection.Emit;
namespace Gel.Compiler.Ast
{
	class StringLiteralExpression : Expression
	{
		private string m_string;
		
		public StringLiteralExpression(string str, Location loc)
		{
			m_string = str;
			Location = loc;
			this.ExpressionType = typeof(string);
		}

		public string Text
		{
			get { return m_string; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "\"{0}\" {1}", m_string, this.ExpressionType);
		}

		public override Expression ResolveExpression()
		{
			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			ec.IlGen.Emit(OpCodes.Ldstr, m_string);
		}
	}
}
