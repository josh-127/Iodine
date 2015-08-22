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
	public class RootVisitor : IAstVisitor
	{
		private ErrorLog errorLog;
		private SymbolTable symbolTable;

		public RootVisitor (ErrorLog errorLog, SymbolTable symbolTable)
		{
			this.errorLog = errorLog;
			this.symbolTable = symbolTable;
		}

		public void Accept (NodeIfStmt ifStmt)
		{
			errorLog.AddError (ErrorType.ParserError, ifStmt.Location, 
				"Statement not allowed outside function body!");
		}

		public void Accept (NodeWhileStmt whileStmt)
		{
			errorLog.AddError (ErrorType.ParserError, whileStmt.Location,
				"Statement not allowed outside function body!");
		}

		public void Accept (NodeDoStmt doStmt)
		{
			errorLog.AddError (ErrorType.ParserError, doStmt.Location,
				"Statement not allowed outside function body!");
		}

		public void Accept (NodeForStmt forStmt)
		{
			errorLog.AddError (ErrorType.ParserError, forStmt.Location,
				"Statement not allowed outside function body!");
		}

		public void Accept (NodeForeach foreachStmt)
		{
			errorLog.AddError (ErrorType.ParserError, foreachStmt.Location,
				"Statement not allowed outside function body!");
		}

		public void Accept (NodeContinue cont)
		{
			errorLog.AddError (ErrorType.ParserError, cont.Location,
				"Statement not allowed outside function body!");
		}

		public void Accept (NodeBreak brk)
		{
			errorLog.AddError (ErrorType.ParserError, brk.Location,
				"Statement not allowed outside function body!");
		}

		public void Accept (NodeTryExcept tryExcept)
		{
			errorLog.AddError (ErrorType.ParserError, tryExcept.Location,
				"Statement not allowed outside function body!");
		}

		public void Accept (NodeRaiseStmt raise)
		{
			errorLog.AddError (ErrorType.ParserError, raise.Location,
				"Statement not allowed outside function body!");
		}

		public void Accept (NodeSwitchStmt switchStmt)
		{
			errorLog.AddError (ErrorType.ParserError, switchStmt.Location,
				"Statement not allowed outside function body!");
		}

		public void Accept (NodeCaseStmt caseStmt)
		{
			errorLog.AddError (ErrorType.ParserError, caseStmt.Location,
				"Statement not allowed outside function body!");
		}

		public void Accept (AstRoot ast)
		{
			visitSubnodes (ast);
		}

		public void Accept (AstNode ast)
		{
			visitSubnodes (ast);
		}

		public void Accept (NodeClassDecl classDecl)
		{
			visitSubnodes (classDecl);
		}

		public void Accept (NodeInterfaceDecl interfaceDecl)
		{
		}

		public void Accept (NodeFuncDecl funcDecl)
		{
			symbolTable.AddSymbol (funcDecl.Name);
			FunctionVisitor visitor = new FunctionVisitor (errorLog, symbolTable);
			symbolTable.BeginScope (true);

			foreach (string param in funcDecl.Parameters) {
				symbolTable.AddSymbol (param);
			}

			funcDecl.Children [0].Visit (visitor);
			symbolTable.EndScope (true);
		}

		public void Accept (NodeExpr expr)
		{
			visitSubnodes (expr);
		}

		public void Accept (NodeStmt stmt)
		{
			visitSubnodes (stmt);
		}

		public void Accept (NodeSuperCall super)
		{
			visitSubnodes (super);
		}

		public void Accept (NodeBinOp binop)
		{
			if (binop.Operation == BinaryOperation.Assign) {
				if (binop.Left is NodeIdent) {
					NodeIdent ident = (NodeIdent)binop.Left;
					if (!this.symbolTable.IsSymbolDefined (ident.Value)) {
						this.symbolTable.AddSymbol (ident.Value);
					}
				}
			}
			binop.Right.Visit (this);
		}

		public void Accept (NodeUnaryOp unaryop)
		{
			visitSubnodes (unaryop);
		}

		public void Accept (NodeCall call)
		{
			visitSubnodes (call);
		}

		public void Accept (NodeArgList arglist)
		{
			visitSubnodes (arglist);
		}

		public void Accept (NodeGetAttr getAttr)
		{
			visitSubnodes (getAttr);
		}

		public void Accept (NodeScope scope)
		{
			visitSubnodes (scope);
		}

		public void Accept (NodeReturnStmt returnStmt)
		{
			visitSubnodes (returnStmt);
		}

		public void Accept (NodeYieldStmt yieldStmt)
		{
			visitSubnodes (yieldStmt);
		}

		public void Accept (NodeIndexer indexer)
		{
			visitSubnodes (indexer);
		}

		public void Accept (NodeList list)
		{
			visitSubnodes (list);
		}

		public void Accept (NodeSelf self)
		{
			visitSubnodes (self);
		}

		public void Accept (NodeTrue ntrue)
		{
			visitSubnodes (ntrue);
		}

		public void Accept (NodeTuple tuple)
		{
			visitSubnodes (tuple);
		}

		public void Accept (NodeLambda lambda)
		{
			symbolTable.BeginScope (true);

			FunctionVisitor visitor = new FunctionVisitor (errorLog, symbolTable);
			foreach (string param in lambda.Parameters) {
				symbolTable.AddSymbol (param);
			}

			lambda.Children [0].Visit (visitor);
			symbolTable.EndScope (true);
		}

		public void Accept (NodeEnumDecl enumDecl)
		{
		}

		public void Accept (NodeIdent ident)
		{
		}

		public void Accept (NodeString str)
		{
		}

		public void Accept (NodeUseStatement useStmt)
		{
		}

		public void Accept (NodeFalse nfalse)
		{
		}

		public void Accept (NodeNull nil)
		{
		}

		public void Accept (NodeInteger integer)
		{
		}

		public void Accept (NodeFloat num)
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

