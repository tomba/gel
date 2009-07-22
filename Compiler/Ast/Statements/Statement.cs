using System;

namespace Gel.Compiler.Ast
{
	/// <summary>
	/// Summary description for Statement.
	/// </summary>
	abstract class Statement : AstNode
	{
		public Statement()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public virtual Statement ResolveStatement()
		{
			throw new NotImplementedException(String.Format("Resolve not implemented in {0}", this.GetType().Name));
		}

		public virtual void EmitStatement(EmitContext ec)
		{
			throw new NotImplementedException(String.Format("Emit not implemented in {0}", this.GetType().Name));
		}
	}
}
