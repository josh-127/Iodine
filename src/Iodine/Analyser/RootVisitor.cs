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
			this.visitSubnodes (binop);
		}

		public void Accept (NodeUnaryOp unaryop)
		{
			visitSubnodes (unaryop);
		}

		public void Accept (NodeIdent ident)
		{
			visitSubnodes (ident);
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
			visitSubnodes (integer);
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
			FunctionVisitor visitor = new FunctionVisitor (errorLog, symbolTable);
			foreachStmt.Visit (visitor);
		}

		public void Accept (NodeContinue cont)
		{
			visitSubnodes (cont);
		}

		public void Accept (NodeFuncDecl funcDecl)
		{
			symbolTable.AddSymbol (funcDecl.Name);
			FunctionVisitor visitor = new FunctionVisitor (errorLog, symbolTable);
			symbolTable.BeginScope ();

			foreach (string param in funcDecl.Parameters) {
				symbolTable.AddSymbol (param);
			}

			funcDecl.Children[0].Visit (visitor);
			symbolTable.EndScope ();
		}

		public void Accept (NodeScope scope)
		{
			visitSubnodes (scope);
		}

		public void Accept (NodeString str)
		{
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

		public void Accept (NodeFalse nfalse)
		{
		}

		public void Accept (NodeNull nil)
		{
		}

		public void Accept (NodeLambda lambda)
		{
			visitSubnodes (lambda);
		}

		public void Accept (NodeBreak brk)
		{
			visitSubnodes (brk);
		}

		public void Accept (NodeTryExcept tryExcept)
		{
		}

		public void Accept (NodeFloat num) 
		{

		}

		public void Accept (NodeTuple tuple)
		{
			visitSubnodes (tuple);
		}

		public void Accept (NodeConstant constant)
		{
			symbolTable.AddSymbol (constant.Name);
		}

		private void visitSubnodes (AstNode root)
		{
			foreach (AstNode node in root) {
				node.Visit (this);
			}
		}
	}
}

