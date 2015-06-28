using System;
using System.Collections.Generic;

namespace Iodine
{
	public class NodeUseStatement : AstNode
	{
		public string Module
		{
			private set;
			get;
		}

		public List<string> Imports
		{
			private set;
			get;
		}

		public NodeUseStatement (string module)
		{
			this.Module = module;
			this.Imports = new List<string> ();
		}

		public NodeUseStatement (string module, List<string> imports)
		{
			this.Module = module;
			this.Imports = imports;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static NodeUseStatement Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "use");
			Token ident = stream.Expect (TokenClass.Identifier);
			if (stream.Match (TokenClass.Keyword, "from") || stream.Match (TokenClass.Comma)) {
				List<string> items = new List<string> ();
				items.Add (ident.Value);
				stream.Accept (TokenClass.Comma);
				while (!stream.Match (TokenClass.Keyword, "from")) {
					Token item = stream.Expect (TokenClass.Identifier);
					if (item != null) items.Add (item.Value);
					if (!stream.Accept (TokenClass.Comma)) {
						break;
					}
				}
				stream.Expect (TokenClass.Keyword, "from");
				Token module = stream.Expect (TokenClass.Identifier);
				return new NodeUseStatement (module.Value, items);
			}
			return new NodeUseStatement (ident.Value);
		}
	}
}

