tree grammar GelGen;

options {
	tokenVocab=Gel;
	language=CSharp; 
	ASTLabelType=CommonTree;
}

@namespace { Gel.Compiler.Parser }

@header
{
	using Gel.Compiler.Ast;
	using Gel.Core;
	using System.Text;
	using System.Collections.Generic;
}

@rulecatch {
	catch (RecognitionException re) {
		Console.WriteLine("RECO ERROR");
		m_numErrors++;
		ReportError(re);
		Recover(input, re);
	}
}

@members
{
	Program m_program;
	MethodDeclaration m_method;

	public int m_numErrors;
}

program returns [ Program prog ]
@init { 
	m_program = new Program("testprogram"); 
	m_method = new MethodDeclaration("main", Modifiers.Static, typeof(int), new Location());
	m_program.AddMethod(m_method); 
}
	:	usingDeclarations*
		(	classMemberDeclaration
		|	s=statement { m_method.Block.AddStatement(s); }
		)*
		{ 
			$prog = m_program;
		}
	;

statementProgram returns [ Program prog ]
@init { 
	m_program = new Program("testprogram"); 
	m_method = new MethodDeclaration("main", Modifiers.Static, typeof(object), new Location());
	m_program.AddMethod(m_method); 
}
	:	slist=statementList { m_method.Block.StatementList = slist; } 
		{
			$prog = m_program; 
		}
	;
	
expressionProgram returns [ Program prog ]
@init { 
	m_program = new Program("testprogram"); 
	m_method = new MethodDeclaration("main", Modifiers.Static, typeof(object), new Location());
	m_program.AddMethod(m_method); 
}
	:	e=expression { m_method.Block.AddStatement(new ReturnStatement(e, e.Location)); }
		{ 
			$prog = m_program; 
		}
	;


usingDeclarations
	:	^('using' ns=namespaceOrTypeName) { ResolveContext.AddUsingNamespace(ns); }
	;

namespaceOrTypeName returns [ string name ]
	:	^(QID
		{
			StringBuilder sb = new StringBuilder();
		}
			(i1=IDENTIFIER 
				{
					if(sb.Length > 0)
						sb.Append(".");
					sb.Append(i1.Text);
				} 
			)+
		)
		{
			$name = sb.ToString();
		}
	;

classMemberDeclaration
	:	methodDeclaration
	;

methodDeclaration
	@init { MethodDeclaration oldMethod; }
	:	^(r=FUNC_DECL /*(m=modifiers)?*/ (t=type)? n=IDENTIFIER
		{
			oldMethod = m_method;
			m_method = new MethodDeclaration(n.Text, Modifiers.Static, t, r);
			m_program.AddMethod(m_method);
		}
		parameterList?
		b=block)
		{
			m_method.Block = b;
			m_method = oldMethod;
		}
	;
	
parameterList
	:	^(PARAM_LIST parameter+)
	;

parameter
	:	t=type n=IDENTIFIER { m_method.CreateParameter(n.Text, t, n); }
	;
	
/* XXX Antlr bugs, can't return valuetypes. later then.
modifiers returns [ Modifiers mods ]
	:	^(MODIFIERS (m=modifier { mods |= m; } )*)
	;

modifier returns [ Modifiers mod ]
	:	'public'	{ mod = Modifiers.Public; }
	|	'private'	{ mod = Modifiers.Private; }
	|	'static'	{ mod = Modifiers.Static; }
	;
*/		
// Types

type returns [ Type ret ]
	:	^(r=TYPE bt=basicType (tlist=typeArgumentList)? (rank=typeRank)?)
		{
			StringBuilder typeName = new StringBuilder(bt);
			if(tlist != null)
			{
				typeName.Append("`");
				typeName.Append(tlist.Count.ToString());
				typeName.Append("[");
				
				bool first = true;
				foreach(Type targ in tlist)
				{
					if(!first)
						typeName.Append(",");
					typeName.Append(targ.FullName);
					first = false;
				}
				typeName.Append("]");
			}
			
			for(int i = 0; i < rank; i++)
				typeName.Append("[]");
			
			Type type = ResolveContext.FindType(typeName.ToString());
			if(type == null)
				throw new CompileException("Type not found: " + typeName.ToString(),  r);
			$ret = type;
		}
	;
	
typeRank returns [ int ret ] 
	:	^(RANK ('[' ']' { ret++; } )+)
	;

typeArgumentList returns [ List<Type> list = new List<Type>() ]
	:	^(TYPEARGS (t=type { list.Add(t); } )+)
	;

basicType returns [ string ret ]
	:	n=builtinType { ret = n; }
	|	n=namespaceOrTypeName { ret = n; }
	;

builtinType returns [ string ret ]
	:	'void'		{ ret = "System.Void"; }
	|	'sbyte'		{ ret = "System.SByte"; }
	|	'byte'		{ ret = "System.Byte"; }
	|	'short'		{ ret = "System.Int16"; }
	|	'ushort'	{ ret = "System.UInt16"; }
	|	'int'		{ ret = "System.Int32"; }
	|	'uint'		{ ret = "System.UInt32"; }
	|	'long'		{ ret = "System.Int64"; }
	|	'ulong'		{ ret = "System.UInt64"; }
	|	'char'		{ ret = "System.Char"; }
	|	'bool'		{ ret = "System.Boolean"; }
	|	'decimal'	{ ret = "System.Decimal"; }
	|	'float'		{ ret = "System.Single"; }
	|	'double'	{ ret = "System.Double"; }
	|	'object'	{ ret = "System.Object"; }
	|	'string'	{ ret = "System.String"; }
	;


// Expressions

expression returns [ Expression ret ]
	:	(	e=assignmentExpression
		|	e=arithmeticExpression
		|	e=comparisonExpression
		|	e=simpleName 
		|	e=literal
		|	e=invocationExpression
		|	e=memberAccessExpression
		|	e=postfixExpression
		|	e=elementAccessExpression
		|	e=objectCreationExpression
		|	e=applyExpression
		|	e=castExpression
		|	e=unaryExpression
		)
		{
			ret = e;
		}
	;

castExpression returns [ Expression ret ]
	:	^(r=CAST t=type e=expression)
		{
			ret = new TypeCastExpression(t, e, r);
		}
	;
	
applyExpression returns [ Expression ret ]
	:	^(r1=APPLY stream=expression 
			^(r2=CLOSURE t=type n=IDENTIFIER (f=filter)? (body=statementList)?) 
		)
		{
			ClosureExpression closure = new ClosureExpression(t, n.Text, body, r2);
			ret = new ApplyExpression(stream, t, n.Text, f, closure, r1);
		}
	;
	
streamClosure returns [ Expression ret ]
	:	^(r=CLOSURE t=type n=IDENTIFIER (f=filter)? (body=statementList)?)
		{  }
	;

filter returns [ Expression ret]
	:	^(FILTER e=expression)
		{ ret = e; }
	;
	
objectCreationExpression returns [ Expression ret ]
	:	^(r='new' t=type (a=argumentList)?)
		{
			ret = new NewExpression(t, a, r);
		}
	;
	
argumentList returns [ List<Expression> elist = new List<Expression>() ]
	:	^(ELIST (e=expression { elist.Add(e); } )*)
	;

assignmentExpression returns [ Expression ret ]
	:	^(li='=' e1=expression e2=expression) { ret = new AssignmentExpression(e1, e2, li); }
	;

invocationExpression returns [ Expression ret ]
	:	^(INVOCATION t=expression (a=argumentList)?)
		{ ret = new InvocationExpression(t, a, t.Location); }
	;

memberAccessExpression returns [ Expression ret ]
	:	^(MEMBER e=expression i=IDENTIFIER)
		{ ret = new MemberAccessExpression(e, i.Text, i); }
	;

postfixExpression returns [ Expression ret ]
	:	^(POSTINC e=expression) { ret = new PostfixExpression(PostfixExpression.Operator.Inc, e, e.Location); }
	|	^(POSTDEC e=expression) { ret = new PostfixExpression(PostfixExpression.Operator.Dec, e, e.Location); }
	;
	
arithmeticExpression returns [ Expression expr ]
	@init { ArithmeticExpression.Operator op; Location loc; }
	:	^(arithmeticOp[out op, out loc] e1=expression e2=expression) 
		{ $expr = new ArithmeticExpression(op, e1, e2, loc); }
	;

arithmeticOp [ out ArithmeticExpression.Operator op, out Location loc ]
	@init{ op = ArithmeticExpression.Operator.None; }
	@finally { loc = new Location(o); }
	:	o='+' { $op = ArithmeticExpression.Operator.Plus; }
	|	o='-' { $op = ArithmeticExpression.Operator.Minus; }
	|	o='*' { $op = ArithmeticExpression.Operator.Mul; }
	|	o='/' { $op = ArithmeticExpression.Operator.Div; }
	|	o='%' { $op = ArithmeticExpression.Operator.Mod; }
	|	o='&' { $op = ArithmeticExpression.Operator.BAnd; }
	|	o='^' { $op = ArithmeticExpression.Operator.BXor; }
	|	o='|' { $op = ArithmeticExpression.Operator.BOr; }
	|	o='>>' { $op = ArithmeticExpression.Operator.Shl; }
	|	o='<<' { $op = ArithmeticExpression.Operator.Shr; }
	;
	
comparisonExpression returns [ Expression expr ]
	@init { ComparisonExpression.Operator op; Location loc; }
	:	^(comparisonOp[out op, out loc] e1=expression e2=expression) 
		{ $expr = new ComparisonExpression(op, e1, e2, loc); }
	;

comparisonOp [ out ComparisonExpression.Operator op, out Location loc ]
	@init{ op = ComparisonExpression.Operator.None; }
	@finally { loc = new Location(o); }
	:	o='==' { $op = ComparisonExpression.Operator.EQ; }
	|	o='!=' { $op = ComparisonExpression.Operator.NE; }
	|	o='>' { $op = ComparisonExpression.Operator.GT; }
	|	o='<' { $op = ComparisonExpression.Operator.LT; }
	|	o='>=' { $op = ComparisonExpression.Operator.GE; }
	|	o='<=' { $op = ComparisonExpression.Operator.LE; }
	;
	
unaryExpression returns [ Expression expr ]
	@init { UnaryExpression.Operator op; Location loc; }
	:	^(unaryOp[out op, out loc] e=expression) 
		{ $expr = new UnaryExpression(op, e, loc); }
	;

unaryOp [ out UnaryExpression.Operator op, out Location loc ]
	@init{ op = UnaryExpression.Operator.None; }
	@finally { loc = new Location(o); }
	:	o=UNARY_PLUS { $op = UnaryExpression.Operator.Plus; }
	|	o=UNARY_MINUS { $op = UnaryExpression.Operator.Minus; }
	|	o='!' { $op = UnaryExpression.Operator.LNot; }
	|	o='~' { $op = UnaryExpression.Operator.BNot; }
	;
	
	
elementAccessExpression returns [ Expression ret ]
	:	^(r=ELEMENT e=expression elist=expressionList)
		{
			ret = new ElementAccessExpression(e, elist, r);
		}
	;
	
simpleName returns [ Expression expr ]
	:	i=IDENTIFIER { $expr = new SimpleName(i.Text, i); }
	;

literal returns [ Expression expr ]
	:	l=BOOLEAN_LITERAL { $expr = new BooleanLiteralExpression(l.Text == "true" ? true : false, l); }
	|	l=INTEGER_LITERAL { $expr = new IntegerLiteralExpression(l.Text, l); }
	|	l=CHARACTER_LITERAL { $expr = new CharacterLiteralExpression(l.Text[0], l); }
	|	l=STRING_LITERAL { $expr = new StringLiteralExpression(l.Text, l); }
	|	l='null' { $expr = new NullLiteralExpression(l); }
	;

expressionList returns [ List<Expression> elist = new List<Expression>() ]
	:	^(ELIST (e=expression { elist.Add(e); } )+)
	;

// Statements

statement returns [ Statement ret ]
	:	(	s=declarationStatement
		|	s=returnStatement
		|	s=ifStatement
		|	s=whileStatement
		|	s=forStatement
		|	s=statementList
		|	s=block
		|	s=expressionStatement
		)
		{
			ret = s;
		}
	;

block returns [ Block ret ]
	:	^(BLOCK (slist=statementList)?)
		{
			ret = new Block(); 
			ret.StatementList = slist;
		}
	;
	
declarationStatement returns [ Statement s ]
	:	^(r=VAR_DECL t=type n=IDENTIFIER (e=variableInitializer[t])?)
		{
			s = new LocalDeclaration(t, n.Text, e, r);
		}
	;

returnStatement returns [ Statement ret ]
	:	^(l='return' (e=expression)) { ret = new ReturnStatement(e, l); }
	;
	
ifStatement returns [ Statement ret ]
	:	^(r='if' cond=expression truestm=statement (falsestm=statement)?)
		{
			ret = new IfStatement(cond, truestm, falsestm, r);
		}
	;
	
variableInitializer [ Type t ] returns [ Expression e ]
	@init
	{
		List<Expression> expList = new List<Expression>();
		Type subType = null; 
	}
	:	exp=expression { e = exp; }
	|	^(r=ARRAY_CREATION
		{
			if(t != null)
			{
				subType = t.GetElementType();
				if(subType == null)
				{
					throw new CompileException("using array initializer to non-array type", r);
				}
			}
		}
		(exp=variableInitializer[subType] { expList.Add(exp); } )*)
		{
			e = new ArrayCreationExpression(t, null, expList, r);
		}
	;

whileStatement returns [ Statement ret ]
	:	^(r='while' e=expression (s=statement)?)
		{
			ret = new WhileStatement(e, s, r);
		}
	;

forStatement returns [ Statement ret ]
	:	^(r='for' init=forInitializer cond=forCondition iter=forIterator (body=statement)?)
		{
			ret = new ForStatement(init, cond, iter, body, r);
		}
	;

forInitializer returns [ Statement ret ]
	:	^(FOR_INIT s=statement) { ret = s; }
	|	// none
	;

forCondition returns [ Expression ret ]
	:	^(FOR_COND e=expression) { ret = e; }
	|	// none
	;
	
forIterator returns [ Statement ret ]
	:	^(FOR_ITER s=statement)  { ret = s; }
	|	// none
	;

expressionStatement returns [ Statement ret ]
	:	e=expression { ret = new ExpressionStatement(e); }
	;
	
statementList returns [ StatementList slist = new StatementList() ]
	:	^(SLIST (s=statement { slist.Add(s); } )+)
	;
