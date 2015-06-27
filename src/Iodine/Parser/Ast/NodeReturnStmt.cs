using System;

namespace Iodine
{
	public class NodeReturnStmt : AstNode
	{
		public AstNode Value
		{
			get
			{
				return this.Children[0];
			}
		}

		public NodeReturnStmt (AstNode val)
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
				return new NodeReturnStmt (null);
			} else {
				return new NodeReturnStmt (NodeExpr.Parse (stream));
			}
		}
	}
}

