using System;

namespace Iodine
{
	public class AstView : IAstVisitor
	{
		private int ident = 0;

		public void Accept (AstNode ast)
		{
			viewSubnodes (ast);
		}


		public void Accept (Ast ast)
		{
			viewSubnodes (ast);
		}

		public void Accept (NodeExpr expr)
		{
			viewSubnodes (expr);
		}

		public void Accept (NodeStmt stmt)
		{
			viewSubnodes (stmt);
		}

		public void Accept (NodeBinOp binop)
		{
			Write (binop.Operation.ToString ());
			viewSubnodes (binop);
		}

		public void Accept (NodeUnaryOp unaryop)
		{
			Write (unaryop.Operation.ToString ());
			viewSubnodes (unaryop);
		}

		public void Accept (NodeIdent ident)
		{
			Write ("Ident: " + ident.Value);
			viewSubnodes (ident);
		}

		public void Accept (NodeCall call)
		{
			Write ("Function call");
			viewSubnodes (call);
		}

		public void Accept (NodeArgList arglist)
		{
			Write ("Argument List");
			viewSubnodes (arglist);
		}

		public void Accept (NodeGetAttr getAttr)
		{
			Write ("Get attribute");
			viewSubnodes (getAttr);
		}

		public void Accept (NodeInteger integer)
		{
			Write ("{0}", integer.Value);
		}

		public void Accept (NodeIfStmt ifStmt)
		{
			Write ("If Statement");
			ifStmt.Body.Visit (this);
			Write ("Else");
			ifStmt.ElseBody.Visit (this);
		}

		public void Accept (NodeWhileStmt whileStmt)
		{
			Write ("While Statement");
			viewSubnodes (whileStmt);
		}

		public void Accept (NodeForStmt forStmt)
		{
			Write ("For Statement");
			viewSubnodes (forStmt);
		}

		public void Accept (NodeForeach foreachStmt)
		{
			Write ("Foreach {0}", foreachStmt.Item);
			viewSubnodes (foreachStmt);
		}

		public void Accept (NodeFuncDecl funcDecl)
		{
			Write ("Function {0}", funcDecl.Name);
			viewSubnodes (funcDecl);
		}

		public void Accept (NodeScope scope)
		{
			viewSubnodes (scope);
		}

		public void Accept (NodeString str)
		{
			Write ("\"{0}\"", str.Value);
		}

		public void Accept (NodeUseStatement useStmt)
		{
			Write ("Import {0}", useStmt.Module);
		}

		public void Accept (NodeClassDecl classDecl)
		{
			Write ("Class {0} : {1}", classDecl.Name, classDecl.Base == null ? "Object" :
				classDecl.Base);
			viewSubnodes (classDecl);
		}

		public void Accept (NodeReturnStmt returnStmt)
		{
			Write ("Return");
			viewSubnodes (returnStmt);
		}

		public void Accept (NodeIndexer indexer)
		{
			Write ("Indexer");
			viewSubnodes (indexer);
		}

		public void Accept (NodeList list)
		{
			Write ("List");
			viewSubnodes (list);
		}

		public void Accept (NodeSelf self)
		{
			Write ("Self reference");
		}

		public void Accept (NodeTrue ntrue)
		{
			Write ("True");
		}

		public void Accept (NodeFalse nfalse)
		{
			Write ("False");
		}

		public void Accept (NodeNull nil)
		{
			Write ("Null");
		}

		public void Accept (NodeLambda lambda)
		{
			Write ("Lambda");
			viewSubnodes (lambda);
		}

		public void Accept (NodeTryExcept tryExcept)
		{
			Write ("Try");
			viewSubnodes (tryExcept);
		}

		private void viewSubnodes (AstNode root)
		{
			ident++;
			foreach (AstNode node in root) {
				node.Visit (this);
			}
			ident--;
		}

		private void Write (string format, params object[] args)
		{
			for (int i = 0; i < ident; i++) {
				Console.Write ("    ");
			}
			Console.WriteLine(String.Format (format, args));
		}
	}
}

