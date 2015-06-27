using System;

namespace Iodine
{
	public class NodeUseStatement : AstNode
	{
		public string Module
		{
			private set;
			get;
		}

		public NodeUseStatement (string module)
		{
			this.Module = module;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static NodeUseStatement Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "use");
			Token ident = stream.Expect (TokenClass.Identifier);
			return new NodeUseStatement (ident.Value);
		}
	}
}

