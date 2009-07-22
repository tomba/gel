using System;

namespace Gel.Compiler.Graph.Visitor
{
	/// <summary>
	/// Summary description for IAstVisitor.
	/// </summary>
	abstract class IAstVisitor
	{
		public object DoVisit(object ob)
		{
			Type[] types = new Type[] { ob.GetType() };

			System.Reflection.MethodInfo mi = this.GetType().GetMethod("Visit", types);

			if (mi == null)
			{
				throw new Exception("Visit(" + types[0].Name + ") not implemented in " + this.GetType().Name);
			}

//			try
			{
				return mi.Invoke(this, new object[] { ob });
			}
/*			catch(System.Reflection.TargetInvocationException e)
			{
				Debug.WriteLine("ERROR {0}", e.InnerException.ToString());

				throw e.InnerException;
			}
			*/
		}
	}
}
