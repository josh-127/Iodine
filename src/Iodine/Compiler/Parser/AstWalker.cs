/**
  * Copyright (c) 2015, GruntTheDivine All rights reserved.

  * Redistribution and use in source and binary forms, with or without modification,
  * are permitted provided that the following conditions are met:
  * 
  *  * Redistributions of source code must retain the above copyright notice, this list
  *    of conditions and the following disclaimer.
  * 
  *  * Redistributions in binary form must reproduce the above copyright notice, this
  *    list of conditions and the following disclaimer in the documentation and/or
  *    other materials provided with the distribution.

  * Neither the name of the copyright holder nor the names of its contributors may be
  * used to endorse or promote products derived from this software without specific
  * prior written permission.
  * 
  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
  * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
  * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
  * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
  * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
  * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
  * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
  * CONTRACT ,STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
  * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
  * DAMAGE.
**/

using System;
using Iodine.Compiler.Ast;

namespace Iodine.Compiler
{
	delegate void WalkCallback (AstNode node);
	class AstWalker : IodineAstVisitor
	{

		private WalkCallback callback;

		public AstWalker (WalkCallback callback)
		{
			this.callback = callback;
		}

		public override void Accept (CompilationUnit ast)
		{
			callback (ast);
			ast.VisitChildren (this);
		}

		public override void Accept (Expression expr)
		{
			callback (expr);
			expr.VisitChildren (this);
		}

		public override void Accept (StatementList stmtList)
		{
			callback (stmtList);
			stmtList.VisitChildren (this);
		}

		public override void Accept (Statement stmt)
		{
			callback (stmt);
			stmt.VisitChildren (this);
		}

		public override void Accept (BinaryExpression binop)
		{
			callback (binop);
			binop.VisitChildren (this);
		}

		public override void Accept (UnaryExpression unaryop)
		{
			callback (unaryop);
			unaryop.VisitChildren (this);
		}

		public override void Accept (NameExpression ident)
		{
			callback (ident);
			ident.VisitChildren (this);
		}

		public override void Accept (CallExpression call)
		{
			callback (call);
			call.VisitChildren (this);
		}

		public override void Accept (ArgumentList arglist)
		{
			callback (arglist);
			arglist.VisitChildren (this);
		}

		public override void Accept (KeywordArgumentList kwargs)
		{
			callback (kwargs);
			kwargs.VisitChildren (this);
		}

		public override void Accept (GetExpression getAttr)
		{
			callback (getAttr);
			getAttr.VisitChildren (this);
		}

		public override void Accept (GetDefaultExpression getAttr)
		{
			callback (getAttr);
			getAttr.VisitChildren (this);
		}

		public override void Accept (IntegerExpression integer)
		{
			callback (integer);
			integer.VisitChildren (this);
		}

		public override void Accept (IfStatement ifStmt)
		{
			callback (ifStmt);
			ifStmt.VisitChildren (this);
		}

		public override void Accept (WhileStatement whileStmt)
		{
			callback (whileStmt);
			whileStmt.VisitChildren (this);
		}

		public override void Accept (DoStatement doStmt)
		{
			callback (doStmt);
			doStmt.VisitChildren (this);
		}

		public override void Accept (ForStatement forStmt)
		{
			callback (forStmt);
			forStmt.VisitChildren (this);
		}

		public override void Accept (ForeachStatement foreachStmt)
		{
			callback (foreachStmt);
			foreachStmt.VisitChildren (this);
		}

		public override void Accept (GivenStatement switchStmt)
		{
			callback (switchStmt);
			switchStmt.VisitChildren (this);
		}

		public override void Accept (WhenStatement caseStmt)
		{
			callback (caseStmt);
			caseStmt.VisitChildren (this);
		}

		public override void Accept (FunctionDeclaration funcDecl)
		{
			callback (funcDecl);
			funcDecl.VisitChildren (this);
		}

		public override void Accept (CodeBlock scope)
		{
			callback (scope);
			scope.VisitChildren (this);
		}

		public override void Accept (StringExpression stringConst)
		{
			callback (stringConst);
			stringConst.VisitChildren (this);
		}

		public override void Accept (UseStatement useStmt)
		{
			callback (useStmt);
			useStmt.VisitChildren (this);
		}

		public override void Accept (InterfaceDeclaration interfaceDecl)
		{
			callback (interfaceDecl);
			interfaceDecl.VisitChildren (this);
		}

		public override void Accept (ClassDeclaration classDecl)
		{
			callback (classDecl);
			classDecl.VisitChildren (this);
		}

		public override void Accept (ReturnStatement returnStmt)
		{
			callback (returnStmt);
			returnStmt.VisitChildren (this);
		}

		public override void Accept (YieldStatement yieldStmt)
		{
			callback (yieldStmt);
			yieldStmt.VisitChildren (this);
		}

		public override void Accept (IndexerExpression indexer)
		{
			callback (indexer);
			indexer.VisitChildren (this);
		}

		public override void Accept (ListExpression list)
		{
			callback (list);
			list.VisitChildren (this);
		}

		public override void Accept (HashExpression hash)
		{
			callback (hash);
			hash.VisitChildren (this);
		}

		public override void Accept (SelfStatement self)
		{
			callback (self);
			self.VisitChildren (this);
		}

		public override void Accept (TrueExpression ntrue)
		{
			callback (ntrue);
			ntrue.VisitChildren (this);
		}

		public override void Accept (FalseExpression nfalse)
		{
			callback (nfalse);
			nfalse.VisitChildren (this);
		}

		public override void Accept (NullExpression nil)
		{
			callback (nil);
			nil.VisitChildren (this);
		}

		public override void Accept (LambdaExpression lambda)
		{
			callback (lambda);
			lambda.VisitChildren (this);
		}

		public override void Accept (TryExceptStatement tryCatch)
		{
			callback (tryCatch);
			tryCatch.VisitChildren (this);
		}

		public override void Accept (WithStatement with)
		{
			callback (with);
			with.VisitChildren (this);
		}

		public override void Accept (BreakStatement brk)
		{
			callback (brk);
			brk.VisitChildren (this);
		}

		public override void Accept (ContinueStatement cont)
		{
			callback (cont);
			cont.VisitChildren (this);
		}

		public override void Accept (TupleExpression tuple)
		{
			callback (tuple);
			tuple.VisitChildren (this);
		}

		public override void Accept (FloatExpression dec)
		{
			callback (dec);
			dec.VisitChildren (this);
		}

		public override void Accept (SuperCallExpression super)
		{
			callback (super);
			super.VisitChildren (this);
		}

		public override void Accept (EnumDeclaration enumDecl)
		{
			callback (enumDecl);
			enumDecl.VisitChildren (this);
		}

		public override void Accept (VariableDeclaration varDecl)
		{
			callback (varDecl);
			varDecl.VisitChildren (this);
		}

		public override void Accept (RaiseStatement raise)
		{
			callback (raise);
			raise.VisitChildren (this);
		}

		public override void Accept (MatchExpression match)
		{
			callback (match);
			match.VisitChildren (this);
		}

		public override void Accept (CaseExpression caseExpr)
		{
			callback (caseExpr);
			caseExpr.VisitChildren (this);
		}

		public override void Accept (ListCompExpression list)
		{
			callback (list);
			list.VisitChildren (this);
		}

		public override void Accept (TernaryExpression ifExpr)
		{
			callback (ifExpr);
			ifExpr.VisitChildren (this);
		}

	}
}

