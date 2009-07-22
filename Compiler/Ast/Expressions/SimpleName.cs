using System;
using System.Reflection;

namespace Gel.Compiler.Ast
{
	class SimpleName : Expression
	{
		string m_name;

		public SimpleName(string name, Location loc)
		{
			m_name = name;
			Location = loc;
		}

		public string Name
		{
			get
			{
				return m_name;
			}
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "SimpleName '{0}' {1}", m_name, this.ExpressionType);
		}

		public override Expression ResolveExpression()
		{
			// TODO: 
			#region SimpleName definition
			//  See C# Reference 7.5.2, 7.3
			//•	If the simple-name appears within a block and if the block’s (or an enclosing block’s) local variable declaration space (§?3.3) contains a local variable or parameter with the given name, then the simple-name refers to that local variable or parameter and is classified as a variable.
			//•	Otherwise, for each type T, starting with the immediately enclosing class, struct, or enumeration declaration and continuing with each enclosing outer class or struct declaration (if any), if a member lookup of the simple-name in T produces a match:
			//	o	If T is the immediately enclosing class or struct type and the lookup identifies one or more methods, the result is a method group with an associated instance expression of this.
			//	o	If T is the immediately enclosing class or struct type, if the lookup identifies an instance member, and if the reference occurs within the block of an instance constructor, an instance method, or an instance accessor, the result is the same as a member access (§?7.5.4) of the form this.E, where E is the simple-name.
			//	o	Otherwise, the result is the same as a member access (§?7.5.4) of the form T.E, where E is the simple-name. In this case, it is a compile-time error for the simple-name to refer to an instance member.
			//•	Otherwise, starting with the namespace in which the simple-name occurs, continuing with each enclosing namespace (if any), and ending with the global namespace, the following steps are evaluated until an entity is located:
			//	o	If the namespace contains a namespace member with the given name, then the simple-name refers to that member and, depending on the member, is classified as a namespace or a type.
			//	o	Otherwise, if the namespace has a corresponding namespace declaration enclosing the location where the simple-name occurs, then:
			//•	If the namespace declaration contains a using-alias-directive that associates the given name with an imported namespace or type, then the simple-name refers to that namespace or type.
			//•	Otherwise, if the namespaces imported by the using-namespace-directives of the namespace declaration contain exactly one type with the given name, then the simple-name refers to that type.
			//•	Otherwise, if the namespaces imported by the using-namespace-directives of the namespace declaration contain more than one type with the given name, then the simple-name is ambiguous and a compile-time error occurs.
			//•	Otherwise, the name given by the simple-name is undefined and a compile-time error occurs.
			#endregion

			// Local variable
			LocalVariable local = ResolveContext.CurrentBlock.FindLocal(m_name);

			if (local != null)
			{
				return new LocalAccessExpression(local);
			}

			// Local const variable
			LocalConst localConst = ResolveContext.CurrentBlock.FindLocalConst(m_name);

			if (localConst != null)
			{
				return localConst.ValueExpression;
			}
			
			// Method argument
			LocalVariable param = ResolveContext.CurrentMethod.FindParam(m_name);

			if (param != null)
			{
				return new LocalAccessExpression(param);
			}

			// Method in current program
			MethodDeclaration method = ResolveContext.CurrentProgram.FindMethod(m_name);
			if (method != null)
			{
				Expression instExp = null;
				if(ResolveContext.CurrentMethod.IsInstance)
				{
					instExp = new ThisExpression(this.Location);
				}

				return new InternalMethodGroup(new MethodDeclaration[] { method },
					instExp, this.Location);
			}

			// Field in current program
			FieldDeclaration field = ResolveContext.CurrentProgram.FindField(m_name);
			if (field != null)
			{
				if (field.IsConst)
				{
					return field.InitExpression;
				}

				Expression instExp = null;
				if (ResolveContext.CurrentMethod.IsInstance)
				{
					instExp = new ThisExpression(this.Location);
				}

				return new InternalFieldAccessExpression(field,
					instExp, this.Location);
			}

			// Type
			Type type = ResolveContext.FindType(m_name);
			if (type != null)
			{
				return new TypeAccessExpression(type);
			}

			// Namespace
			if (ResolveContext.FindNamespace(m_name))
			{
				return new NamespaceAccessExpression(m_name);
			}

			if (ResolveContext.TypeHint != null)
			{
				LocalVariable newLocal = ResolveContext.CurrentMethod.CreateLocalVariable(m_name, ResolveContext.TypeHint, this.Location);
				ResolveContext.CurrentBlock.AddLocalVariable(newLocal);
				return new LocalAccessExpression(newLocal);
			}
			
			throw new CompileException("Unresolved: {0}", this, m_name);
		}
	}
}
