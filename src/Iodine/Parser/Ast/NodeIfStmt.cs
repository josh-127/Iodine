using System;

namespace Iodine
{
	public class NodeIfStmt : AstNode
	{
		public AstNode Condition {
			get {
				return this.Children[0];
			}
		}

		public AstNode Body {
			get {
				return this.Children[1];
			}
		}

		public AstNode ElseBody {
			get {
				return this.Children[2];
			}
		}

		public NodeIfStmt (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			NodeIfStmt ifStmt = new NodeIfStmt (stream.Location);
			stream.Expect (TokenClass.Keyword, "if");
			stream.Expect (TokenClass.OpenParan);
			ifStmt.Add (NodeExpr.Parse (stream));
			stream.Expect (TokenClass.CloseParan);
			ifStmt.Add (NodeStmt.Parse (stream));
			if (stream.Accept (TokenClass.Keyword, "else")) {
				ifStmt.Add (NodeStmt.Parse (stream));
			} else {
				ifStmt.Add (new Ast (stream.Location));
			}
			return ifStmt;
		}
	}
}

