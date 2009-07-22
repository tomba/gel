grammar Gel;

options {
	language=CSharp;
	backtrack=true;
	memoize=true;
	output=AST;
	ASTLabelType=CommonTree;
}

tokens {
	ELEMENT;	// Element access: foo[x]
	MEMBER;		// Member access: foo.x
	INVOCATION;	// Call: foo()
	POSTINC;
	POSTDEC;
	PREINC;
	PREDEC;
	
	VAR_DECL;
	FUNC_DECL;
	APPLY;		// stream apply: f : { char c | ... }
	CLOSURE;	// Stream closure
	FILTER;		// Filter for closure
	
	PARAM_LIST;
	MODIFIERS;
	
	TYPE;
	TYPEARGS;
	RANK;
	
	BLOCK;	// Block of statements with their own scope
	SLIST;	// Statement list
	ELIST;	// Expression list. Arguments for calls and ctors
	
	ARRAY_CREATION;
	
	FOR_INIT;
	FOR_COND;
	FOR_ITER;
	
	CAST;
	UNARY_MINUS;
	UNARY_PLUS;
	
	QID;
}

@namespace { Gel.Compiler.Parser }

@header
{
#pragma warning disable 219
	using System.Text;
}


/*------------------------------------------------------------------
 * PARSER RULES
 *------------------------------------------------------------------*/

@rulecatch {
	catch (RecognitionException re) {
		Console.WriteLine("RECO ERROR");
		m_numErrors++;
		ReportError(re);
		Recover(input, re);
	}
}

@parser::members
{
	public int m_numErrors;
	
	public ITree ParseProgram()
	{
		program_return r = program();
		return (ITree)r.Tree;
	}

	public ITree ParseStatement()
	{
		statementList_return r = statementList();
		return (ITree)r.Tree;
	}
	
	public ITree ParseExpression()
	{
		expression_return r = expression();
		return (ITree)r.Tree;
	}
}

program
	:	usingDeclarations*
		(	classMemberDeclaration
		|	statement
		)*
	;

usingDeclarations
	:	'using' namespaceName ';' -> ^('using' namespaceName)
	;

classMemberDeclaration
	:	methodDeclaration
	;

methodDeclaration
	:	modifiers? type IDENTIFIER '(' parameterList? ')' block 
		-> ^(FUNC_DECL modifiers? type IDENTIFIER parameterList? block)
	;
	
parameterList
	:	parameter (',' parameter)* -> ^(PARAM_LIST parameter+)
	;

parameter
	:	type IDENTIFIER
	;

modifiers
	:	modifier+ -> ^(MODIFIERS modifier+)
	;
	
modifier
	:	'public'
	|	'private'
	|	'static'
	;
	
// B.2 Syntactic Grammar

// B.2.1 Basic concepts

namespaceName
	:	namespaceOrTypeName
	;
	
typeName
	:	namespaceOrTypeName
	;
	
namespaceOrTypeName
	:	IDENTIFIER ( '.' IDENTIFIER)* -> ^(QID IDENTIFIER+)
	;

// B.2.2 Types

type
	:	basicType typeArgumentList? typeRank? 
		-> ^(TYPE basicType typeArgumentList? typeRank?)
	;
	
typeRank
	:	('[' ']')+ -> ^(RANK ('[' ']')+)
	;

typeArgumentList
	:	'<' type (',' type)* '>' -> ^(TYPEARGS type+)
	;
	
basicType
	:	builtinType
	|	typeName
	;	
	
builtinType
	:	'void'
	|	'sbyte'
	|	'byte'
	|	'short'
	|	'ushort'
	|	'int'
	|	'uint'
	|	'long'
	|	'ulong'
	|	'char'
	|	'bool'
	|	'decimal'
	|	'float'
	|	'double'
	|	'object'
	|	'string'
	;


/*
 *
 * B.2.4 Expressions
 *
 */

argumentList
	:	argument (',' argument)* -> ^(ELIST argument+)
	;
	
argument
	:	expression
	;

primaryExpression
	:	(primaryExpressionStart->primaryExpressionStart)
		(	'++' 				-> ^(POSTINC $primaryExpression)
		|	'--'				-> ^(POSTDEC $primaryExpression)
		|	'.' i=IDENTIFIER		-> ^(MEMBER $primaryExpression $i)
		|	'(' argumentList? ')'		-> ^(INVOCATION $primaryExpression argumentList?)
		|	'[' elist=expressionList ']'	-> ^(ELEMENT $primaryExpression $elist)
		|	':' streamClosure 		-> ^(APPLY $primaryExpression streamClosure)
		)*
	;

primaryExpressionStart
	:	literal
	|	simpleName
	|	parenthesizedExpression
	//|	elementAccess
	|	thisAccess
	|	baseAccess
	|	objectCreationExpression
	//|	delegateCreationExpression
	|	typeofExpression
	|	'delegate' '(' parameterList? ')' block // I guess this can't return a real delegate, as we can't create new types
	;

simpleName
	:	IDENTIFIER
	;

parenthesizedExpression
	:	'('! assignmentExpression ')'!
	;

expressionList
	:	expression (',' expression)* -> ^(ELIST expression+)
	;

thisAccess
	:	'this'
	;
	
baseAccess
	:	'base' '.' IDENTIFIER
	|	'base' '[' expressionList ']'
	;

objectCreationExpression
	:	'new'^^ type '('! argumentList? ')'!
	;
	
typeofExpression
	:	'typeof' '(' type ')'
	;
	
unaryExpression
	:	primaryExpression
	|	'+' unaryExpression		-> ^(UNARY_PLUS unaryExpression)
	|	'-' unaryExpression		-> ^(UNARY_MINUS unaryExpression)
	|	'!'^^ unaryExpression
	|	'~'^^ unaryExpression
	|	'++' unaryExpression	-> ^(PREINC unaryExpression)
	|	'--' unaryExpression	-> ^(PREINC unaryExpression)
	|	'(' type ')' primaryExpression -> ^(CAST type primaryExpression)
	;
		
multiplicativeExpression
	:	unaryExpression ( ('*' | '/' | '%')^^ unaryExpression )*
	;

additiveExpression
	:	multiplicativeExpression ( ('+' | '-')^^ multiplicativeExpression )*
	;
	
shiftExpression
	:	additiveExpression ( ('<<' | '>>')^^ additiveExpression )*
	;

relationalExpression
	:	shiftExpression 
		( ( ('<' | '>' | '<=' | '>=')^^ shiftExpression )*
		| ( ( 'is' | 'as' )^^ type )
		)
	;
	
equalityExpression
	:	relationalExpression ( ('==' | '!=')^^ relationalExpression )*
	;
	
andExpression
	:	equalityExpression ( '&'^^ equalityExpression )*
	;
	
exclusiveOrExpression
	:	andExpression ( '^'^^ andExpression )*
	;
	
inclusiveOrExpression
	:	exclusiveOrExpression ( '|'^^ exclusiveOrExpression )*
	;

conditionalAndExpression
	:	inclusiveOrExpression ( '&&'^^ inclusiveOrExpression )*
	;

conditionalOrExpression
	:	conditionalAndExpression ( '||'^^ conditionalAndExpression )*
	;

// could the first conditionalAndExpression be primaryExpression?
assignmentExpression
	:	conditionalAndExpression ( assignmentOperator^^ conditionalOrExpression )?
	;
	
assignmentOperator
	:	'='
	|	'+='
	|	'-='
	|	'*='
	|	'/='
	|	'&='
	|	'|='
	|	'^='
	|	'%='
	;
	
expression
	:	assignmentExpression
	;

constantExpression
	:	expression
	;
	
booleanExpression
	:	expression
	;
	
streamClosure
	:	'{'
		type IDENTIFIER
		(',' equalityExpression)?
		('|' statementList)?
		'}'
		-> ^(CLOSURE type IDENTIFIER ^(FILTER equalityExpression)? statementList? )
	;    


/*
 *
 * Statements
 *
 */

statement
	:	declarationStatement 
	|	embeddedStatement 
	;
	
embeddedStatement
	:	block
	|	emptyStatement 
	|	expressionStatement
	|	selectionStatement 
	|	iterationStatement 
	|	jumpStatement
//	|	tryStatement 
//	|	usingStatement 
	;
	
block	:	'{' statementList? '}' -> ^(BLOCK statementList?)
	;
	
statementList
	:	statement (statement)* -> ^(SLIST statement+)
	;
	
emptyStatement
	:	';'!
	;
	
declarationStatement
	:	type! localVariableDeclarator[$type.tree] (','! localVariableDeclarator[$type.tree])* ';'! 
	;

localVariableDeclarator [ CommonTree type ]
	:	IDENTIFIER ('=' variableInitializer)? -> ^(VAR_DECL {$type} IDENTIFIER variableInitializer?)
	;

variableInitializer
	:	expression
	|	'{' (variableInitializer (',' variableInitializer)*)? '}' -> ^(ARRAY_CREATION variableInitializer*)
	;


expressionStatement
	:	expression ';'!
	;
	
selectionStatement
	:	ifStatement 
//	|	switchStatement
	;

ifStatement
	:	'if' '(' booleanExpression ')' s1=embeddedStatement //('else' s2=embeddedStatement)?
		-> ^('if' booleanExpression $s1) // $s2?) // XXX
	;
//    if ( boolean-expression ) embedded-statement else embedded-statement 


iterationStatement
	:	whileStatement 
//	|	doStatement 
	|	forStatement 
//	|	foreachStatement 
	;

whileStatement
	:	'while' '(' booleanExpression ')' embeddedStatement
		-> ^('while' booleanExpression embeddedStatement)
	;
	
forStatement
	:	'for' '(' forInitializer? ';' forCondition? ';' forIterator? ')' embeddedStatement
		-> ^('for' forInitializer? forCondition? forIterator? embeddedStatement)
	;
	
forInitializer
	:	declarationStatement -> ^(FOR_INIT declarationStatement)
	|	statementExpressionList -> ^(FOR_INIT statementExpressionList)
	;

forCondition
	:	booleanExpression -> ^(FOR_COND booleanExpression)
	;
	
forIterator
	:	statementExpressionList -> ^(FOR_ITER statementExpressionList)
	;

statementExpressionList
	:	expression (',' expression)* -> ^(SLIST expression+)
	;
	
    

jumpStatement
	:	'break' ';'!
	|	'continue' ';'!
	|	'return'^^ expression? ';'!
//	|	throwStatement 
    ;
    
    
 
 /*------------------------------------------------------------------
 * LEXER RULES
 *------------------------------------------------------------------*/


// B.1.1 Line terminators
fragment
NEWLINE
  : ( '\u000D' '\u000A'   // cr followed by lf
    | '\u000D'                // Carriage return
    | '\u000A'                // Line feed
    | '\u0085'                // Next line
    | '\u2028'                // Line separator
    | '\u2029'                // Paragraph separator
    )
  ;

fragment
NewLineChar
  : ( '\u000D' | '\u000A' | '\u0085' | '\u2028' | '\u2029' )
  ;

fragment
NotNewLineChar
  : ~( '\u000D' | '\u000A' | '\u0085' | '\u2028' | '\u2029' )
  ;

// B.1.2

WHITESPACE
  : ( ' '                     // Any character with Unicode class Zs
    | '\u0009'                // Horizontal tab
    | '\u000B'                // Vertical tab
    | '\u000C'                // Form feed
	|	NEWLINE				  // New line
    )+
    { $channel = HIDDEN; }
  ;


// B.1.3
SINGLELINECOMMENT
  : '//' (NotNewLineChar)* NEWLINE { $channel=HIDDEN; }
  ;

DELIMITEDCOMMENT
  : '/*' ( options { greedy=false; } : . )* '*/'
    { $channel=HIDDEN; }
  ;


// B.1.8
 
literal
	:	BOOLEAN_LITERAL
	|	INTEGER_LITERAL
	|	CHARACTER_LITERAL
	|	STRING_LITERAL
	|	'null'
	;

BOOLEAN_LITERAL 
	:	'true'
	|	'false'
	;

INTEGER_LITERAL
	:	DecimalIntegerLiteral IntegerTypeSuffix?
	|	HexadecimalIngeterLiteral IntegerTypeSuffix?
	|	BinaryIntegerLiteral IntegerTypeSuffix?
	;

fragment
IntegerTypeSuffix
	:	'u' | 'l' | 'ul' | 'lu' | 'U' | 'L' | 'UL' | 'LU'
	;
	
fragment
DecimalIntegerLiteral
	:	DecimalDigit+
	;
	
fragment
DecimalDigit
	:	( '0'..'9' )
	;

fragment
HexadecimalIngeterLiteral
	:	('0x' | '0X') HexDigit+
	;

fragment
HexDigit
	:	( '0'..'9' | 'a'..'f' | 'A'..'F' )
	;

fragment
BinaryIntegerLiteral
	:	'0b' ('0' | '1')+
	;

// TODO: RealLiteral

CHARACTER_LITERAL
	:	'\'' Character '\''
  		{
 			this.Text = this.Text.Substring(1, 1);
  		}
	;

fragment
Character
	:SingleChar
	| SimpleEscapeSequence
	| HexadecimalEscapeSequence
//	| UnicodeEscapeSequence // TODO
	;
  
fragment
SingleChar
	: ~( '\'' | '\\' | '\u000D' | '\u000A' | '\u0085' | '\u2028' | '\u2029' )
	;

fragment
SimpleEscapeSequence
	: '\\' ('\'' | '\"' | '\\' | '0' | 'a' | 'b' | 'f' | 'n' | 'r' | 't' | 'v' )
	;

fragment
HexadecimalEscapeSequence
	:	'\\x' HexDigit (HexDigit (HexDigit HexDigit? )? )?
	;

STRING_LITERAL
	:	RegularStringLiteral
  		{
 			this.Text = this.Text.Substring(1, this.Text.Length - 2);
  		}
	|	VerbatimStringLiteral
  		{
 			this.Text = this.Text.Substring(2, this.Text.Length - 3);
  		}
	;

fragment
RegularStringLiteral
	:	'\"' ( RegularStringLiteralCharacter )* '\"'
	;

fragment
RegularStringLiteralCharacter
  : SingleRegularStringChar
  | SimpleEscapeSequence
  | HexadecimalEscapeSequence
//  | UnicodeEscapeSequence // TODO
  ;

fragment
SingleRegularStringChar
  : ~( '\"' | '\\' | '\u000D' | '\u000A' | '\u0085' | '\u2028' | '\u2029' )
  ;

fragment
VerbatimStringLiteral
	:	'@' '\"' ( VerbatimStringLiteralCharacter )* '\"'
	;


fragment
VerbatimStringLiteralCharacter
	:	~( '\"' )
	|	'\"' '\"'
	;


// B.1.6
IDENTIFIER
	:	('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_')*
	;

