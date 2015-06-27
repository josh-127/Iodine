using System;

namespace Iodine
{
	public interface IAstVisitor
	{
		void Accept (Ast ast);
		void Accept (NodeExpr expr);
		void Accept (NodeStmt stmt);
		void Accept (NodeBinOp binop);
		void Accept (NodeUnaryOp unaryop);
		void Accept (NodeIdent ident);
		void Accept (NodeCall call);
		void Accept (NodeArgList arglist);
		void Accept (NodeGetAttr getAttr);
		void Accept (NodeInteger integer);
		void Accept (NodeIfStmt ifStmt);
		void Accept (NodeWhileStmt whileStmt);
		void Accept (NodeForStmt forStmt);
		void Accept (NodeForeach foreachStmt);
		void Accept (NodeFuncDecl funcDecl);
		void Accept (NodeScope scope);
		void Accept (NodeString stringConst);
		void Accept (NodeUseStatement useStmt);
		void Accept (NodeClassDecl classDecl);
		void Accept (NodeReturnStmt returnStmt);
		void Accept (NodeIndexer indexer);
		void Accept (NodeList list);
		void Accept (NodeSelf self);
		void Accept (NodeTrue ntrue);
		void Accept (NodeFalse nfalse);
		void Accept (NodeNull nil);
		void Accept (NodeLambda lambda);
		void Accept (NodeTryExcept tryCatch);
	}
}

