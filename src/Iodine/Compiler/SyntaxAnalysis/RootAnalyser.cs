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
	/// <summary>
	/// Performs semantic analysis on the root of an Iodine AST
	/// </summary>
	internal class RootAnalyser : IodineAstVisitor
	{
		private ErrorSink errorLog;
		private SymbolTable symbolTable;

		public RootAnalyser (ErrorSink errorLog, SymbolTable symbolTable)
		{
			this.errorLog = errorLog;
			this.symbolTable = symbolTable;
		}

		public override void Accept (StatementList stmtList)
		{
			stmtList.VisitChildren (this);
		}

		public override void Accept (IfStatement ifStmt)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, ifStmt.Location);
		}

		public override void Accept (WhileStatement whileStmt)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, whileStmt.Location);
		}

		public override void Accept (DoStatement doStmt)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, doStmt.Location);
		}

		public override void Accept (ForStatement forStmt)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, forStmt.Location);
		}

		public override void Accept (ForeachStatement foreachStmt)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, foreachStmt.Location);
		}

		public override void Accept (ContinueStatement cont)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, cont.Location);
		}

		public override void Accept (BreakStatement brk)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, brk.Location);
		}

		public override void Accept (TryExceptStatement tryExcept)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, tryExcept.Location);
		}

		public override void Accept (RaiseStatement raise)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, raise.Location);
		}

		public override void Accept (GivenStatement switchStmt)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, switchStmt.Location);
		}

		public override void Accept (WhenStatement caseStmt)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, caseStmt.Location);
		}

		public override void Accept (WithStatement withStmt)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, withStmt.Location);
		}

		public override void Accept (ReturnStatement returnStmt)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, returnStmt.Location);
		}

		public override void Accept (YieldStatement yieldStmt)
		{
			errorLog.Add (Errors.StatementNotAllowedOutsideFunction, yieldStmt.Location);
		}
			
		public override void Accept (CompilationUnit ast)
		{
			ast.VisitChildren (this);
		}

		public void Accept (AstNode ast)
		{
			ast.VisitChildren (this);
		}

		public override void Accept (ClassDeclaration classDecl)
		{
			classDecl.VisitChildren (this);
		}

		public override void Accept (VariableDeclaration varDecl)
		{
			symbolTable.AddSymbol (varDecl.Name);
			varDecl.VisitChildren (this);
		}

		public override void Accept (FunctionDeclaration funcDecl)
		{
			symbolTable.AddSymbol (funcDecl.Name);
			FunctionAnalyser visitor = new FunctionAnalyser (errorLog, symbolTable);
			symbolTable.BeginScope (true);

			foreach (string param in funcDecl.Parameters) {
				symbolTable.AddSymbol (param);
			}

			funcDecl.VisitChildren (visitor);
			symbolTable.EndScope (true);
		}

		public override void Accept (Expression expr)
		{
			expr.VisitChildren (this);
		}

		public override void Accept (Statement stmt)
		{
			stmt.VisitChildren (this);
		}

		public override void Accept (SuperCallExpression super)
		{
			super.VisitChildren (this);
		}

		public override void Accept (BinaryExpression binop)
		{
			if (binop.Operation == BinaryOperation.Assign) {
				if (binop.Left is NameExpression) {
					NameExpression ident = (NameExpression)binop.Left;
					if (!symbolTable.IsSymbolDefined (ident.Value)) {
						symbolTable.AddSymbol (ident.Value);
					}
				}
			}
			binop.Right.Visit (this);
		}

		public override void Accept (UnaryExpression unaryop)
		{
			unaryop.VisitChildren (this);
		}

		public override void Accept (CallExpression call)
		{
			call.VisitChildren (this);
		}

		public override void Accept (ArgumentList arglist)
		{
			arglist.VisitChildren (this);
		}

		public override void Accept (KeywordArgumentList kwargs)
		{
			kwargs.VisitChildren (this);
		}

		public override void Accept (GetExpression getAttr)
		{
			getAttr.VisitChildren (this);
		}

		public override void Accept (CodeBlock scope)
		{
			scope.VisitChildren (this);
		}

		public override void Accept (IndexerExpression indexer)
		{
			indexer.VisitChildren (this);
		}

		public override void Accept (ListExpression list)
		{
			list.VisitChildren (this);
		}

		public override void Accept (HashExpression hash)
		{
			hash.VisitChildren (this);
		}
		public override void Accept (TupleExpression tuple)
		{
			tuple.VisitChildren (this);
		}

		public override void Accept (LambdaExpression lambda)
		{
			symbolTable.BeginScope (true);
			FunctionAnalyser visitor = new FunctionAnalyser (errorLog, symbolTable);
			foreach (string param in lambda.Parameters) {
				symbolTable.AddSymbol (param);
			}

			lambda.VisitChildren (visitor);
			symbolTable.EndScope (true);
		}

		public override void Accept (MatchExpression match)
		{
			FunctionAnalyser visitor = new FunctionAnalyser (errorLog, symbolTable);
			match.Visit (visitor);
		}

		public override void Accept (ListCompExpression list)
		{
			symbolTable.BeginScope (true);
			symbolTable.AddSymbol (list.Identifier);
			list.VisitChildren (this);
			symbolTable.EndScope (true);
		}
	}
}

