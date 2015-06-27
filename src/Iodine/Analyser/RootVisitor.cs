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

		public void Accept (AstNode ast)
		{
			this.visitSubnodes (ast);
		}

		public void Accept (Ast ast)
		{
			visitSubnodes (ast);
		}

		public void Accept (NodeExpr expr)
		{
		}

		public void Accept (NodeStmt stmt)
		{
		}

		public void Accept (NodeBinOp binop)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeUnaryOp unaryop)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeIdent ident)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeCall call)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeArgList arglist)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeGetAttr getAttr)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeInteger integer)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeIfStmt ifStmt)
		{
			errorLog.AddError (ErrorType.ParserError, "Unexpected if statement!");
		}

		public void Accept (NodeWhileStmt whileStmt)
		{
			errorLog.AddError (ErrorType.ParserError, "Unexpected while statement!");
		}

		public void Accept (NodeForStmt forStmt)
		{
			errorLog.AddError (ErrorType.ParserError, "Unexpected for statement!");
		}

		public void Accept (NodeForeach foreachStmt)
		{
			errorLog.AddError (ErrorType.ParserError, "Unexpected foreach statement!");
		}

		public void Accept (NodeFuncDecl funcDecl)
		{
			symbolTable.AddSymbol (funcDecl.Name);
			FunctionVisitor visitor = new FunctionVisitor (errorLog, symbolTable);
			symbolTable.BeginScope ();

			foreach (string param in funcDecl.Parameters) {
				symbolTable.AddSymbol (param);
			}

			visitor.Accept (funcDecl.Children[0]);
			symbolTable.EndScope ();
		}

		public void Accept (NodeScope scope)
		{
			visitSubnodes (scope);
		}

		public void Accept (NodeString str)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeUseStatement useStmt)
		{
		}

		public void Accept (NodeClassDecl classDecl)
		{
			visitSubnodes (classDecl);
		}

		public void Accept (NodeReturnStmt returnStmt)
		{
			errorLog.AddError (ErrorType.ParserError, "Unexpected return statement!");
		}

		public void Accept (NodeIndexer indexer)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeList list)
		{
			errorLog.AddError (ErrorType.ParserError, "Can not define list outside function!");
		}

		public void Accept (NodeSelf self)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeTrue ntrue)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeFalse nfalse)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeNull nil)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeLambda lambda)
		{
			errorLog.AddError (ErrorType.ParserError, "Expression not valid outside function body!");
		}

		public void Accept (NodeTryExcept tryExcept)
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

