using System;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class TypeConversionExpression : Expression
	{
		Expression m_expr;

		static Type[] s_dstTypes = new Type[] { 
				typeof(byte), typeof(sbyte), 
				typeof(ushort), typeof(short),
				typeof(uint), typeof(int),
				typeof(ulong), typeof(long),
				typeof(float), typeof(double)
			};
		static OpCode[] s_convCode = new OpCode[] {
				OpCodes.Conv_U1, OpCodes.Conv_I1, 
				OpCodes.Conv_U2, OpCodes.Conv_I2,
				OpCodes.Conv_U4, OpCodes.Conv_I4,
				OpCodes.Conv_U8, OpCodes.Conv_I8,
				OpCodes.Conv_R4, OpCodes.Conv_R8
			};

		public TypeConversionExpression(Type destType, Expression expr, Location loc)
		{
			m_expr = expr;
			this.ExpressionType = destType;
			Location = loc;

			if (!destType.IsValueType || !m_expr.ExpressionType.IsValueType)
			{
				throw new Exception("faaaail");
			}
		}

		public Expression Expr
		{
			set { m_expr = value; }
			get { return m_expr; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "TypeConversion {0}", this.ExpressionType);

			m_expr.PrintTree(indent + 2);
		}

		public override void EmitExpression(EmitContext ec)
		{
			m_expr.EmitExpression(ec);

			int idx = Array.IndexOf<Type>(s_dstTypes, this.ExpressionType);

			if (idx == -1)
				throw new NotImplementedException();

			OpCode c = s_convCode[idx];
			ec.IlGen.Emit(c);
		}
	}
}
