using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace Gel.Compiler.Ast
{
	class ApplyExpression : Expression
	{
		Expression m_stream;
		Type m_type;
		string m_name;
		Expression m_filter;
		Expression m_closure;

		Block m_block;
		LocalVariable m_var;
		MethodInfo m_getEnumerator;
		Type m_enumeratorType;
		MethodInfo m_moveNext;
		MethodInfo m_getCurrent;

		public ApplyExpression(Expression stream, Type type, string name, Expression filter,
			Expression closure, Location loc)
		{
			m_stream = stream;
			m_type = type;
			m_name = name;
			m_filter = filter;
			m_closure = closure;
			this.Location = loc;
		}

		public override Expression ResolveExpression()
		{
			// Note that this block does not exist in the AST tree
			// It's just a container for the variable
			m_block = new Block(ResolveContext.CurrentBlock);
			m_block.ResolveStatement();
			ResolveContext.CurrentBlock = m_block;

			m_var = ResolveContext.CurrentMethod.CreateLocalVariable(m_name, m_type, this.Location);
			m_block.AddLocalVariable(m_var);

			m_stream = m_stream.ResolveExpression();

			if (m_filter != null)
			{
				m_filter = m_filter.ResolveExpression();
				if (m_filter.ExpressionType != typeof(bool))
					throw new CompileException("Filter is not boolean", this.Location);
			}

			m_closure = m_closure.ResolveExpression();

			Type enumType = typeof(IEnumerable<>).MakeGenericType(m_type);
			Type[] ifaces = m_stream.ExpressionType.GetInterfaces();

			foreach (Type iface in ifaces)
			{
				if (iface == enumType)
				{
					m_getEnumerator = iface.GetMethod("GetEnumerator");
					m_enumeratorType = typeof(IEnumerator<>).MakeGenericType(m_type);
					m_moveNext = m_enumeratorType.GetInterface("IEnumerator").GetMethod("MoveNext");
					m_getCurrent = m_enumeratorType.GetProperty("Current").GetGetMethod();

					break;
				}
			}

			if (m_getEnumerator == null)
			{
				throw new CompileException("Stream does not support this type asdhjhdfjk", this.Location);
			}

			ResolveContext.CurrentBlock = m_block.Parent;

			return this;
		}

		public override void EmitStatement(EmitContext ec)
		{
			// Block's emitstatement creates the local vars.
			m_block.EmitStatement(ec);

			m_stream.EmitExpression(ec);

			LocalBuilder enumLocal = ec.IlGen.DeclareLocal(m_enumeratorType);
			Label endLabel = ec.IlGen.DefineLabel();
			Label loopLabel = ec.IlGen.DefineLabel();

			ec.IlGen.EmitCall(OpCodes.Callvirt, m_getEnumerator, null);

			ec.IlGen.Emit(OpCodes.Stloc, enumLocal);

			// loop:
			ec.IlGen.MarkLabel(loopLabel);

			// MoveNext
			ec.IlGen.Emit(OpCodes.Ldloc, enumLocal);
			ec.IlGen.EmitCall(OpCodes.Callvirt, m_moveNext, null);
			ec.IlGen.Emit(OpCodes.Brfalse_S, endLabel);

			// Current
			ec.IlGen.Emit(OpCodes.Ldloc, enumLocal);
			ec.IlGen.EmitCall(OpCodes.Callvirt, m_getCurrent, null);
			ec.IlGen.Emit(OpCodes.Stloc, m_var.LocalBuilder);

			// filter
			if(m_filter != null)
				m_filter.EmitConditionalStatement(ec, null, loopLabel);

			m_closure.EmitExpression(ec);

			ec.IlGen.Emit(OpCodes.Pop);

			ec.IlGen.Emit(OpCodes.Br_S, loopLabel);

			// end:
			ec.IlGen.MarkLabel(endLabel);
		}

		public override void PrintTree(int indent)
		{
			Output(indent, "Apply({0} {1})", m_type.Name, m_name);
			m_stream.PrintTree(indent + 2);
			if(m_filter != null)
				m_filter.PrintTree(indent + 2);
			m_closure.PrintTree(indent + 2);
		}
	}
}
