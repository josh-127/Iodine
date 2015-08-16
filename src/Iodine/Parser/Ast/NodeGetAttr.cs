using System;

namespace Iodine.Compiler.Ast
{
	public class NodeGetAttr : AstNode
	{
		public AstNode Target {
			get {
				return this.Children[0];
			}
		}

		public string Field {
			private set;
			get;
		}

		public NodeGetAttr (Location location, AstNode target, string field)
			: base (location)
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
			return new NodeGetAttr (stream.Location, lvalue, ident.Value);
		}
	}
}

