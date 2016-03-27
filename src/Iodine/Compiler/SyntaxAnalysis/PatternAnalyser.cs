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
using System.Linq;
using System.Collections.Generic;
using Iodine.Compiler;
using Iodine.Compiler.Ast;
using Iodine.Runtime;

namespace Iodine.Compiler
{
	/// <summary>
	/// Performs semantic analysis on trees from pattern matching statements
	/// </summary>
	internal class PatternAnalyzer : IodineAstVisitor
	{
		private ErrorSink errorLog;
		private SymbolTable symbolTable;
		private IodineAstVisitor parentVisitor;

		public PatternAnalyzer (ErrorSink errorLog, SymbolTable symbolTable, IodineAstVisitor parent)
		{
			parentVisitor = parent;
			this.symbolTable = symbolTable;
			this.errorLog = errorLog;
		}

		public override void Accept (BinaryExpression pattern)
		{
			switch (pattern.Operation) {
			case BinaryOperation.Or:
			case BinaryOperation.And:
				pattern.Left.Visit (this);
				pattern.Right.Visit (this);
				break;
			default:
				errorLog.Add (Errors.IllegalPatternExpression, pattern.Location);
				break;
			}
		}

		public override void Accept (NameExpression ident)
		{
			symbolTable.AddSymbol (ident.Value);
		}

		public override void Accept (CallExpression call)
		{
			call.Visit (parentVisitor);
		}

		public override void Accept (ArgumentList arglist)
		{
			arglist.Visit (parentVisitor);
		}

		public override void Accept (KeywordArgumentList kwargs)
		{
			kwargs.Visit (parentVisitor);
		}

		public override void Accept (GetExpression getAttr)
		{
			getAttr.Visit (parentVisitor);
		}

		public override void Accept (UnaryExpression unaryop)
		{
			errorLog.Add (Errors.IllegalPatternExpression, unaryop.Location);
		}

		public override void Accept (StatementList stmtList)
		{
			errorLog.Add (Errors.IllegalPatternExpression, stmtList.Location);
		}

		public override void Accept (IfStatement ifStmt)
		{
			errorLog.Add (Errors.IllegalPatternExpression, ifStmt.Location);
		}

		public override void Accept (WhileStatement whileStmt)
		{
			errorLog.Add (Errors.IllegalPatternExpression, whileStmt.Location);
		}

		public override void Accept (WithStatement withStmt)
		{
			errorLog.Add (Errors.IllegalPatternExpression, withStmt.Location);
		}

		public override void Accept (DoStatement doStmt)
		{
			errorLog.Add (Errors.IllegalPatternExpression, doStmt.Location);
		}

		public override void Accept (ForStatement forStmt)
		{
			errorLog.Add (Errors.IllegalPatternExpression, forStmt.Location);
		}

		public override void Accept (ForeachStatement foreachStmt)
		{
			errorLog.Add (Errors.IllegalPatternExpression, foreachStmt.Location);
		}

		public override void Accept (GivenStatement switchStmt)
		{
			errorLog.Add (Errors.IllegalPatternExpression, switchStmt.Location);
		}

		public override void Accept (WhenStatement caseStmt)
		{
			errorLog.Add (Errors.IllegalPatternExpression, caseStmt.Location);
		}

		public override void Accept (FunctionDeclaration funcDecl)
		{
			errorLog.Add (Errors.IllegalPatternExpression, funcDecl.Location);
		}

		public override void Accept (CodeBlock scope)
		{
			errorLog.Add (Errors.IllegalPatternExpression, scope.Location);
		}

		public override void Accept (UseStatement useStmt)
		{
			errorLog.Add (Errors.IllegalPatternExpression, useStmt.Location);
		}

		public override void Accept (ClassDeclaration classDecl)
		{
			errorLog.Add (Errors.IllegalPatternExpression, classDecl.Location);
		}

		public override void Accept (InterfaceDeclaration contractDecl)
		{
			errorLog.Add (Errors.IllegalPatternExpression, contractDecl.Location);
		}

		public override void Accept (EnumDeclaration enumDecl)
		{
			errorLog.Add (Errors.IllegalPatternExpression, enumDecl.Location);
		}

		public override void Accept (ReturnStatement returnStmt)
		{
			errorLog.Add (Errors.IllegalPatternExpression, returnStmt.Location);
		}

		public override void Accept (YieldStatement yieldStmt)
		{
			errorLog.Add (Errors.IllegalPatternExpression, yieldStmt.Location);
		}

		public override void Accept (TernaryExpression ifExpr)
		{
			errorLog.Add (Errors.IllegalPatternExpression, ifExpr.Location);
		}

		public override void Accept (ListCompExpression list)
		{
			errorLog.Add (Errors.IllegalPatternExpression, list.Location);
		}

		public override void Accept (TryExceptStatement tryExcept)
		{
			errorLog.Add (Errors.IllegalPatternExpression, tryExcept.Location);
		}

		public override void Accept (RaiseStatement raise)
		{
			errorLog.Add (Errors.IllegalPatternExpression, raise.Location);
		}

		public override void Accept (SuperCallExpression super)
		{
			errorLog.Add (Errors.IllegalPatternExpression, super.Location);
		}

		public override void Accept (BreakStatement brk)
		{
			errorLog.Add (Errors.IllegalPatternExpression, brk.Location);
		}

		public override void Accept (ContinueStatement cont)
		{
			errorLog.Add (Errors.IllegalPatternExpression, cont.Location);
		}

		public override void Accept (MatchExpression match)
		{
			errorLog.Add (Errors.IllegalPatternExpression, match.Location);
		}

		public override void Accept (CaseExpression caseExpr)
		{
			errorLog.Add (Errors.IllegalPatternExpression, caseExpr.Location);
		}

		public override void Accept (StringExpression str)
		{
			str.Visit (parentVisitor);
		}
			
		public override void Accept (IndexerExpression indexer)
		{
			indexer.Visit (parentVisitor);
		}

		public override void Accept (ListExpression list)
		{
			list.Visit (this);
		}

		public override void Accept (HashExpression hash)
		{
			hash.Visit (parentVisitor);
		}

		public override void Accept (SelfStatement self)
		{
			self.Visit (parentVisitor);
		}

		public override void Accept (TrueExpression ntrue)
		{
			ntrue.Visit (parentVisitor);
		}

		public override void Accept (FalseExpression nfalse)
		{
			nfalse.Visit (parentVisitor);
		}

		public override void Accept (NullExpression nil)
		{
			nil.Visit (parentVisitor);
		}

		public override void Accept (LambdaExpression lambda)
		{
			lambda.Visit (parentVisitor);
		}

		public override void Accept (TupleExpression tuple)
		{
			tuple.VisitChildren (this);
		}
	}
}

