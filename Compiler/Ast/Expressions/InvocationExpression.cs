#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

#endregion

namespace Gel.Compiler.Ast
{
	class InvocationExpression : Expression
	{
		Expression m_target;
		List<Expression> m_arguments;

		public InvocationExpression(Expression target, List<Expression> arguments, Location loc)
		{
			m_target = target;
			m_arguments = arguments;
			if (arguments == null)
				m_arguments = new List<Expression>();
			this.Location = loc;
		}

		public Expression Target
		{
			get { return m_target; }
		}

		public List<Expression> Arguments
		{
			get { return m_arguments; }
			set { m_arguments = value; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "Invocation");
			m_target.PrintTree(indent + 2);
			foreach (Expression arg in m_arguments)
			{
				arg.PrintTree(indent + 2);
			}
		}

		public override Expression ResolveExpression()
		{
			m_target = m_target.ResolveExpression();

			for (int i = 0; i < m_arguments.Count; i++)
			{
				m_arguments[i] = m_arguments[i].ResolveExpression();
			}

			if (m_target is MethodGroupExpression)
			{
				#region MethodGroup
				MethodGroupExpression mg = (MethodGroupExpression)m_target;

				if (mg.InstanceExpression == null)
				{
					// static access

					foreach (MethodInfo methodInfo in mg.MethodInfoArray)
					{
						ParameterInfo[] parameters = methodInfo.GetParameters();

						if (parameters.Length != m_arguments.Count)
						{
							continue;
						}

						//XXX eka exact match, sit muita

						bool match = true;
						for (int i = 0; i < parameters.Length; i++)
						{
							if (parameters[i].ParameterType != m_arguments[i].ExpressionType)
							{
								if (parameters[i].ParameterType == typeof(object))
								{
									if (m_arguments[i].ExpressionType.IsValueType)
									{
										m_arguments[i] = new BoxExpression(m_arguments[i], m_arguments[i].Location);
									}
								}
								else
								{
									match = false;
									break;
								}
							}
						}

						if (match == false)
						{
							continue;
						}

						return new MethodCallExpression(methodInfo, null, m_arguments, this.Location);
					}

					throw new CompileException("No suitable member found", this.Location);
				}
				else
				{
					// instance call

					foreach (MethodInfo methodInfo in mg.MethodInfoArray)
					{
						ParameterInfo[] parameters = methodInfo.GetParameters();

						if (parameters.Length != m_arguments.Count)
						{
							continue;
						}

						bool match = true;
						for (int i = 0; i < parameters.Length; i++)
						{
							if (parameters[i].ParameterType != m_arguments[i].ExpressionType)
							{
								match = false;
								break;
							}
						}

						if (match == false)
						{
							continue;
						}

						return new MethodCallExpression(methodInfo, mg.InstanceExpression, m_arguments, this.Location);
					}

					throw new CompileException("No suitable member found", this.Location);
				}
				#endregion
			}
			else if (m_target is InternalMethodGroup)
			{
				#region InternalMethodGroup
				InternalMethodGroup mg = (InternalMethodGroup)m_target;

				foreach (MethodDeclaration methodInfo in mg.MethodArray)
				{
					List<LocalVariable> parameters = methodInfo.Parameters;

					if (parameters.Count != m_arguments.Count)
					{
						continue;
					}

					if (methodInfo.IsInstance && mg.InstanceExpression == null)
					{
						continue;
					}

					bool match = true;
					for (int i = 0; i < parameters.Count; i++)
					{
						if (ImplicitConversionPossible(parameters[i].VariableType, m_arguments[i]) == false)
						{
							match = false;
							break;
						}
					}

					if (match == false)
					{
						continue;
					}

					for (int i = 0; i < parameters.Count; i++)
					{
						m_arguments[i] = ImplicitConversion(parameters[i].VariableType, m_arguments[i]);
					}

					return new InternalMethodCall(methodInfo, methodInfo.IsInstance ? mg.InstanceExpression : null, m_arguments, this.Location);
				}

				throw new CompileException("No suitable member found", this.Location);
				#endregion
			}
			else if (m_target is DynamicMemberAccessExpression)
			{
				DynamicMemberAccessExpression dynAccess = (DynamicMemberAccessExpression)m_target;

				return new DynamicMethodCall(dynAccess.MemberName, dynAccess.InstanceExpression, m_arguments, this.Location);
			}
			else if (m_target is SimpleName)
			{
				throw new CompileException(String.Format("{0} not resolved", ((SimpleName)m_target).Name), m_target.Location);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}
