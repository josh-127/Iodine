using System;

namespace Iodine
{
	public class NodeRaiseStmt : AstNode
	{
		public AstNode Value {
			get {
				return this.Children[0];
			}
		}

		public NodeRaiseStmt (Location location, AstNode val)
			: base (location)
		{
			this.Add (val);
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "raise");
			return new NodeRaiseStmt (stream.Location, NodeExpr.Parse (stream));
		}
	}
}

