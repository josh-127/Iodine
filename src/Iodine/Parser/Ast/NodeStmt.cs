using System;

namespace Iodine
{
	public class NodeStmt : AstNode
	{
		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream) 
		{
			if (stream.Match (TokenClass.Keyword, "class")) {
				return NodeClassDecl.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "func")) {
				return NodeFuncDecl.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "if")) {
				return NodeIfStmt.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "for")) {
				return NodeForStmt.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "foreach")) {
				return NodeForeach.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "while")) {
				return NodeWhileStmt.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "use")) {
				return NodeUseStatement.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "return")) {
				return NodeReturnStmt.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "try")) {
				return NodeTryExcept.Parse (stream);
			} else if (stream.Accept (TokenClass.Keyword, "break")) {
				return new NodeBreak ();
			} else if (stream.Match (TokenClass.OpenBrace)) {
				return NodeScope.Parse (stream);
			} else if (stream.Accept (TokenClass.SemiColon)) {
				return new NodeStmt ();
			} else if (stream.Match (TokenClass.Identifier, TokenClass.Colon)) {
				return NodeConstant.Parse (stream);
			} else {
				AstNode node = NodeExpr.Parse (stream);
				if (node == null) {
					//stream.MakeError ();
				}
				return new NodeExpr (node);
			}
		}
	}
}

