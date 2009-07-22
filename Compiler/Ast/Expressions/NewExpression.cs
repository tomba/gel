#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Gel.Compiler.Ast
{
	class NewExpression : Expression
	{
		List<Expression> m_arguments;
		ConstructorInfo m_constructorInfo;

		public NewExpression(Type type, List<Expression> arguments, Location loc)
		{
			this.ExpressionType = type;
			m_arguments = arguments;
			if (m_arguments == null)
				m_arguments = new List<Expression>();
			this.Location = loc;
		}

		public List<Expression> Arguments
		{
			get { return m_arguments; }
			set { m_arguments = value; }
		}

		public ConstructorInfo ConstructorInfo
		{
			get { return m_constructorInfo; }
			set { m_constructorInfo = value; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "New {0}", this.ExpressionType);
			foreach (Expression arg in m_arguments)
			{
				arg.PrintTree(indent + 2);
			}
		}

		public override Expression ResolveExpression()
		{
			for (int i = 0; i < m_arguments.Count; i++)
			{
				m_arguments[i] = m_arguments[i].ResolveExpression();
			}

			List<Type> argTypes = new List<Type>();
			foreach (Expression arg in m_arguments)
			{
				argTypes.Add(arg.ExpressionType);
			}

			ConstructorInfo ci = this.ExpressionType.GetConstructor(argTypes.ToArray());

			if (ci == null)
			{
				throw new CompileException("No suitable constructor in " + this.ExpressionType.FullName, this.Location);
			}

			m_constructorInfo = ci;

			return this;
		}

		public override void EmitExpression(EmitContext ec)
		{
			foreach (Expression arg in m_arguments)
			{
				arg.EmitExpression(ec);
			}

			ec.IlGen.Emit(OpCodes.Newobj, m_constructorInfo);
		}
	}
}
