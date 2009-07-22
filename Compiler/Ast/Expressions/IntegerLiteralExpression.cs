using System;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class IntegerLiteralExpression : Expression
	{
		ulong m_number;

		public IntegerLiteralExpression(int num, Location loc)
		{
			m_number = (ulong)num;
			this.Location = loc;
			this.ExpressionType = typeof(int);
		}

		// parse hexadecimal, integer and binary literals
		public IntegerLiteralExpression(string numstr, Location loc)
		{
			this.Location = loc;

			bool isUnsigned = false;
			bool isLong = false;
			bool isHex = false;
			bool isBinary = false;

			numstr = numstr.ToUpper();

			if (numstr.EndsWith("UL") || numstr.EndsWith("LU"))
			{
				isUnsigned = true;
				isLong = true;
				numstr = numstr.Substring(0, numstr.Length - 2);
			}
			else if (numstr.EndsWith("L"))
			{
				isLong = true;
				numstr = numstr.Substring(0, numstr.Length - 1);
			}
			else if(numstr.EndsWith("U"))
			{
				isUnsigned = true;
				numstr = numstr.Substring(0, numstr.Length - 1);
			}

			ulong b = 10;

			if (numstr.StartsWith("0X"))
			{
				isHex = true;
				b = 16;
				numstr = numstr.Substring(2);
			}
			else if (numstr.StartsWith("0B"))
			{
				isBinary = true;
				b = 2;
				numstr = numstr.Substring(2);
			}

			ulong number = 0;

			try
			{
				checked
				{
					ulong exp = 1;

					for (int i = numstr.Length - 1; i >= 0; i--)
					{
						ulong n;
						char numchar = numstr[i];
						if (numchar >= '0' && numchar <= '9')
							n = (ulong)(numchar - '0');
						else if (numchar >= 'A' && numchar <= 'F')
							n = (ulong)(numchar - 'A' + 10);
						else
							throw new CompileException("Int literal parse error", Location);

						number += n * exp;
						if(i > 0) // this can overflow in the last loop, so pass it
							exp *= b;
					}
				}
			}
			catch (OverflowException)
			{
				throw new CompileException("Number too big", Location);
			}

			if (number <= int.MaxValue)
				this.ExpressionType = typeof(int);
			else if (number <= uint.MaxValue)
				this.ExpressionType = typeof(uint);
			else if (number <= long.MaxValue)
				this.ExpressionType = typeof(long);
			else
				this.ExpressionType = typeof(ulong);

			if(isUnsigned && isLong)
				this.ExpressionType = typeof(ulong);
			else if (isLong)
			{
				if (number > long.MaxValue)
					throw new CompileException("Number does not fit in a long", Location);
				this.ExpressionType = typeof(long);
			}
			else if (isUnsigned)
			{
				if (number > uint.MaxValue)
					throw new CompileException("Number does not fit in a uint", Location);
				this.ExpressionType = typeof(uint);
			}

			m_number = number;
		}

		public int Number
		{
			get { return (int)m_number; }
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
			if (this.ExpressionType == typeof(int) || this.ExpressionType == typeof(uint))
			{
				int n = (int)m_number;
				if (n == 0)
					ec.IlGen.Emit(OpCodes.Ldc_I4_0);
				else if (n == 1)
					ec.IlGen.Emit(OpCodes.Ldc_I4_1);
				else if (n == 2)
					ec.IlGen.Emit(OpCodes.Ldc_I4_2);
				else if (n == 3)
					ec.IlGen.Emit(OpCodes.Ldc_I4_3);
				else if (n == 4)
					ec.IlGen.Emit(OpCodes.Ldc_I4_4);
				else if (n == 5)
					ec.IlGen.Emit(OpCodes.Ldc_I4_5);
				else if (n == 6)
					ec.IlGen.Emit(OpCodes.Ldc_I4_6);
				else if (n == 7)
					ec.IlGen.Emit(OpCodes.Ldc_I4_7);
				else if (n == 8)
					ec.IlGen.Emit(OpCodes.Ldc_I4_8);
				else if (n == -1)
					ec.IlGen.Emit(OpCodes.Ldc_I4_M1);
				else
					ec.IlGen.Emit(OpCodes.Ldc_I4, n);
			}
			else
			{
				ec.IlGen.Emit(OpCodes.Ldc_I8, (long)m_number);
			}
		}
	}
}
