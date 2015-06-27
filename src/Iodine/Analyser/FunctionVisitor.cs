using System;

namespace Iodine
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

		public void Accept (AstNode ast)
		{
			visitSubnodes (ast);
		}

		public void Accept (Ast ast)
		{
			visitSubnodes (ast);
		}

		public void Accept (NodeExpr expr)
		{
			visitSubnodes (expr);
		}

		public void Accept (NodeStmt stmt)
		{
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

		public void Accept (NodeUnaryOp unaryop)
		{
			visitSubnodes (unaryop);
		}

		public void Accept (NodeIdent ident)
		{
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

		public void Accept (NodeInteger integer)
		{
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

		public void Accept (NodeForeach foreachStmt)
		{
			symbolTable.AddSymbol (foreachStmt.Item);
			foreachStmt.Iterator.Visit (this);
			foreachStmt.Body.Visit (this);
		}

		public void Accept (NodeFuncDecl funcDecl)
		{
			errorLog.AddError (ErrorType.ParserError, "Closures not supported at this time!");
		}

		public void Accept (NodeLambda lambda)
		{
			symbolTable.BeginScope ();
			foreach (string param in lambda.Parameters) {
				symbolTable.AddSymbol (param);
			}
			FunctionVisitor visitor = new FunctionVisitor (errorLog, symbolTable);
			lambda.Children[0].Visit (visitor);
			symbolTable.EndScope ();
		}

		public void Accept (NodeScope scope)
		{
			symbolTable.BeginScope ();
			visitSubnodes (scope);
			symbolTable.EndScope ();
		}

		public void Accept (NodeString str)
		{
		}

		public void Accept (NodeUseStatement useStmt)
		{
			errorLog.AddError (ErrorType.ParserError, "use statement not valid inside function body!");
		}

		public void Accept (NodeClassDecl classDecl)
		{
			errorLog.AddError (ErrorType.ParserError, "Can not define a class inside a function!");
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

		public void Accept (NodeTryExcept tryExcept)
		{
			tryExcept.TryBody.Visit (this);
			if (tryExcept.ExceptionIdentifier != null) {
				symbolTable.AddSymbol (tryExcept.ExceptionIdentifier);
			}
			tryExcept.ExceptBody.Visit (this);
		}

		private void visitSubnodes (AstNode root)
		{
			foreach (AstNode node in root) {
				node.Visit (this);
			}
		} 
	}
}

