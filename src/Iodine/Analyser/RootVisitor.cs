using System;

namespace Iodine
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

		public void Accept (Ast ast)
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

		public void Accept (NodeFuncDecl funcDecl)
		{
			symbolTable.AddSymbol (funcDecl.Name);
			FunctionVisitor visitor = new FunctionVisitor (errorLog, symbolTable);
			symbolTable.BeginScope (true);

			foreach (string param in funcDecl.Parameters) {
				symbolTable.AddSymbol (param);
			}

			funcDecl.Children[0].Visit (visitor);
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
			FunctionVisitor visitor = new FunctionVisitor (this.errorLog, this.symbolTable);
			lambda.Visit (visitor);
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

