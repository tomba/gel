using System;

namespace Gel.Compiler.Ast
{
	/// <summary>
	/// Summary description for BinaryExpression.
	/// </summary>
	abstract class BinaryExpression : Expression
	{
		protected Expression m_left;
		protected Expression m_right;

		public BinaryExpression()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public Expression Left
		{
			get
			{
				return m_left;
			}

			set
			{
				m_left = value;
			}
		}

		public Expression Right
		{
			get
			{
				return m_right;
			}

			set
			{
				m_right = value;
			}
		}

	}
}
