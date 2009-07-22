using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Gel.Core
{
	public class GelMethod
	{
		MethodInfo m_methodInfo;
		Modifiers m_modifiers;
		DynamicMethodDelegate m_methodDelegate;

		public GelMethod(MethodInfo methodInfo, Modifiers modifiers)
		{
			m_methodInfo = methodInfo;
			m_modifiers = modifiers;
		}

		public MethodInfo MethodInfo
		{
			get { return m_methodInfo; }
		}

		public string Name
		{
			get { return m_methodInfo.Name; }
		}

		public bool IsStatic
		{
			get { return (m_modifiers & Modifiers.Static) != 0; }
		}

		public object Invoke(object instance, params object[] args)
		{
			if(m_methodDelegate == null)
				m_methodDelegate = DynamicMethodDelegateFactory.Create(this);

			return m_methodDelegate(instance, args);
		}
	}
}
