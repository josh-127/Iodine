using System;

namespace Iodine
{
	public class NodeForeach : AstNode
	{
		public string Item {
			private set;
			get;
		}

		public AstNode Iterator {
			get {
				return this.Children[0];
			}
		}

		public AstNode Body {
			get {
				return this.Children[1];
			}
		}

		public NodeForeach (string item, AstNode iterator, AstNode body)
		{
			this.Item = item;
			this.Add (iterator);
			this.Add (body);
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream) 
		{
			stream.Expect (TokenClass.Keyword, "foreach");
			stream.Expect (TokenClass.OpenParan);
			Token identifier = stream.Expect (TokenClass.Identifier);
			stream.Expect (TokenClass.Keyword, "in");
			AstNode expr = NodeExpr.Parse (stream);
			stream.Expect (TokenClass.CloseParan);
			AstNode body = NodeStmt.Parse (stream);

			if (identifier == null) {
				return new NodeForeach (null, expr, body);
			} else {
				return new NodeForeach (identifier.Value, expr, body);
			}
		}
	}
}

