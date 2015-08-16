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

		public void Accept (NodeUseStatement useStmt)
		{
			errorLog.AddError (ErrorType.ParserError, useStmt.Location,
				"use statement not valid inside function body!");
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
			this.visitSubnodes (binop);
		}

		public void Accept (NodeInterfaceDecl interfaceDecl)
		{
			symbolTable.AddSymbol (interfaceDecl.Name);
		}

		public void Accept (NodeClassDecl classDecl)
		{
			symbolTable.AddSymbol (classDecl.Name);
			RootVisitor visitor = new RootVisitor (errorLog, symbolTable);
			classDecl.Visit (visitor);
		}

		public void Accept (NodeEnumDecl enumDecl)
		{
			symbolTable.AddSymbol (enumDecl.Name);
		}

		public void Accept (NodeFuncDecl funcDecl)
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

		public void Accept (NodeForeach foreachStmt)
		{
			symbolTable.AddSymbol (foreachStmt.Item);
			foreachStmt.Iterator.Visit (this);
			foreachStmt.Body.Visit (this);
		}

		public void Accept (NodeLambda lambda)
		{
			symbolTable.BeginScope ();
			foreach (string param in lambda.Parameters) {
				symbolTable.AddSymbol (param);
			}
			FunctionVisitor visitor = new FunctionVisitor (errorLog, symbolTable);
			lambda.Children [0].Visit (visitor);
			symbolTable.EndScope ();
		}

		public void Accept (NodeScope scope)
		{
			symbolTable.BeginScope ();
			visitSubnodes (scope);
			symbolTable.EndScope ();
		}

		public void Accept (NodeTryExcept tryExcept)
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

		public void Accept (NodeExpr expr)
		{
			visitSubnodes (expr);
		}

		public void Accept (NodeRaiseStmt raise)
		{
			visitSubnodes (raise);
		}

		public void Accept (NodeSuperCall super)
		{
			visitSubnodes (super);
		}

		public void Accept (NodeReturnStmt returnStmt)
		{
			visitSubnodes (returnStmt);
		}

		public void Accept (NodeList list)
		{
			visitSubnodes (list);
		}

		public void Accept (NodeIndexer indexer)
		{
			visitSubnodes (indexer);
		}

		public void Accept (NodeTuple tuple)
		{
			visitSubnodes (tuple);
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

		public void Accept (NodeIfStmt ifStmt)
		{
			visitSubnodes (ifStmt);
		}

		public void Accept (NodeWhileStmt whileStmt)
		{
			visitSubnodes (whileStmt);
		}

		public void Accept (NodeForStmt forStmt)
		{
			visitSubnodes (forStmt);
		}

		public void Accept (NodeStmt stmt)
		{
		}

		public void Accept (NodeIdent ident)
		{
		}

		public void Accept (NodeInteger integer)
		{
		}

		public void Accept (NodeFloat num)
		{
		}

		public void Accept (NodeString str)
		{
		}

		public void Accept (NodeSelf self)
		{
		}

		public void Accept (NodeTrue ntrue)
		{
		}

		public void Accept (NodeFalse nfalse)
		{
		}

		public void Accept (NodeNull nil)
		{
		}

		public void Accept (NodeBreak brk)
		{
		}

		public void Accept (NodeContinue cont)
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

