// C:\Program Files\Microsoft Visual Studio 8\VC#\Specifications\1033\C# Language Specification 1.2.doc

header
{
// THIS FILE WAS GENERATED BY ANTLR
// -- DO NOT EDIT THIS FILE --

#pragma warning disable 618,219,162

using System.Text;
using System.Collections.Generic;

using BatMud.BatScript.Compiler.Ast;
using BatMud.BatScript.Core;
}

options
{
	language = "CSharp";
	namespace = "BatMud.BatScript.Compiler.BatParser";
}

// Parser grammar

class BatParser extends Parser;

options
{
	k = 2;							// lookahead
	defaultErrorHandler = false;
	ASTLabelType = "BatMud.BatScript.Compiler.BatParser.BatAst";
	buildAST = true;
	classHeaderPrefix = "";
}

{
	Program m_currentProgram = null;
	MethodDeclaration m_currentMethod = null;
	Block m_currentBlock = null;
}
 
// Entrypoints

program returns [ Program prog = null ]
	{
		m_currentProgram = new Program("testprog");
		prog = m_currentProgram;
	}
	:	(usingDeclarations)*
		(classMemberDeclarations)*
	;
	
usingDeclarations
	{
		string ns = null;
	}
	:	USING ns=namespaceName SEMI
		{
			ResolveContext.AddUsingNamespace(ns);
		}
	;

functionUnit returns [ MethodDeclaration m = null ]
	:	m = methodDeclaration
		{
			ReturnStatement retStm;

			if (m.ReturnType.IsPrimitive)
			{
				retStm = new ReturnStatement(new IntegerLiteralExpression(0, null), null);
			}
			else if (m.ReturnType.IsValueType)
			{
				throw new NotImplementedException();
			}
			else
			{
				retStm = new ReturnStatement(new NullLiteralExpression(null), null);
			}
			
			m.Block.StatementList.Add(retStm);	
		}
	;
	
blockUnit [ string funcName, Type returnType, Type[] parameterTypes, string[] parameterNames ] 
	returns [ MethodDeclaration m = null ]
	{
		m_currentMethod = new MethodDeclaration(funcName, Modifiers.Public, returnType, null);
		Block b = new Block(m_currentBlock, null);
		m_currentBlock = b;
		
		for(int i = 0; i < parameterTypes.Length; i++)
		{
			LocalVariable local = m_currentMethod.CreateParameter(parameterNames[i], parameterTypes[i], null);
			m_currentBlock.AddLocalVariable(local);
		}
		
		StatementList stmList;
	}
	:	stmList=statementList
		{ 
			ReturnStatement retStm;

			if (returnType.IsPrimitive)
			{
				retStm = new ReturnStatement(new IntegerLiteralExpression(0, null), null);
			}
			else if (returnType.IsValueType)
			{
				throw new NotImplementedException();
			}
			else
			{
				retStm = new ReturnStatement(new NullLiteralExpression(null), null);
			}
			
			stmList.Add(retStm);	
		
			m_currentBlock.StatementList = stmList;
			m_currentBlock = m_currentBlock.Parent;
			m_currentMethod.Block = b;
			m = m_currentMethod;
		}
	;

expressionUnit [ Type returnType, Type[] parameterTypes ] returns [ MethodDeclaration m = null ]
	{
		// TODO: add method params
		m_currentMethod = new MethodDeclaration("_funcName", Modifiers.Public, returnType, null);
		Block b = new Block(m_currentBlock, null);
		m_currentBlock = b;
		StatementList stmList = new StatementList();
		m_currentBlock.StatementList = stmList;
		Expression e = null;
	}
	:	(e=expression)? (SEMI)?
		{
			stmList.Add(new ReturnStatement(e, null));
			m_currentBlock.StatementList = stmList;
			m_currentBlock = m_currentBlock.Parent;
			m_currentMethod.Block = b;
			m = m_currentMethod;
		}
	;

// B.2.1 Basic concepts

namespaceName returns [ string n = null ]
	:	n=namespaceOrTypename
	;
	
typeName returns [ string n = null ]
	:	n=namespaceOrTypename
	;
	
namespaceOrTypename returns [ string n = null ]
	{
		StringBuilder sb = new StringBuilder();
		List<Type> typeList = null;
	}
	:	i1:IDENTIFIER
		{
			sb.Append(#i1.getText()); 
		} 
		(
			DOT i2:IDENTIFIER
			{
				sb.Append("."); 
				sb.Append(#i2.getText()); 
			}
		)*
		((typeArgumentList) => typeList=typeArgumentList)?
		{
			if(sb.ToString() == "list")
			{
				sb = new StringBuilder("System.Collections.Generic.List");
				
				if(typeList == null)
				{
					sb.Append("`1[System.Object]");
				}
			}
			
			if(sb.ToString() == "mapping")
			{
				sb = new StringBuilder("System.Collections.Generic.Dictionary");
				
				if(typeList == null)
				{
					sb.Append("`2[System.Object,System.Object]");
				}
			}
		
			if(typeList != null)
			{
				sb.Append("`");
				sb.Append(typeList.Count);
				sb.Append("[");
				for(int i = 0; i < typeList.Count; i++)
				{
					sb.Append(typeList[i].FullName);
					if(i < typeList.Count - 1)
						sb.Append(",");
				}
				sb.Append("]");
			}
		
			n = sb.ToString();
		}
	;
	
// B.2.2 Types

typeArgumentList returns [ List<Type> l ]
	:	LTHAN l=typeArguments GTHAN
	;

typeArguments returns [ List<Type> l = new List<Type>() ]
	{
		Type t = null;
	}
	:	t=typeArgument { l.Add(t); } (COMMA t=typeArgument { l.Add(t); } )*
	;
	
typeArgument returns [ Type t = null ]
	:	t=type
	;

type returns [ Type ret = null ]
	{
		string n;
	}
	:	ret=simpleType
	|	(arrayType)=> ret=arrayType
	|	n=typeName
		{
			ret = ResolveContext.FindType(n); 
			if(ret == null)
				throw new CompileException("Type not found: " + n,  new Location());
		}
	|	MIXED		{ ret = Type.GetType("System.Object"); }
	|	STRING		{ ret = Type.GetType("System.String"); }
	|	BATPROGRAM	{ ret = typeof(BatMud.BatScript.Core.BatProgram); }
	|	BATOBJECT	{ ret = typeof(BatMud.BatScript.Core.BatObject); }
	;
	
simpleType returns [ Type ret = null ]
	:	ret=numericType
	|	BOOL		{ ret = Type.GetType("System.Boolean"); }
	;
	
numericType returns [ Type ret = null ]
	:	ret=integralType
	|	ret=floatingpointType
	;
	
classType returns [ Type ret = null ]
	{
		string n;
	}
	:	n=typeName
		{
			ret = ResolveContext.FindType(n); 
			if(ret == null)
				throw new CompileException("Type not found",  new Location());
		}
	|	MIXED		{ ret = Type.GetType("System.Object"); }
	|	STRING		{ ret = Type.GetType("System.String"); }
	|	BATPROGRAM	{ ret = typeof(BatMud.BatScript.Core.BatProgram); }
	|	BATOBJECT	{ ret = typeof(BatMud.BatScript.Core.BatObject); }
	;
	
integralType returns [ Type ret = null ]
	:	UINT8		{ ret = Type.GetType("System.UInt8"); }
	|	UINT16		{ ret = Type.GetType("System.UInt16"); }
	|	UINT32		{ ret = Type.GetType("System.UInt32"); }
	|	UINT64		{ ret = Type.GetType("System.UInt64"); }
	|	INT8		{ ret = Type.GetType("System.Int8"); }
	|	INT16		{ ret = Type.GetType("System.Int16"); }
	|	INT32		{ ret = Type.GetType("System.Int32"); }
	|	INT64		{ ret = Type.GetType("System.Int64"); }
	|	CHAR		{ ret = Type.GetType("System.Char"); }
	;
	
floatingpointType returns [ Type ret = null ]
	:	FLOAT32		{ ret = Type.GetType("System.Single"); }
	|	FLOAT64		{ ret = Type.GetType("System.Double"); }
	;

arrayType returns [ Type ret = null ]
	{
		int rank;
	}
	:	ret=nonArrayType rank=rankSpecifiers
		{
			string typeName = ret.FullName;
			for (int i = 0; i < rank; i++)
				typeName += "[]";
				
			ret = Type.GetType(typeName);
		}
	;

nonArrayType returns [ Type ret = null ]
	:	ret=classType
	|	ret=simpleType
	;

rankSpecifiers returns [ int rank = 0 ]
	:	( options { greedy=true; }: 
			LBRACK RBRACK
			{
				rank++;
			}
		)+
	;

genericType returns [ Type ret = null ]
	:	
	;

identifier [ out Location loc ]
	returns [ string ret = "unimplemented" ]
	{ loc = null; }
	:	i:IDENTIFIER
		{
			ret = #i.getText();
			loc = #i.GetLocation();
		}
	;
	
// B.2.4 Expressions

argumentList returns [ List<Expression> argList = new List<Expression>() ]
	{
		Expression e;
	}
	:	e=argument { argList.Add(e); } (COMMA! e=argument { argList.Add(e); } )*
	;
	
argument returns [ Expression exp = null ]
	:	exp=expression
	;
	
primaryStart returns [ Expression exp = null ]
	:	exp=literal
	|	exp=simpleName
	|	exp=dynamicName
	|	exp=parenthesizedExpression
//	|	memberAccess
	|	(arrayCreationExpression)=> exp=arrayCreationExpression
	|	exp=objectCreationExpression
	|	exp=typeofExpression
	|	predefinedTypeAccess	// TODO
	|	exp=listCreationExpression
	|	exp=mappingCreationExpression
	;

listCreationExpression returns [ Expression exp = null ]
	{
		List<Expression> expList = new List<Expression>();
	}
	:	root:LPAREN LBRACE (exp=expression { expList.Add(exp); } (COMMA exp=expression { expList.Add(exp); } )*)?  RBRACE RPAREN
		{
			exp = new ListCreationExpression(expList, #root.GetLocation());
		}
	;

mappingCreationExpression returns [ Expression exp = null ]
	{
		List<Expression> keyList = new List<Expression>();
		List<Expression> valueList = new List<Expression>();
		Expression keyExpr = null;
		Expression valueExpr = null;
	}
	:	root:LPAREN LBRACK 
		(
			keyExpr=expression COLON valueExpr=expression { keyList.Add(keyExpr); valueList.Add(valueExpr); } 
			(
				COMMA keyExpr=expression COLON valueExpr=expression { keyList.Add(keyExpr); valueList.Add(valueExpr); } 
			)*
		)?
		RBRACK RPAREN
		{
			exp = new MappingCreationExpression(keyList, valueList, #root.GetLocation());
		}
	;

primaryExpression returns [ Expression exp = null ]
	{
		string member = null;
		List<Expression> argList = null;
	}
	// TODO: this doesn't really work at all =)
	:	exp=primaryStart 
		(	options { greedy=true; }:
			exp=postfixExpression[exp]
		|	argList=elementAccess
			{
				exp = new ElementAccessExpression(exp, argList, exp.Location);
			}
		|	(invocationExpression)=> argList=invocationExpression
			{
				exp = new InvocationExpression(exp, argList, exp.Location);
			}
		|	member=memberAccess
			{
				exp = new MemberAccessExpression(exp, member, exp.Location);
			}
		)*
	;
	
dynamicName returns [ DynamicName exp = null ]
	:	DOLLAR i:IDENTIFIER
		{
			exp = new DynamicName(#i.getText(), #i.GetLocation());
		}
	;

simpleName returns [ SimpleName exp = null ]
	:	i:IDENTIFIER
		{
			exp = new SimpleName(#i.getText(), #i.GetLocation());
		}
	;
	
parenthesizedExpression returns [ Expression exp = null ]
	:	LPAREN! exp=expression RPAREN!
	;

memberAccess returns [ string member = null ]
	:	DOT! i:IDENTIFIER
		{
			member = #i.getText();
		}
	;
	
predefinedType	// TODO
	:	BOOL
	|	UINT8
	|	UINT16
	|	UINT32
	|	UINT64
	|	INT8
	|	INT16
	|	INT32
	|	INT64
	|	CHAR
	|	FLOAT32
	|	FLOAT64
	|	MIXED
	|	STRING
	;

predefinedTypeAccess
	:	predefinedType DOT! IDENTIFIER
	;
	
invocationExpression returns [ List<Expression> argList = null ]
	:	LPAREN! ( argList=argumentList )? RPAREN!
		{
			if(argList == null)
				argList = new List<Expression>();
		}
	;
	
elementAccess returns [ List<Expression> expList = null ]
	:	LBRACK expList=expressionList RBRACK
	;
	
expressionList returns [ List<Expression> expList = new List<Expression>() ]
	{
		Expression exp = null;
	}
	:	exp=expression { expList.Add(exp); } (COMMA! exp=expression { expList.Add(exp); } )*
	;
	
postfixExpression [ Expression exp ] returns [ Expression postExp = null ]
	:	r1:INC
		{
			postExp = new PostfixExpression(PostfixExpression.Operator.Inc, exp, #r1.GetLocation());
		}
	|	r2:DEC
		{
			postExp = new PostfixExpression(PostfixExpression.Operator.Dec, exp, #r2.GetLocation());
		}
	;
	
objectCreationExpression returns [ Expression exp = null ]
	{
		Type t = null;
		List<Expression> argList = new List<Expression>();
	}
	:	root:NEW^ t=type LPAREN! ( argList=argumentList )? RPAREN!
		{
			exp = new NewExpression(t, argList, #root.GetLocation());
		}
	;
	
arrayCreationExpression returns [ Expression exp = null ]
	{
		Type t = null;
	}
	:	(NEW t=arrayType arrayInitializer[t])=> NEW t=arrayType exp=arrayInitializer[t]
	|	root:NEW t=nonArrayType LBRACK exp=expression RBRACK //XXX: (options {greedy=true;}: rankSpecifiers[ret])? (arrayInitializer)?
		{
			t = Type.GetType(t.FullName + "[]");
			exp = new ArrayCreationExpression(t, exp, null, #root.GetLocation());
		}
	;
	
typeofExpression returns [ Expression exp = null; ]
	{
		Type t = null;
	}
	:	root:TYPEOF LPAREN t=typeofType RPAREN
		{
			exp = new TypeOfExpression(t, #root.GetLocation());
		}
	;
	
typeofType returns [ Type t = null ]
	:	t=type
	|	VOID { return typeof(void); }
	;
	
unaryExpression returns [ Expression exp = null ]
	{
		Type t = null;
		UnaryExpression.Operator unaryOper;
		Location loc;
	}
	:	( castExpression unaryExpression )=> t=castExpression exp=unaryExpression
		{
			exp = new TypeCastExpression(t, exp, null);
		}
	|	exp=primaryExpression
	|	unaryOper=unaryOp[out loc] exp=unaryExpression
		{
			exp = new UnaryExpression(unaryOper, exp, loc);
		}
	|	exp=prefixExpression
	;

unaryOp [ out Location loc ]
	returns [ UnaryExpression.Operator oper = UnaryExpression.Operator.None ]
	{
		loc = null;
	}
	:	o1:PLUS		{ loc = #o1.GetLocation(); oper = UnaryExpression.Operator.Plus; }
	|	o2:MINUS	{ loc = #o2.GetLocation(); oper = UnaryExpression.Operator.Minus; }
	|	o3:LNOT		{ loc = #o3.GetLocation(); oper = UnaryExpression.Operator.LNot; }
	|	o4:BNOT		{ loc = #o4.GetLocation(); oper = UnaryExpression.Operator.BNot; }
	;

prefixExpression returns [ Expression exp = null ]
	{
		Location l = null;
	}
	:	r1:INC exp=unaryExpression 
		{
			l = #r1.GetLocation();
			exp = new AssignmentExpression(exp, new ArithmeticExpression(ArithmeticExpression.Operator.Plus, exp, new IntegerLiteralExpression(1, l), l), l); 
		}
	|	r2:DEC exp=unaryExpression
		{
			l = #r2.GetLocation();
			exp = new AssignmentExpression(exp, new ArithmeticExpression(ArithmeticExpression.Operator.Minus, exp, new IntegerLiteralExpression(1, l), l), l); 
		}
	;

castExpression returns [ Type t = null ]
	:	LPAREN! t=type RPAREN!
	;

multiplicativeExpression returns [ Expression exp = null ]
	{
		ArithmeticExpression.Operator oper;
		Expression e1, e2;
		Location loc;
	}
	:	e1=unaryExpression 
		( 
			oper=multiplicativeOp[out loc] e2=unaryExpression 
			{
				e1 = new ArithmeticExpression(oper, e1, e2, loc);
			}
		)*
		{
			exp = e1;
		}
	;
	
multiplicativeOp [ out Location loc ]
	returns [ ArithmeticExpression.Operator oper = ArithmeticExpression.Operator.None ]
	{ loc = null; }
	:	r1:STAR		{ loc = #r1.GetLocation(); oper = ArithmeticExpression.Operator.Mul; }
	|	r2:DIV		{ loc = #r2.GetLocation(); oper = ArithmeticExpression.Operator.Div; }
	|	r3:MOD		{ loc = #r3.GetLocation(); oper = ArithmeticExpression.Operator.Mod; }
	;
	
additiveExpression returns [ Expression exp = null ]
	{
		ArithmeticExpression.Operator oper;
		Expression e1, e2;
		Location loc;
	}
	:	e1=multiplicativeExpression 
		(	
			oper=additiveOp[out loc] e2=multiplicativeExpression 
			{
				e1 = new ArithmeticExpression(oper, e1, e2, loc); 
			} 
		)*
		{
			exp = e1;
		}
	;
	
additiveOp [ out Location loc ]
	returns [ ArithmeticExpression.Operator oper = ArithmeticExpression.Operator.None ]
	{ loc = null; }
	:	r1:PLUS		{ loc = #r1.GetLocation(); oper = ArithmeticExpression.Operator.Plus; }
	|	r2:MINUS	{ loc = #r2.GetLocation(); oper = ArithmeticExpression.Operator.Minus; }
	;
	
shiftExpression returns [ Expression exp = null ]
	{
		ArithmeticExpression.Operator oper;
		Expression e1, e2;
		Location loc;
	}
	:	e1=additiveExpression 
		(
			oper=shiftOp[out loc] e2=additiveExpression
			{
				e1 = new ArithmeticExpression(oper, e1, e2, loc);
			}
		)*
		{
			exp = e1;
		}
	;
	
shiftOp [ out Location loc ] 
	returns [ ArithmeticExpression.Operator oper = ArithmeticExpression.Operator.None ]
	{ loc = null; }
	:	r1:SHL		{ loc = #r1.GetLocation(); oper = ArithmeticExpression.Operator.Shl; }
	|	r2:SHR		{ loc = #r2.GetLocation(); oper = ArithmeticExpression.Operator.Shr; }
	;
	
relationalExpression returns [ Expression exp = null ]
	{
		ComparisonExpression.Operator oper;
		TypeComparisonExpression.Operator oper2;
		Expression e1, e2;
		Location loc;
		Type t;
	}
	:	e1=shiftExpression
		( 
			oper=relationalOp[out loc] e2=shiftExpression
			{
				e1 = new ComparisonExpression(oper, e1, e2, loc);
			}
		| oper2=typeCompOp[out loc] t=type
			{
				e1 = new TypeComparisonExpression(oper2, e1, t, loc);
			}
		)*
		{
			exp = e1;
		}
	;
	
relationalOp [ out Location loc ]
	returns [ ComparisonExpression.Operator oper = ComparisonExpression.Operator.None ]
	{ loc = null; }
	:	r1:LTHAN	{ loc = #r1.GetLocation(); oper = ComparisonExpression.Operator.LT; } 
	|	r2:GTHAN	{ loc = #r2.GetLocation(); oper = ComparisonExpression.Operator.GT; }	
	|	r3:LE		{ loc = #r3.GetLocation(); oper = ComparisonExpression.Operator.LE; }
	|	r4:GE		{ loc = #r4.GetLocation(); oper = ComparisonExpression.Operator.GE; }
	;
	
typeCompOp [ out Location loc ]
	returns [ TypeComparisonExpression.Operator oper = TypeComparisonExpression.Operator.None ]
	{ loc = null; }
	:	r1:IS		{ loc = #r1.GetLocation(); oper = TypeComparisonExpression.Operator.IS; } 
	|	r2:AS		{ loc = #r2.GetLocation(); oper = TypeComparisonExpression.Operator.AS; }
	;
	
equalityExpression returns [ Expression exp = null ]
	{
		ComparisonExpression.Operator oper;
		Expression e1, e2;
		Location loc;
	}
	:	e1=relationalExpression 
		( 
			oper=equalityOp[out loc] e2=relationalExpression 
			{
				e1 = new ComparisonExpression(oper, e1, e2, new Location());
			}
		)*
		{
			exp = e1;
		}
	;

equalityOp [ out Location loc ]
	returns [ ComparisonExpression.Operator oper = ComparisonExpression.Operator.None ]
	{ loc = null; }
	:	r1:EQ	{ loc = #r1.GetLocation(); oper = ComparisonExpression.Operator.EQ; }
	|	r2:NE	{ loc = #r2.GetLocation(); oper = ComparisonExpression.Operator.NE; }
	;
	
andExpression returns [ Expression exp = null ]
	{
		Expression e1, e2;
	}		
	:	e1=equalityExpression
		( 
			root:BAND e2=equalityExpression 
			{
				e1 = new ArithmeticExpression(ArithmeticExpression.Operator.BAnd, e1, e2, #root.GetLocation());
			}
		)*
		{
			exp = e1;
		}
	;
	
exclusiveOrExpression returns [ Expression exp = null ]
	{
		Expression e1, e2;
	}
	:	e1=andExpression 
		( 
			root:BXOR e2=andExpression 
			{
				e1 = new ArithmeticExpression(ArithmeticExpression.Operator.BXor, e1, e2, #root.GetLocation());
			}
		)*
		{
			exp = e1;
		}
	;
	
inclusiveOrExpression returns [ Expression exp = null ]
	{
		Expression e1, e2;
	}
	:	e1=exclusiveOrExpression 
		( 
			root:BOR e2=exclusiveOrExpression
			{
				e1 = new ArithmeticExpression(ArithmeticExpression.Operator.BOr, e1, e2, #root.GetLocation());
			}
		)*
		{
			exp = e1;
		}
	;
	
conditionalAndExpression returns [ Expression exp = null ]
	{
		Expression e1, e2;
	}
	:	e1=inclusiveOrExpression
		(
			root:LAND e2=inclusiveOrExpression
			{
				e1 = new LogicalExpression(LogicalExpression.Operator.And, e1, e2, #root.GetLocation());
			}
		)*
		{
			exp = e1;
		}
	;
	
conditionalOrExpression returns [ Expression exp = null ]
	{
		Expression e1, e2;
	}
	:	e1=conditionalAndExpression
		(
			root:LOR e2=conditionalAndExpression
			{
				e1 = new LogicalExpression(LogicalExpression.Operator.Or, e1, e2, #root.GetLocation());
			}
		)*
		{
			exp = e1;
		}
	;
	
conditionalExpression returns [ Expression exp = null ]
	{
		Expression e1, e2;
	}
	:	exp=conditionalOrExpression 
		( 
			root:QUESTION e1=expression COLON! e2=expression 
			{
				exp = new ConditionalExpression(exp, e1, e2, #root.GetLocation());
			}
		)?
	;

assignment returns [ Expression exp = null ]
	{
		Expression e1, e2;
		ArithmeticExpression.Operator op;
		Location loc;
	}
	:	e1=unaryExpression op=assignmentOp[out loc] e2=expression
		{
			if(op == ArithmeticExpression.Operator.None)
			{
				exp = new AssignmentExpression(e1, e2, loc);
			}
			else
			{
				exp = new AssignmentExpression(e1, 
											new ArithmeticExpression(op, e1, e2, loc),
											loc);
			}
		}
	;
	
assignmentOp [ out Location loc ]
	returns [ ArithmeticExpression.Operator oper = ArithmeticExpression.Operator.None ]
	{ loc = null; }
	:	r1:ASSIGN		{ loc = #r1.GetLocation(); oper = ArithmeticExpression.Operator.None; }
	|	r2:PLUS_ASN		{ loc = #r2.GetLocation(); oper = ArithmeticExpression.Operator.Plus; }
	|	r3:MINUS_ASN	{ loc = #r3.GetLocation(); oper = ArithmeticExpression.Operator.Minus; }
	|	r4:STAR_ASN		{ loc = #r4.GetLocation(); oper = ArithmeticExpression.Operator.Mul; }
	|	r5:DIV_ASN		{ loc = #r5.GetLocation(); oper = ArithmeticExpression.Operator.Div; }
	|	r6:MOD_ASN		{ loc = #r6.GetLocation(); oper = ArithmeticExpression.Operator.Mod; }
	|	r7:BAND_ASN		{ loc = #r7.GetLocation(); oper = ArithmeticExpression.Operator.BAnd; }
	|	r8:BOR_ASN		{ loc = #r8.GetLocation(); oper = ArithmeticExpression.Operator.BOr; }
	|	r9:BXOR_ASN		{ loc = #r9.GetLocation(); oper = ArithmeticExpression.Operator.BXor; }
	|	r10:SHL_ASN		{ loc = #r10.GetLocation(); oper = ArithmeticExpression.Operator.Shl; }
	|	r11:SHR_ASN		{ loc = #r11.GetLocation(); oper = ArithmeticExpression.Operator.Shr; }
	;
	
expression returns [ Expression exp = null ]
	:	(conditionalExpression)=> exp=conditionalExpression
	|	exp=assignment
	;

// B.2.5 Statements

statement returns [ Statement stm = null ]
	:	(declarationStatement)=> stm=declarationStatement 
	|	stm=embeddedStatement
	;
	
embeddedStatement returns [ Statement stm = null ]
	:	stm = block
	|	emptyStatement
	|	stm = expressionStatement
	|	stm = selectionStatement
	|	stm = iterationStatement
	|	stm = jumpStatement
	|	tryStatement
	;
	
block returns [ Block b = null ]
	{
		StatementList stmList;
	}
	:	root:LBRACE! 
		{
			b = new Block(m_currentBlock, #root.GetLocation()); 
			m_currentBlock = b;
		}
		stmList=statementList RBRACE! 
		{ 
			m_currentBlock.StatementList = stmList;
			m_currentBlock = m_currentBlock.Parent;
		}
	;
	
statementList returns [ StatementList stmList = new StatementList(); ]
	{
		Statement stm = null;
	}
	:	(stm=statement { stmList.Add(stm); })*
	;
	
emptyStatement
	:	SEMI!
	;
	
declarationStatement returns [ Statement stm = null ]
	:	stm=localVariableDeclaration SEMI
	|	localConstantDeclaration SEMI { stm = new NopStatement(); }
	;
	
localVariableDeclaration returns [ StatementList stmList = null ]
	{
		Type t;
		stmList = new StatementList();
		Statement stm = null;
	}
	:	t=type stm=localVariableDeclarator[t] { if(stm != null) stmList.Add(stm); }
		( COMMA! stm=localVariableDeclarator[t] { if(stm != null) stmList.Add(stm); } )*
	;
	
localVariableDeclarator [ Type t ] returns [ Statement stm = null ]
	{
		string n;
		Location loc;
		Expression e = null;
	}
	:	n=identifier[out loc] 
		(
			ASSIGN! e=localVariableInitializer[t] 
			{
				stm = new ExpressionStatement(new AssignmentExpression(new SimpleName(n, loc), e, loc), loc);
			}
		)?
		{
			LocalVariable local = m_currentMethod.CreateLocalVariable(n, t, loc);
			m_currentBlock.AddLocalVariable(local);
		}
	;

localVariableInitializer [ Type t ] returns [ Expression exp = null ]
	:	exp = expression
	|	exp = arrayInitializer[t]
	;
	
localConstantDeclaration
	{
		Type t = null;
	}
	:	CONST t=type constantDeclarators[t]
	;
	
constantDeclarators [ Type t ]
	:	constantDeclarator[t] (COMMA constantDeclarator[t])*
	;
	
constantDeclarator [ Type t ]
	{
		Expression exp = null;
	}
	:	i:IDENTIFIER ASSIGN exp=expression
		{
			LocalConst var = new LocalConst(#i.getText(), t, exp, #i.GetLocation());
			m_currentBlock.AddLocalConst(var);
		}
	;
	
expressionStatement returns [ ExpressionStatement stm = null ]
	{
		Expression exp = null;
	}
	:	exp=statementExpression SEMI! { stm = new ExpressionStatement(exp, new Location()); }
	;
	
statementExpression returns [ Expression exp = null ]
	:	(assignment) => exp=assignment
	|	exp=prefixExpression
	|	exp=primaryExpression			//TODO: not all primaryExpression are accepted
	;
	
selectionStatement returns [ Statement stm = null ]
	:	stm = ifStatement
	|	switchStatement
	;
	
ifStatement returns [ IfStatement stm = null ]
	{
		Expression condExp = null;
		Statement trueStm = null;
		Statement falseStm = null;
	}
	:	root:IF LPAREN condExp=expression RPAREN trueStm=embeddedStatement 
		( options { warnWhenFollowAmbig = false; }:			// don't warn about dangling else
			ELSE falseStm=embeddedStatement
		)?
		{
			stm = new IfStatement(condExp, trueStm, falseStm, #root.GetLocation());
		}
	;
	
switchStatement
	:	SWITCH^ LPAREN! expression RPAREN! switchBlock
	;
	
switchBlock
	:	LBRACE! (switchSection)* RBRACE!
	;
	
switchSection
	:	switchLabels statementList
	;
	
switchLabels
	:	CASE! expression COLON!
	|	DEFAULT COLON!
	;
	
iterationStatement returns [ Statement stm = null ]
	:	stm=whileStatement
	|	doStatement
	|	stm=forStatement
	|	foreachStatement
	;
	
whileStatement returns [ WhileStatement ret = null ]
	{
		Expression exp = null;
		Statement stm = null;
	}
	:	root:WHILE^ LPAREN! exp=expression RPAREN! stm=embeddedStatement
		{
			ret = new WhileStatement(exp, stm, #root.GetLocation());
		}
	;
	
doStatement
	:	DO^ embeddedStatement WHILE! LPAREN! expression RPAREN! SEMI!
	;
	
forStatement returns [ ForStatement ret = null ]
	{
		StatementList initializerList = null;
		Expression condition = null;
		StatementList loopStatementList = null;
		Statement body = null;
	}
	:	root:FOR LPAREN (initializerList=forInitializer)? SEMI (condition=expression)? SEMI 
		(loopStatementList=statementExpressionList)? RPAREN! body=embeddedStatement
		{
			ret = new ForStatement(initializerList, condition, loopStatementList, body, #root.GetLocation());
		}
	;
	
forInitializer returns [ StatementList stmList = null ]
	:	(localVariableDeclaration)=> stmList=localVariableDeclaration
	|	stmList=statementExpressionList
	;
	
statementExpressionList returns [ StatementList stmList = new StatementList() ]
	{
		Expression exp = null;
	}
	:	exp=statementExpression { stmList.Add(new ExpressionStatement(exp, exp.Location)); } 
		(COMMA exp=statementExpression { stmList.Add(new ExpressionStatement(exp, exp.Location)); } )*
	;
	
foreachStatement
	{ 
		Location loc;
	}
	:	FOREACH^ LPAREN! type identifier[out loc] IN! expression RPAREN! embeddedStatement
	;
	
jumpStatement returns [ Statement ret = null ]
	:	breakStatement
	|	continueStatement
	|	ret = returnStatement
	|	throwStatement
	;
	
breakStatement
	:	BREAK SEMI!
	;
	
continueStatement
	:	CONTINUE SEMI!
	;
	
returnStatement returns [ ReturnStatement ret = null ]
	{
		Expression e = null;
	}
	:	root:RETURN^ (e=expression)? SEMI!
		{
			ret = new ReturnStatement(e, #root.GetLocation());
		}
	;
	
throwStatement
	:	THROW^ (expression)? SEMI!
	;

tryStatement
	:	TRY^ block (catchClause)+ (finallyClause)?
	;
	
catchClause
	:	CATCH^ (LPAREN! classType IDENTIFIER RPAREN!)? block
	;

finallyClause
	:	FINALLY^ block
	;
	
// B.2.7 Classes

classMemberDeclarations
	:	(fieldDeclaration) => fieldDeclaration
	|	methodDeclaration
	;
	
fieldDeclaration
	{
		Type t = null;
		Modifiers mods = Modifiers.None;
		Expression initExpr = null;
	}
	:	mods=fieldModifiers t=type i:IDENTIFIER (ASSIGN initExpr=fieldInitializer)? SEMI
		{
			m_currentProgram.CreateField(#i.getText(), mods, t, initExpr, #i.GetLocation());
		}
	;
	
fieldModifiers returns [ Modifiers mods = Modifiers.None ]
	{
		Modifiers mod = Modifiers.None;
	}
	:	(mod=fieldModifier { mods |= mod; })* 
	;
	
fieldModifier returns [ Modifiers mods = Modifiers.None ]
	:	STATIC	{ mods = Modifiers.Static; }
	|	PUBLIC	{ mods = Modifiers.Public; }
	|	PRIVATE	{ mods = Modifiers.Private; }
	|	CONST	{ mods = Modifiers.Const; }
	;

fieldInitializer returns [ Expression exp = null ]
	:	exp=expression
	;

methodDeclaration returns [ MethodDeclaration m = null ]
	{
		string n;
		Type t;
		Modifiers mods;
		Block b;
	}
	:	mods=methodModifiers t=returnType n=typeName root:LPAREN!
		{
			m_currentMethod = new MethodDeclaration(n, mods, t, #root.GetLocation());
		}
		(p:formalParameterList)? RPAREN! 
		{
		}
		b=block
		{
			m_currentMethod.Block = b;
			m = m_currentMethod;
			
			m_currentProgram.AddMethod(m);
		}
	;
	
returnType returns [ Type ret = null ]
	:	VOID	{ ret = Type.GetType("System.Void"); }
	|	ret=type
	;
	
methodModifiers returns [ Modifiers mods = Modifiers.None ]
	{
		Modifiers mod = Modifiers.None;
	}
	:	(mod=methodModifier { mods |= mod; })* 
	;
	
methodModifier returns [ Modifiers mods = Modifiers.None ]
	:	STATIC	{ mods = Modifiers.Static; }
	|	PUBLIC	{ mods = Modifiers.Public; }
	|	PRIVATE	{ mods = Modifiers.Private; }
	;
	
formalParameterList
	:	fixedParameter (COMMA! fixedParameter)* (COMMA! parameterArray)?
	|	parameterArray
	;
	
fixedParameter
	{
		Type t = null;
	}
	:	(parameterModifier)? t=type i:IDENTIFIER
		{
			m_currentMethod.CreateParameter(#i.getText(), t, #i.GetLocation());
		}
	;
	
parameterModifier
	:	REF
	|	OUT
	;
	
parameterArray
	:	PARAMS arrayType IDENTIFIER
	;

variableInitializer [ Type t ] returns [ Expression exp = null ]
	:	exp = expression 
	|	exp = arrayInitializer[t]
	;

// B.2.9 Arrays

arrayInitializer [ Type t ] returns [ Expression exp = null ]
	{
		List<Expression> expList = new List<Expression>();
		// Get subarray type. If the main array is int[][], then subarray is int[]
		Type t2 = null;
	}
	:	root:LBRACE
		{
			if(t != null)
			{
				t2 = t.GetElementType();
				if(t2 == null)
				{
					throw new CompileException("using array initializer to non-array type", #root.GetLocation());
				}
			}
		}
		(
			exp=variableInitializer[t2] { expList.Add(exp); }
			(
				options {greedy=true;}:
				COMMA exp=variableInitializer[t2] { expList.Add(exp); }
			)*
			(COMMA)?
		)?
		RBRACE
		{
			exp = new ArrayCreationExpression(t, null, expList, #root.GetLocation());
		}
	;	

// B.1.8 Literals

literal returns [ Expression exp = null ]
	:	exp=booleanLiteral
	|	il:INTEGER_LITERAL		{ exp = new IntegerLiteralExpression(#il.getText(), #il.GetLocation()); }
	|	rl:REAL_LITERAL			{ exp = new FloatLiteralExpression(#rl.getText(), #rl.GetLocation()); }
	|	CHARACTER_LITERAL	// TODO
	|	sl:STRING_LITERAL		{ exp = new StringLiteralExpression(#sl.getText(), #sl.GetLocation()); }
	|	exp=nullLiteral
	;

booleanLiteral returns [ Expression exp = null ]
	:	tn:TRUE		{ exp = new BooleanLiteralExpression(true, #tn.GetLocation()); }
	|	fn:FALSE	{ exp = new BooleanLiteralExpression(false, #fn.GetLocation()); }
	;

nullLiteral returns [ Expression exp = null ]
	:	n:NULL
		{
			exp = new NullLiteralExpression(#n.GetLocation());
		}
	;



//**********************************************************************************************************

/***
 * B.1 LEXICAL GRAMMAR
 */

class BatLexer extends Lexer;

options
{
	k = 3;							// lookahead
	charVocabulary='\u0003'..'\uFFFF';
//	exportVocab=BatMud;
	testLiterals=false;
	classHeaderPrefix = "";
}

// B.1.7 Keywords
tokens
{
	AS          = "as";
	BOOL        = "bool";
	BREAK       = "break";
	CASE        = "case";
	CATCH       = "catch";
	CHAR        = "char";
	CONTINUE    = "continue";
	CONST		= "const";
	DO          = "do";
	ELSE        = "else";
	FALSE       = "false";
	FINALLY     = "finally";
	FLOAT32     = "float";
	FLOAT64     = "float64";
	FOR         = "for";
	FOREACH     = "foreach";
	IF          = "if";
	IN          = "in";
	INT8        = "int8";
	INT16       = "int16";
	INT32       = "int";
	INT64       = "int64";
	IS          = "is";
	MIXED		= "mixed";
	NEW         = "new";
	NULL        = "null";
	BATOBJECT	= "batobject";
	OUT         = "out";
	PUBLIC		= "public";
	PRIVATE		= "private";
	BATPROGRAM	= "batprogram";
	REF         = "ref";
	RETURN      = "return";
	SIZEOF      = "sizeof";
	STATIC		= "static";
	STRING      = "string";
	SWITCH      = "switch";
	THROW       = "throw";
	TRUE        = "true";
	TRY         = "try";
	TYPEOF      = "typeof";
	UINT8       = "uint8";
	UINT16      = "uint16";
	UINT32      = "uint";
	UINT64      = "uint64";
	USING       = "using";
	VOID        = "void";
	WHILE       = "while";
}

/**
 * B.1 Lexical Grammar
 *
 * Input
 *   : (InputSection)*
 *   ;
 *
 * Input_Section
 *   : (InputElement)* NewLine
 *   | PPDirective
 *   ;
 *
 * InputElement
 *   : WhiteSpace
 *   | Comment
 *   | Token
 *   ;
 */

// B.1.1 Line terminators
protected
NEWLINE
  : ( { LA(2) == '\u000A' }? '\u000D' '\u000A'   // cr followed by lf
    | '\u000D'                // Carriage return
    | '\u000A'                // Line feed
    | '\u0085'                // Next line
    | '\u2028'                // Line separator
    | '\u2029'                // Paragraph separator
    )
    { newline(); }
  ;

protected
NewLineChar
  : ( '\u000D' | '\u000A' | '\u0085' | '\u2028' | '\u2029' )
  ;

protected
NotNewLineChar
  : ~( '\u000D' | '\u000A' | '\u0085' | '\u2028' | '\u2029' )
  ;


// B.1.2 White space
WHITESPACE
  : ( ' '                     // Any character with Unicode class Zs
    | '\u0009'                // Horizontal tab
    | '\u000B'                // Vertical tab
    | '\u000C'                // Form feed
	|	NEWLINE				  // New line
    )+
    { $setType(Token.SKIP); }
  ;

// B.1.3 Comments
SINGLELINECOMMENT
  : "//" (NotNewLineChar)* NEWLINE
    { $setType(Token.SKIP); }
  ;

DELIMITEDCOMMENT
  : "/*"                      // start comment
    ( { LA(2) != '/' }? '*'   // asterisk which is not followed by '/'
	|	NEWLINE				  // new line
    | ~( '*'| '\u000D' | '\u000A' | '\u0085' | '\u2028' | '\u2029' )
    )*
    "*/"                      // end comment
    { $setType(Token.SKIP); }
  ;

/**
 * B.1.4 Tokens
 *
 * Token
 *  : Identifier
 *  | Keyword
 *  | IntegerLiteral
 *  | RealLiteral
 *  | CharacterLiteral
 *  | StringLiteral
 *  | OperatorOrPunctuator
 *  ;
 */

// B.1.5 Unicode character escape sequences
protected
UnicodeEscapeSequence
  : '\\' 'u' HexDigit HexDigit HexDigit HexDigit
  | '\\' 'U' HexDigit HexDigit HexDigit HexDigit
             HexDigit HexDigit HexDigit HexDigit
  ;

// B.1.6 Identifiers
IDENTIFIER
options { testLiterals = true; }
  : IdentifierStartCharacter ( IdentifierCharacter )*
  ;

protected
IdentifierStartCharacter
  : ('a'..'z'|'A'..'Z'|'_')
  ;

protected
IdentifierCharacter
  : ('a'..'z'|'A'..'Z'|'_'|'0'..'9')
  ;

/**
 * B.1.8 Literals
 *
 * Literal
 *   : BooleanLiteral
 *   | IntegerLiteral
 *   | RealLiteral
 *   | CharacterLiteral
 *   | StringLiteral
 *   | NullLiteral
 *   ;
 */

NUMERIC_LITERAL
  : ( (DecimalDigit)+ '.' (DecimalDigit) )=>
    (DecimalDigit)+ '.' (DecimalDigit)+ (ExponentPart)?
    { $setType( REAL_LITERAL ); }
  | ( '.' (DecimalDigit) )=>
    '.' (DecimalDigit)+ (ExponentPart)?
    { $setType( REAL_LITERAL ); }
  | ( (DecimalDigit)+ (ExponentPart) )=>
    (DecimalDigit)+ (ExponentPart)
    { $setType( REAL_LITERAL ); }
  | (DecimalDigit)+
    { $setType( INTEGER_LITERAL ); }
  | "0x" (HexDigit)+
    { $setType( INTEGER_LITERAL ); }
  | "0b" (BinaryDigit)+
    { $setType( INTEGER_LITERAL ); }
  | '.' 
	{ $setType( DOT ); }			// just a dot, not a token because of ambigiousness with real literals
  ;
  
protected
DecimalDigit
  : ( '0' | '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9' )
  ;

protected
HexDigit
  : ( '0' | '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9'
    | 'A' | 'B' | 'C' | 'D' | 'E' | 'F' 
    | 'a' | 'b' | 'c' | 'd' | 'e' | 'f' )
  ;

protected
BinaryDigit
	:	( '0' | '1' )
	;

protected
ExponentPart
  : 'e' ( '+' | '-' )? ( DecimalDigit )+
  | 'E' ( '+' | '-' )? ( DecimalDigit )+
  ;

CHARACTER_LITERAL
  : '\'' Character '\''
  ;

protected
Character
  : SingleChar
  | SimpleEscapeSequence
  | HexadecimalEscapeSequence
  | UnicodeEscapeSequence
  ;

protected
SingleChar
  : ~( '\'' | '\\' | '\u000D' | '\u000A' | '\u0085' | '\u2028' | '\u2029' )
  ;

protected
SimpleEscapeSequence
  : "\\'"  | "\\\"" | "\\\\" | "\\0"  | "\\a"
  | "\\b"  | "\\f"  | "\\n"  | "\\r"  | "\\t"  | "\\v"
  ;

protected
HexadecimalEscapeSequence
// turn off ambigious warnings for antlr
  : "\\x" HexDigit ( options { warnWhenFollowAmbig = false; }:
					 HexDigit ( options { warnWhenFollowAmbig = false; }:
					            HexDigit ( options { warnWhenFollowAmbig = false; }:
					                       HexDigit
					                     )?
					          )?
			       )?
  ;

STRING_LITERAL
  : RegularStringLiteral
  | VerbatimStringLiteral
  ;

protected
RegularStringLiteral
	:	'\"' ( RegularStringChar )* '\"'
  		{
  			string str = $getText;
  			str = str.Substring(1, str.Length - 2);
  			$setText(str);
  		}
	;

protected
RegularStringChar
  : SingleRegularStringChar
  | SimpleEscapeSequence
  | HexadecimalEscapeSequence
  | UnicodeEscapeSequence
  ;

protected
SingleRegularStringChar
  : ~( '\"' | '\\' | '\u000D' | '\u000A' | '\u0085' | '\u2028' | '\u2029' )
  ;

protected
VerbatimStringLiteral
	:	"@\"" ( VerbatimStringChar )* '\"'
  		{
  			string str = $getText;
  			str = str.Substring(2, str.Length - 3);
  			$setText(str);
  		}
	;


protected
VerbatimStringChar
  : ~( '\"' )
  | "\"\""
  ;

// B.1.9 Operators and punctuators

LBRACE     : '{';    RBRACE     : '}';
LBRACK     : '[';    RBRACK     : ']';
LPAREN     : '(';    RPAREN     : ')';

PLUS       : '+';    PLUS_ASN   : "+=";
MINUS      : '-';    MINUS_ASN  : "-=";
STAR       : '*';    STAR_ASN   : "*=";
DIV        : '/';    DIV_ASN    : "/=";
MOD        : '%';    MOD_ASN    : "%=";
INC        : "++";   DEC        : "--";

SHL        : "<<";   SHL_ASN    : "<<=";
SHR        : ">>";   SHR_ASN    : ">>=";

BAND       : '&';    BAND_ASN   : "&=";
BOR        : '|';    BOR_ASN    : "|=";
BXOR       : '^';    BXOR_ASN   : "^=";
BNOT       : '~';

ASSIGN     : '=';    EQ         : "==";
LTHAN      : '<';    LE         : "<=";
GTHAN      : '>';    GE         : ">=";
LNOT       : '!';    NE         : "!=";
LOR        : "||";   LAND       : "&&";

COMMA      : ',';
COLON      : ':';    SEMI       : ';';
QUESTION   : '?';

DOLLAR		: '$';

