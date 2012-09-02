tree grammar JavaScriptTreeParser;

options {
	tokenVocab = JavaScript;
	language = CSharp2;
	ASTLabelType=CommonTree;
	backtrack=true;
}

@header {
using System;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.Expressions;
}

@members {
	public override void ReportError(RecognitionException e) { throw e; }

	public string UnescapeStringLiteral(string orig) {
		if (orig.IndexOf('\\') == -1)
			return orig.Substring(1, orig.Length - 2);

		var result = new System.Text.StringBuilder();
		for (int i = 1; i < orig.Length - 1; i++) {
			char ch = orig[i];
			if (ch == '\\') {
				ch = orig[++i];
				if (ch == '\'')
					result.Append('\'');
				else if (ch == '\"')
					result.Append('\"');
				else if (ch == 'b')
					result.Append('\b');
				else if (ch == 'f')
					result.Append('\f');
				else if (ch == 'n')
					result.Append('\n');
				else if (ch == 'r')
					result.Append('\r');
				else if (ch == 't')
					result.Append('\t');
				else if (ch == 'v')
					result.Append('\v');
				else if (ch == '0')
					result.Append('\0');
				else
					result.Append('\\').Append(ch);	// TODO: More escape sequences.
			}
			else
				result.Append(ch);
		}
		return result.ToString();
	}
}

public program returns [IList<JsStatement> result]
@init { $result = new List<JsStatement>(); }
	: (s = statement { $result.Add(s); })*;
	

public expression returns [JsExpression result]
	: ( x=number
	  | 'null' { x = JsExpression.Null; }
	  | x=string
/*	  | x=regexp*/
	  | x=identifier
	  | x=boolean
	  | x=unary
	  | x=binary
	  | x=this
	  | x=arrayLiteral
	  | x=objectLiteral
	  | x=member
	  | x=comma
	  | x=conditional
	  | x=call
	  | x=createObject
	  | x=functionDefinitionExpression
	  ) { $result = x; };

public statement returns [JsStatement result]
	: ( s=functionDeclaration
	  | s=blockStatement
	  | s=variableStatement
	  | s=emptyStatement
	  | x=expression { s = new JsExpressionStatement(x); }
	  | s=ifStatement
	  | s=iterationStatement
	  | s=continueStatement
	  | s=breakStatement
	  | s=returnStatement
	  | s=withStatement
	  | s=labelledStatement
	  | s=switchStatement
	  | s=throwStatement
	  | s=tryStatement
	  | s=gotoStatement
	  | s=yieldStatement
	  ) { $result = s; };

number returns [JsExpression result]
	: NumericLiteral {
	      double value;
	      if ($NumericLiteral.Text.StartsWith("0x")) {
	          value = int.Parse($NumericLiteral.Text.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
	      }
	      else {
	          value = double.Parse($NumericLiteral.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
	      }
	      $result = JsExpression.Number(value);
	};

identifier returns [JsExpression result]
	: Identifier { result = JsExpression.Identifier($Identifier.Text); };

string returns [JsExpression result]
	: StringLiteral { result = JsExpression.String(UnescapeStringLiteral($StringLiteral.Text)); };

boolean returns [JsExpression result]
	: ( 'true' { $result = JsExpression.True; }
	  | 'false' { $result = JsExpression.False; }
	  );

this returns [JsExpression result]
	: 'this' { result = JsExpression.This; };

unary returns [JsExpression result]
@init { System.Func<JsExpression, JsExpression> factory = null; }
	: ^(( 'typeof'           { factory = JsExpression.TypeOf; }
	    | '!'                { factory = JsExpression.LogicalNot; }
	    | UNARY_MINUS        { factory = JsExpression.Negate; }
	    | UNARY_PLUS         { factory = JsExpression.Positive; }
	    | POSTFIX_PLUSPLUS   { factory = JsExpression.PostfixPlusPlus; }
	    | POSTFIX_MINUSMINUS { factory = JsExpression.PostfixMinusMinus; }
	    | '++'               { factory = JsExpression.PrefixPlusPlus; }
	    | '--'               { factory = JsExpression.PrefixMinusMinus; }
	    | 'delete'           { factory = JsExpression.Delete; }
	    | 'void'             { factory = JsExpression.Void; }
	    | '~'                { factory = JsExpression.BitwiseNot; }
	    ) x=expression) { result = factory(x); };

binary returns [JsExpression result]
@init { System.Func<JsExpression, JsExpression, JsExpression> factory = null; }
	: ^(( '&&'         { factory = JsExpression.LogicalAnd; }
	    | '||'         { factory = JsExpression.LogicalOr; }
	    | '!='         { factory = JsExpression.NotEqual; }
	    | '<='         { factory = JsExpression.LesserOrEqual; }
	    | '>='         { factory = JsExpression.GreaterOrEqual; }
	    | '<'          { factory = JsExpression.Lesser; }
	    | '>'          { factory = JsExpression.Greater; }
	    | '=='         { factory = JsExpression.Equal; }
	    | '-'          { factory = JsExpression.Subtract; }
	    | '+'          { factory = JsExpression.Add; }
	    | '%'          { factory = JsExpression.Modulo; }
	    | '/'          { factory = JsExpression.Divide; }
	    | '*'          { factory = JsExpression.Multiply; }
	    | '&'          { factory = JsExpression.BitwiseAnd; }
	    | '|'          { factory = JsExpression.BitwiseOr; }
	    | '^'          { factory = JsExpression.BitwiseXor; }
	    | '==='        { factory = JsExpression.Same; }
	    | '!=='        { factory = JsExpression.NotSame; }
	    | '<<'         { factory = JsExpression.LeftShift; }
	    | '>>'         { factory = JsExpression.RightShiftSigned; }
	    | '>>>'        { factory = JsExpression.RightShiftUnsigned; }
	    | 'instanceof' { factory = JsExpression.InstanceOf; }
	    | 'in'         { factory = JsExpression.In; }
	    | '='          { factory = JsExpression.Assign; }
	    | '*='         { factory = JsExpression.MultiplyAssign; }
	    | '/='         { factory = JsExpression.DivideAssign; }
	    | '%='         { factory = JsExpression.ModuloAssign; }
	    | '+='         { factory = JsExpression.AddAssign; }
	    | '-='         { factory = JsExpression.SubtractAssign; }
	    | '<<='        { factory = JsExpression.LeftShiftAssign; }
	    | '>>='        { factory = JsExpression.RightShiftSignedAssign; }
	    | '>>>='       { factory = JsExpression.RightShiftUnsignedAssign; }
	    | '&='         { factory = JsExpression.BitwiseAndAssign; }
	    | '|='         { factory = JsExpression.BitwiseOrAssign; }
	    | '^='         { factory = JsExpression.BitwiseXOrAssign; }
	   ) a=expression { $result = a; } (b=expression { $result = factory($result, b); })+);

objectLiteral returns [JsExpression result]
@init { var properties = new List<JsObjectLiteralProperty>(); }
	: ^(OBJECT_LITERAL (p=objectLiteralProperty { properties.Add(p); })*) { $result = JsExpression.ObjectLiteral(properties); };

objectLiteralProperty returns [JsObjectLiteralProperty result]
@init { string name = null; }
	: (( Identifier { name = $Identifier.Text; }
	   | StringLiteral { name = UnescapeStringLiteral($StringLiteral.Text); }
	   | NumericLiteral { name = $NumericLiteral.Text; }
	  ) a=expression) { $result = new JsObjectLiteralProperty(name, a); };

arrayLiteral returns [JsExpression result]
@init { var items = new List<JsExpression>(); }
	: ^(ARRAY_LITERAL (p=expression { items.Add(p); })*) { $result = JsExpression.ArrayLiteral(items); };

member returns [JsExpression result]
@init { var args = new List<JsExpression>(); }
	: ^(MEMBER t=expression { $result = t; } ( ^('[' i=expression) { $result = JsExpression.Index($result, i); }
	                                         | m=Identifier { $result = JsExpression.MemberAccess($result, m.Text); }
											 | ^(CALL { args.Clear(); } (x=expression { args.Add(x); })*) { $result = JsExpression.Invocation($result, args); }
	                                         )+);

comma returns [JsExpression result]
@init { var l = new List<JsExpression>(); }
	: ^(',' (x=expression { l.Add(x); })+) { $result = JsExpression.Comma(l); };

conditional returns [JsExpression result]
@init { var l = new List<JsExpression>(); }
	: ^('?' a=expression b=expression c=expression) { $result = JsExpression.Conditional(a, b, c); };

call returns [JsExpression result]
@init { var p = new List<JsExpression>(); }
	: ^(CALL a=expression (b=expression { p.Add(b); })*) { $result = JsExpression.Invocation(a, p); };

createObject returns [JsExpression result]
@init { var args = new List<JsExpression>(); }
	: ^('new' a=expression (b=expression { args.Add(b); })*) { $result = JsExpression.New(a, args); };

functionDefinitionExpression returns [JsExpression result]
@init { var parms = new List<string>(); string name = null; }
	: ^('function' (n=Identifier { name = n.Text; })? ^(ARGS (p=Identifier { parms.Add(p.Text); })*) b=blockStatement) { $result = JsExpression.FunctionDefinition(parms, JsBlockStatement.MakeBlock(b), name); };

functionDeclaration returns [JsStatement result]
@init { var parms = new List<string>(); string name = null; }
	: ^(FUNCTION_DECLARATION (n=Identifier { name = n.Text; }) ^(ARGS (p=Identifier { parms.Add(p.Text); })*) b=blockStatement) { $result = new JsFunctionStatement(name, parms, JsBlockStatement.MakeBlock(b)); };

blockStatement returns [JsStatement result]
@init { var stmts = new List<JsStatement>(); }
	: ^('{' (s=statement { stmts.Add(s); })*) { $result = new JsBlockStatement(stmts); };

emptyStatement returns [JsStatement result]
	: ';' { $result = new JsEmptyStatement(); };

labelledStatement returns [JsStatement result]
	: ^(':' i=Identifier s=statement) { $result = new JsLabelledStatement(i.Text, s); };

variableStatement returns [JsStatement result]
@init { var vars = new List<JsVariableDeclaration>(); }
	: ^('var' (d=variableDeclaration { vars.Add(d); })+) { $result = new JsVariableDeclarationStatement(vars); };

variableDeclaration returns [JsVariableDeclaration result]
	: ^('=' n=Identifier x=expression) { $result = new JsVariableDeclaration(n.Text, x); }
	| (n=Identifier) { $result = new JsVariableDeclaration(n.Text, null); };

ifStatement returns [JsStatement result]
	: ^('if' a=expression b=statement c=statement?) { $result = new JsIfStatement(a, b, c); };

iterationStatement returns [JsStatement result]
	: ( x = doWhileStatement
	  | x = whileStatement
	  | x = forStatement
	  | x = forInStatement
	  ) { $result = x; };

doWhileStatement returns [JsStatement result]
	: ^('do' a=statement b=expression) { $result = new JsDoWhileStatement(b, a); };

whileStatement returns [JsStatement result]
	: ^('while' a=expression b=statement) { $result = new JsWhileStatement(a, b); };

forStatement returns [JsStatement result]
@init { JsStatement init = new JsEmptyStatement(); }
	: ^('for' ^(INIT (a=variableStatement { init = a; } | b=expression { init = new JsExpressionStatement(b); })?) ^(TEST c=expression?) ^(INCR d=expression?) e=statement) { $result = new JsForStatement(init, c, d, e); };

forInStatement returns [JsStatement result]
@init { bool declare = false; }
	: ^(FOR_IN ('var' { declare = true; })? a=Identifier b=expression c=statement) { $result = new JsForEachInStatement(a.Text, b, c, declare); };

continueStatement returns [JsStatement result]
	: ^('continue' i=Identifier?) { $result = new JsContinueStatement(i != null ? i.Text : null); };

breakStatement returns [JsStatement result]
	: ^('break' i=Identifier?) { $result = new JsBreakStatement(i != null ? i.Text : null); };

returnStatement returns [JsStatement result]
	: ^('return' x=expression?) { $result = new JsReturnStatement(x); };

withStatement returns [JsStatement result]
	: ^('with' x=expression s=statement) { $result = new JsWithStatement(x, s); };

tryStatement returns [JsStatement result]
@init { JsCatchClause catchClause = null; }
	: ^('try' a=statement (^('catch' b=Identifier c=statement { catchClause = new JsCatchClause(b.Text, c); }))? (^('finally' d=statement))?) { $result = new JsTryStatement(a, catchClause, d); };

throwStatement returns [JsStatement result]
	: ^('throw' a=expression) { $result = new JsThrowStatement(a); };

switchStatement returns [JsStatement result]
@init { var sections = new List<JsSwitchSection>(); }
	: ^('switch' a=expression (s=switchSection { sections.Add(s); })*) { $result = new JsSwitchStatement(a, sections); };

switchSection returns [JsSwitchSection result]
@init { var values = new List<JsExpression>(); }
	: (^('case' a=expression { values.Add(a); }) | 'default' { values.Add(null); })+ s=blockStatement? { $result = new JsSwitchSection(values, s ?? new JsEmptyStatement()); };

gotoStatement returns [JsStatement result]
	: ^('goto' l=Identifier) { $result = new JsGotoStatement(l.Text); };

yieldStatement returns [JsStatement result]
	: ^('yield' a=expression?) { $result = new JsYieldStatement(a); };
