using System;

namespace Iodine
{
	public class NodeIndexer : AstNode
	{
		public AstNode Target {
			get {
				return this.Children[0];
			}
		}

		public AstNode Index {
			get {
				return this.Children[1];
			}
		}

		public NodeIndexer (Location location, AstNode lvalue, AstNode index)
			: base (location)
		{
			this.Add (lvalue);
			this.Add (index);
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (AstNode lvalue, TokenStream stream)
		{
			stream.Expect (TokenClass.OpenBracket);
			AstNode index = NodeExpr.Parse (stream);
			stream.Expect (TokenClass.CloseBracket);
			return new NodeIndexer (stream.Location, lvalue, index);
		}
	}
}

