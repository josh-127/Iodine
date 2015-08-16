using System;

namespace Iodine
{
	public class NodeReturnStmt : AstNode
	{
		public AstNode Value {
			get {
				return this.Children [0];
			}
		}

		public NodeReturnStmt (Location location, AstNode val)
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
			stream.Expect (TokenClass.Keyword, "return");
			if (stream.Accept (TokenClass.SemiColon)) {
				return new NodeReturnStmt (stream.Location, new Ast (stream.Location));
			} else {
				return new NodeReturnStmt (stream.Location, NodeExpr.Parse (stream));
			}
		}
	}
}

