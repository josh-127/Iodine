using System;

namespace Iodine
{
	public class NodeWhileStmt : AstNode
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

		public NodeWhileStmt (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			NodeWhileStmt ret = new NodeWhileStmt (stream.Location);
			stream.Expect (TokenClass.Keyword, "while");
			stream.Expect (TokenClass.OpenParan);
			ret.Add (NodeExpr.Parse (stream));
			stream.Expect (TokenClass.CloseParan);
			ret.Add (NodeStmt.Parse (stream));
			return ret;
		}
	}
}

