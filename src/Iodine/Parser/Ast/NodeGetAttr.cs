using System;

namespace Iodine
{
	public class NodeGetAttr : AstNode
	{
		public AstNode Target
		{
			get
			{
				return this.Children[0];
			}
		}

		public string Field
		{
			private set;
			get;
		}

		public NodeGetAttr (AstNode target, string field)
		{
			this.Add(target);
			this.Field = field;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (AstNode lvalue, TokenStream stream)
		{
			stream.Expect (TokenClass.Dot);
			Token ident = stream.Expect (TokenClass.Identifier);
			return new NodeGetAttr (lvalue, ident.Value);
		}
	}
}

