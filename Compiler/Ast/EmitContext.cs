using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Gel.Compiler.Ast
{
	class EmitContext
	{
		MethodInfo m_methodInfo;
		ILGenerator m_ilGen;
		bool m_emitSymbols = false;

		public MethodInfo MethodInfo
		{
			get { return m_methodInfo; }
			set { m_methodInfo = value; }
		}

		public ILGenerator IlGen
		{
			get { return m_ilGen; }
			set { m_ilGen = value; }
		}

		public bool EmitSymbols
		{
			get { return m_emitSymbols; }
			set { m_emitSymbols = value; }
		}
	}
}
