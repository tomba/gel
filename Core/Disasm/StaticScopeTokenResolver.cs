using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace ClrTest.Reflection
{
	public class StaticScopeTokenResolver : ITokenResolver
	{
		Module m_module;

		public StaticScopeTokenResolver(MethodInfo method)
		{
			m_module = method.Module;
		}

		public String AsString(int token)
		{
			return m_module.ResolveString(token);
		}

		public FieldInfo AsField(int token)
		{
			return m_module.ResolveField(token);
		}

		public Type AsType(int token)
		{
			return m_module.ResolveType(token);
		}

		public MethodBase AsMethod(int token)
		{
			return m_module.ResolveMethod(token);
		}

		public MemberInfo AsMember(int token)
		{
			return m_module.ResolveMember(token);
		}

		public byte[] AsSignature(int token)
		{
			return m_module.ResolveSignature(token);
		}
	}
}
