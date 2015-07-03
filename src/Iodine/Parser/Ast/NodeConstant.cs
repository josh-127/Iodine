using System;

namespace Iodine
{
	public class NodeConstant : AstNode
	{
		public string Name {
			private set;
			get;
		}

		public AstNode Value {
			get {
				return this.Children[0];
			}
		}

		public NodeConstant (string name)
		{
			this.Name = name;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			Token ident = stream.Expect (TokenClass.Identifier);
			stream.Expect (TokenClass.Colon);
			NodeConstant node = new NodeConstant (ident.Value);
			if (stream.Match (TokenClass.IntLiteral)) {
				node.Add (new NodeInteger (Int64.Parse (stream.Expect (TokenClass.IntLiteral).Value)));
			} else if (stream.Match (TokenClass.InterpolatedStringLiteral)) {
				node.Add (new NodeString (stream.Expect (TokenClass.InterpolatedStringLiteral).Value));
			}
			return node;
		}
	}
}

