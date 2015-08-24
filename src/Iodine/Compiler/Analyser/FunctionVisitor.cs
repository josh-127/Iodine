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
using System.Collections.Generic;
using Iodine.Compiler.Ast;

namespace Iodine.Compiler
{
	public class FunctionVisitor : IAstVisitor
	{
		private ErrorLog errorLog;
		private SymbolTable symbolTable;

		public FunctionVisitor (ErrorLog errorLog, SymbolTable symbolTable)
		{
			this.errorLog = errorLog;
			this.symbolTable = symbolTable;
		}

		public void Accept (UseStatement useStmt)
		{
			errorLog.AddError (ErrorType.ParserError, useStmt.Location,
				"use statement not valid inside function body!");
		}

		public void Accept (BinaryExpression binop)
		{
			if (binop.Operation == BinaryOperation.Assign) {
				if (binop.Left is NameExpression) {
					NameExpression ident = (NameExpression)binop.Left;
					if (!this.symbolTable.IsSymbolDefined (ident.Value)) {
						this.symbolTable.AddSymbol (ident.Value);
					}
				}
			}
			this.visitSubnodes (binop);
		}

		public void Accept (InterfaceDeclaration interfaceDecl)
		{
			symbolTable.AddSymbol (interfaceDecl.Name);
		}

		public void Accept (ClassDeclaration classDecl)
		{
			symbolTable.AddSymbol (classDecl.Name);
			RootVisitor visitor = new RootVisitor (errorLog, symbolTable);
			classDecl.Visit (visitor);
		}

		public void Accept (EnumDeclaration enumDecl)
		{
			symbolTable.AddSymbol (enumDecl.Name);
		}

		public void Accept (FunctionDeclaration funcDecl)
		{
			symbolTable.AddSymbol (funcDecl.Name);
			FunctionVisitor visitor = new FunctionVisitor (errorLog, symbolTable);
			symbolTable.BeginScope ();

			foreach (string param in funcDecl.Parameters) {
				symbolTable.AddSymbol (param);
			}

			funcDecl.Children [0].Visit (visitor);
			symbolTable.EndScope ();
		}

		public void Accept (ForeachStatement foreachStmt)
		{
			symbolTable.AddSymbol (foreachStmt.Item);
			foreachStmt.Iterator.Visit (this);
			foreachStmt.Body.Visit (this);
		}

		public void Accept (LambdaExpression lambda)
		{
			symbolTable.BeginScope ();
			foreach (string param in lambda.Parameters) {
				symbolTable.AddSymbol (param);
			}
			FunctionVisitor visitor = new FunctionVisitor (errorLog, symbolTable);
			lambda.Children [0].Visit (visitor);
			symbolTable.EndScope ();
		}

		public void Accept (CodeBlock scope)
		{
			symbolTable.BeginScope ();
			visitSubnodes (scope);
			symbolTable.EndScope ();
		}

		public void Accept (TryExceptStatement tryExcept)
		{
			tryExcept.TryBody.Visit (this);
			if (tryExcept.ExceptionIdentifier != null) {
				symbolTable.AddSymbol (tryExcept.ExceptionIdentifier);
			}
			tryExcept.ExceptBody.Visit (this);
		}

		public void Accept (AstNode ast)
		{
			visitSubnodes (ast);
		}

		public void Accept (AstRoot ast)
		{
			visitSubnodes (ast);
		}

		public void Accept (Expression expr)
		{
			visitSubnodes (expr);
		}

		public void Accept (RaiseStatement raise)
		{
			visitSubnodes (raise);
		}

		public void Accept (SuperCallExpression super)
		{
			visitSubnodes (super);
		}

		public void Accept (ReturnStatement returnStmt)
		{
			visitSubnodes (returnStmt);
		}

		public void Accept (YieldStatement yieldStmt)
		{
			visitSubnodes (yieldStmt);
		}

		public void Accept (ListExpression list)
		{
			visitSubnodes (list);
		}

		public void Accept (HashExpression hash)
		{
			visitSubnodes (hash);
		}

		public void Accept (IndexerExpression indexer)
		{
			visitSubnodes (indexer);
		}

		public void Accept (TupleExpression tuple)
		{
			visitSubnodes (tuple);
		}

		public void Accept (UnaryExpression unaryop)
		{
			visitSubnodes (unaryop);
		}

		public void Accept (CallExpression call)
		{
			visitSubnodes (call);
		}

		public void Accept (ArgumentList arglist)
		{
			visitSubnodes (arglist);
		}

		public void Accept (KeywordArgumentList kwargs)
		{
			visitSubnodes (kwargs);
		}

		public void Accept (GetExpression getAttr)
		{
			visitSubnodes (getAttr);
		}

		public void Accept (IfStatement ifStmt)
		{
			visitSubnodes (ifStmt);
		}

		public void Accept (SwitchStatement switchStmt)
		{
			visitSubnodes (switchStmt);
		}

		public void Accept (CaseStatement caseStmt)
		{
			visitSubnodes (caseStmt);
		}

		public void Accept (WhileStatement whileStmt)
		{
			visitSubnodes (whileStmt);
		}

		public void Accept (DoStatement doStmt)
		{
			visitSubnodes (doStmt);
		}

		public void Accept (ForStatement forStmt)
		{
			visitSubnodes (forStmt);
		}

		public void Accept (Statement stmt)
		{
		}

		public void Accept (NameExpression ident)
		{
		}

		public void Accept (IntegerExpression integer)
		{
		}

		public void Accept (FloatExpression num)
		{
		}

		public void Accept (StringExpression str)
		{
		}

		public void Accept (SelfStatement self)
		{
		}

		public void Accept (TrueExpression ntrue)
		{
		}

		public void Accept (FalseExpression nfalse)
		{
		}

		public void Accept (NullExpression nil)
		{
		}

		public void Accept (BreakStatement brk)
		{
		}

		public void Accept (ContinueStatement cont)
		{
		}

		private void visitSubnodes (AstNode root)
		{
			foreach (AstNode node in root) {
				node.Visit (this);
			}
		}
	}
}

