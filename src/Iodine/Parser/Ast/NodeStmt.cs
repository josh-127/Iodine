using System;
using Iodine.Compiler;

namespace Iodine.Compiler.Ast
{
	public class NodeStmt : AstNode
	{
		public NodeStmt (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream) 
		{
			if (stream.Match (TokenClass.Keyword, "class")) {
				return NodeClassDecl.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "enum")) {
				return NodeEnumDecl.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "interface")) {
				return NodeInterfaceDecl.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "func") ||
				stream.Match (TokenClass.Operator, "@")) {
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
			} else if (stream.Match (TokenClass.Keyword, "raise")) {
				return NodeRaiseStmt.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "try")) {
				return NodeTryExcept.Parse (stream);
			} else if (stream.Accept (TokenClass.Keyword, "break")) {
				return new NodeBreak (stream.Location);
			} else if (stream.Accept (TokenClass.Keyword, "continue")) {
				return new NodeContinue (stream.Location);
			} else if (stream.Accept (TokenClass.Keyword, "super")) {
				stream.ErrorLog.AddError (ErrorType.ParserError, stream.Location,
					"super () constructor must be called first!");
				return NodeSuperCall.Parse (stream, new NodeClassDecl (stream.Location, "", null));
			} else if (stream.Match (TokenClass.OpenBrace)) {
				return NodeScope.Parse (stream);
			} else if (stream.Accept (TokenClass.SemiColon)) {
				return new NodeStmt (stream.Location);
			} else {
				AstNode node = NodeExpr.Parse (stream);
				if (node == null) {
					stream.MakeError ();
				}
				return new NodeExpr (stream.Location, node);
			}
		}
	}
}

