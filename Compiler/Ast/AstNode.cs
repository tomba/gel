using System;
using System.Collections.Generic;

namespace Gel.Compiler.Ast
{
	abstract class AstNode
	{
		Location m_location = null;
		// dsttype -> srctype[]
		static Dictionary<Type, Type[]> s_numericConversions = new Dictionary<Type, Type[]>();

		static AstNode()
		{
			s_numericConversions[typeof(byte)] = new Type[] { 
				typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
				typeof(float), typeof(double), typeof(decimal) };
			s_numericConversions[typeof(ushort)] = new Type[] { 
				typeof(int), typeof(uint), typeof(long), typeof(ulong),
				typeof(float), typeof(double), typeof(decimal) };
			s_numericConversions[typeof(uint)] = new Type[] { 
				typeof(long), typeof(ulong),
				typeof(float), typeof(double), typeof(decimal) };
			s_numericConversions[typeof(ulong)] = new Type[] { 
				typeof(float), typeof(double), typeof(decimal) };

			s_numericConversions[typeof(sbyte)] = new Type[] { 
				typeof(short), typeof(int), typeof(long),
				typeof(float), typeof(double), typeof(decimal) };
			s_numericConversions[typeof(short)] = new Type[] { 
				typeof(int), typeof(long),
				typeof(float), typeof(double), typeof(decimal) };
			s_numericConversions[typeof(int)] = new Type[] { 
				typeof(long),
				typeof(float), typeof(double), typeof(decimal) };
			s_numericConversions[typeof(long)] = new Type[] { 
				typeof(float), typeof(double), typeof(decimal) };

			s_numericConversions[typeof(char)] = new Type[] { 
				typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
				typeof(float), typeof(double), typeof(decimal) };

			s_numericConversions[typeof(float)] = new Type[] { 
				typeof(double) };
		}

		public AstNode()
		{
		}

		public Location Location
		{
			set { m_location = value; }
			get { return m_location; }
		}

		public abstract void PrintTree(int indent);

		public void Output(int indent, string str, params object[] args)
		{
			Gel.Compiler.Output.WriteLine(indent, str, args);
		}

		#region Type Checking
		// Tries to implicitely typecast exp to destinationType
		// C# Spec chapter 6
		public static Expression ImplicitConversion(Type dstType, Expression exp)
		{
			Type srcType = exp.ExpressionType;

			#region Identity conversion
			//6.1.1 Identity conversion
			//An identity conversion converts from any type to the same type. This conversion exists only such that an entity that already has a required type can be said to be convertible to that type.

			if (srcType == dstType)
				return exp;
			#endregion

			#region Implicit numeric conversions
			//6.1.2 Implicit numeric conversions
			//The implicit numeric conversions are:
			//•	From sbyte to short, int, long, float, double, or decimal.
			//•	From byte to short, ushort, int, uint, long, ulong, float, double, or decimal.
			//•	From short to int, long, float, double, or decimal.
			//•	From ushort to int, uint, long, ulong, float, double, or decimal.
			//•	From int to long, float, double, or decimal.
			//•	From uint to long, ulong, float, double, or decimal.
			//•	From long to float, double, or decimal.
			//•	From ulong to float, double, or decimal.
			//•	From char to ushort, int, uint, long, ulong, float, double, or decimal.
			//•	From float to double.
			//Conversions from int, uint, long, or ulong to float and from long or ulong to double may cause a loss of precision, but will never cause a loss of magnitude. The other implicit numeric conversions never lose any information.
			//There are no implicit conversions to the char type, so values of the other integral types do not automatically convert to the char type.

			if (s_numericConversions.ContainsKey(srcType))
			{
				if (Array.IndexOf<Type>(s_numericConversions[srcType], dstType) != -1)
				{
					return new TypeConversionExpression(dstType, exp, exp.Location);
				}
			}
			#endregion

			#region Implicit enumeration conversions
			//6.1.3 Implicit enumeration conversions
			//An implicit enumeration conversion permits the decimal-integer-literal 0 to be converted to any enum-type.
			#endregion
			// TODO

			#region Implicit reference conversions
			//6.1.4 Implicit reference conversions
			//The implicit reference conversions are:
			//•	From any reference-type to object.
			//•	From any class-type S to any class-type T, provided S is derived from T.
			//•	From any class-type S to any interface-type T, provided S implements T.
			//•	From any interface-type S to any interface-type T, provided S is derived from T.
			//•	From an array-type S with an element type SE to an array-type T with an element type TE, provided all of the following are true:
			//o	S and T differ only in element type. In other words, S and T have the same number of dimensions.
			//o	Both SE and TE are reference-types.
			//o	An implicit reference conversion exists from SE to TE.
			//•	From any array-type to System.Array.
			//•	From any delegate-type to System.Delegate.
			//•	From the null type to any reference-type.
			//The implicit reference conversions are those conversions between reference-types that can be proven to always succeed, and therefore require no checks at run-time.
			//Reference conversions, implicit or explicit, never change the referential identity of the object being converted. In other words, while a reference conversion may change the type of the reference, it never changes the type or value of the object being referred to.

			//•	From any reference-type to object.
			if (dstType == typeof(object) && !srcType.IsValueType)
			{
				return exp;
			}

			//•	From the null type to any reference-type.
			if (!dstType.IsValueType && srcType == typeof(NullType))
			{
				return exp;
			}

			// TODO the rest
			#endregion

			#region Boxing conversions
			//6.1.5 Boxing conversions
			//A boxing conversion permits any value-type to be implicitly converted to type object or System.ValueType or to any interface-type implemented by the value-type. Boxing a value of a value-type consists of allocating an object instance and copying the value-type value into that instance. A struct can be boxed to the type System.ValueType, since that is a base class for all structs (§11.3.2).
			//Boxing conversions are described further in §4.3.1.
			if (dstType == typeof(object) && srcType.IsValueType)
			{
				return new BoxExpression(exp, exp.Location);
			}
			#endregion

			#region Implicit constant expression conversions
			//6.1.6 Implicit constant expression conversions
			//An implicit constant expression conversion permits the following conversions:
			//•	A constant-expression (§7.15) of type int can be converted to type sbyte, byte, short, ushort, uint, or ulong, provided the value of the constant-expression is within the range of the destination type.
			//•	A constant-expression of type long can be converted to type ulong, provided the value of the constant-expression is not negative.
			if (exp is IntegerLiteralExpression)
			{
				int num = ((IntegerLiteralExpression)exp).Number;
				bool failed = false;

				if (dstType == typeof(sbyte) && num <= sbyte.MaxValue && num >= sbyte.MinValue)
					exp.ExpressionType = typeof(sbyte);
				else if (dstType == typeof(byte) && num <= byte.MaxValue && num >= byte.MinValue)
					exp.ExpressionType = typeof(byte);
				else if (dstType == typeof(ushort) && num <= ushort.MaxValue && num >= ushort.MinValue)
					exp.ExpressionType = typeof(ushort);
				else if (dstType == typeof(short) && num <= short.MaxValue && num >= short.MinValue)
					exp.ExpressionType = typeof(short);
					/*
				else if (dstType == typeof(uint) && num <= uint.MaxValue && num >= uint.MinValue)
					exp.ExpressionType = typeof(uint);
				else if (dstType == typeof(ulong) && num <= ulong.MaxValue && num >= ulong.MinValue)
					exp.ExpressionType = typeof(ulong);
					 */
				else
					failed = true;

				// TODO the rest

				if(failed == false)
					return exp;
			}
			#endregion

			#region Implicit Gel Conversions
			// Objects will be implicitely typecasted/unboxed to target type
			
			if(srcType == typeof(object))
			{
				return new UnboxExpression(dstType, exp, exp.Location);
			}
			#endregion

			// fail
			return null;
		}

		public static bool ImplicitConversionPossible(Type destinationType, Expression exp)
		{
			// TODO: currently does unnecessary objects
			// also not very good method name 

			Expression newExp = ImplicitConversion(destinationType, exp);

			if (newExp == null)
				return false;
			else
				return true;
		}

		public static bool TypeCheckImplicit(Type destinationType, ref Expression exp)
		{
			Expression newExp = ImplicitConversion(destinationType, exp);

			if (newExp == null)
			{
				return false;
			}
			else
			{
				exp = newExp;
				return true;
			}
		}

		// Tries to make Types of exp1 and exp2 compatible
		public static bool TypeCheck(ref Expression exp1, ref Expression exp2)
		{
			if (exp1.ExpressionType == exp2.ExpressionType)
				return true;

			if (exp1.ExpressionType == typeof(object) && exp2.ExpressionType != typeof(object))
			{
				exp1 = ImplicitConversion(exp2.ExpressionType, exp1);
				return true;
			}

			if (exp2.ExpressionType == typeof(object) && exp1.ExpressionType != typeof(object))
			{
				exp2 = ImplicitConversion(exp1.ExpressionType, exp2);
				return true;
			}

			Expression e;

			e = ImplicitConversion(exp1.ExpressionType, exp2);
			if (e != null)
			{
				exp2 = e;
				return true;
			}

			e = ImplicitConversion(exp2.ExpressionType, exp1);
			if (e != null)
			{
				exp1 = e;
				return true;
			}

			return false;
		}
		#endregion

	}
}
