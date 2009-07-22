using System;
using System.Reflection;
using Gel.Core;

namespace Gel.Compiler.Ast
{
	class MemberAccessExpression : Expression
	{
		Expression m_exp;
		string m_member;

		public MemberAccessExpression(Expression exp, string member, Location loc)
		{
			m_exp = exp;
			m_member = member;
			Location = loc;
		}

		public Expression Base
		{
			get { return m_exp; }
			set { m_exp = value; }
		}

		public string Member
		{
			get { return m_member; }
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "MemberAccess {0}", m_member);
			m_exp.PrintTree(indent + 2);
		}

		public override Expression ResolveExpression()
		{
			m_exp = m_exp.ResolveExpression();

			if (m_exp.ExpressionClass == ExpressionClass.Namespace)
			{
				string name = ((NamespaceAccessExpression)m_exp).Namespace + "." + m_member;

				Type t = ResolveContext.FindType(name);

				if (t != null)
				{
					return new TypeAccessExpression(t);
				}

				if (ResolveContext.FindNamespace(name))
				{
					return new NamespaceAccessExpression(name);
				}

				throw new CompileException("Unresolved: {0}", this, name);
			}
			else if (m_exp.ExpressionClass == ExpressionClass.Type)
			{
				MemberInfo[] mis = m_exp.ExpressionType.GetMember(m_member, BindingFlags.Public | BindingFlags.Static);
				if (mis.Length == 0)
				{
					throw new CompileException(
						String.Format("No '{0}' member in {1}", m_member, m_exp.ExpressionType.FullName),
						this.Location);
				}

				MemberTypes memberType = mis[0].MemberType;
				foreach (MemberInfo mi in mis)
				{
					if (memberType != mi.MemberType)
						throw new CompileException("Mismatching member types", this);
				}

				if (memberType == MemberTypes.NestedType)
				{
					throw new NotImplementedException();
				}
				else if (memberType == MemberTypes.Method)
				{
					MethodInfo[] methodInfos = new MethodInfo[mis.Length];
					Array.Copy(mis, methodInfos, mis.Length);
					return new MethodGroupExpression(methodInfos, null, this.Location);
				}
				else if (memberType == MemberTypes.Property)
				{
					return new PropertyAccessExpression((PropertyInfo)mis[0], null, this.Location);
				}
				else if (memberType == MemberTypes.Field)
				{
					FieldInfo finfo = (FieldInfo)mis[0];
					if (finfo.IsLiteral)
					{
						if(finfo.FieldType == typeof(int))
							return new IntegerLiteralExpression((int)finfo.GetRawConstantValue(), this.Location);
						// XXX convert the rest somehow. UnknownLiteral?
					}
					else
						return new FieldAccessExpression((FieldInfo)mis[0], null, this.Location);
				}

				throw new CompileException("Invalid member reference", this);
			}
			else if (m_exp.ExpressionClass == ExpressionClass.PropertyAccess ||
				m_exp.ExpressionClass == ExpressionClass.IndexerAccess ||
				m_exp.ExpressionClass == ExpressionClass.Variable ||
				m_exp.ExpressionClass == ExpressionClass.Value)
			{
				if (m_exp.ExpressionType == typeof(GelObject))
				{
					return new DynamicMemberAccessExpression(m_exp, m_member, this.Location);
				}

				MemberInfo[] mis = m_exp.ExpressionType.GetMember(m_member);

				if (mis.Length == 0)
				{
					throw new CompileException(
						String.Format("No '{0}' member in {1}", m_member, m_exp.ExpressionType.FullName),
						this.Location);
				}

				MemberTypes memberType = mis[0].MemberType;
				foreach (MemberInfo mi in mis)
				{
					if (memberType != mi.MemberType)
						throw new CompileException("Mismatching member types", this);
				}

				if (memberType == MemberTypes.NestedType)
				{
					throw new NotImplementedException();
				}
				else if (memberType == MemberTypes.Method)
				{
					MethodInfo[] methodInfos = new MethodInfo[mis.Length];
					Array.Copy(mis, methodInfos, mis.Length);
					return new MethodGroupExpression(methodInfos, m_exp, this.Location);
				}
				else if (memberType == MemberTypes.Property)
				{
					return new PropertyAccessExpression((PropertyInfo)mis[0], m_exp, this.Location);
				}
				else if (memberType == MemberTypes.Field)
				{
					FieldInfo fieldInfo = (FieldInfo)mis[0];
					if (fieldInfo.IsLiteral)
					{
						if (fieldInfo.FieldType == typeof(int))
							return new IntegerLiteralExpression((int)fieldInfo.GetRawConstantValue(), this.Location);
						// XXX convert the rest somehow. UnknownLiteral?
					}
					else
						return new FieldAccessExpression(fieldInfo, m_exp, this.Location);
				}

				throw new CompileException("Invalid member reference", this);
			}
				/*
			else if (m_exp is DynamicName)
			{
				string name = ((DynamicName)m_exp).Name + "." + m_member;

				return new DynamicName(name, this.Location);
			}
				 */
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}
