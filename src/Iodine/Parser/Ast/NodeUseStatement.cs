using System;
using System.IO;
using System.Text;
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

		public bool Wildcard
		{
			private set;
			get;
		}

		public NodeUseStatement (string module)
		{
			this.Module = module;
			this.Imports = new List<string> ();
		}

		public NodeUseStatement (string module, List<string> imports, bool wildcard)
		{
			this.Module = module;
			this.Imports = imports;
			this.Wildcard = wildcard;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static NodeUseStatement Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "use");
			string ident = "";
			if (!stream.Match (TokenClass.Operator, "*"))
				ident = ParseModuleName (stream);
			if (stream.Match (TokenClass.Keyword, "from") || stream.Match (TokenClass.Comma) ||
				stream.Match (TokenClass.Operator, "*")) {
				List<string> items = new List<string> ();
				bool wildcard = false;
				if (!stream.Accept (TokenClass.Operator, "*")) {
					items.Add (ident);
					stream.Accept (TokenClass.Comma);
					while (!stream.Match (TokenClass.Keyword, "from")) {
						Token item = stream.Expect (TokenClass.Identifier);
						if (item != null) items.Add (item.Value);
						if (!stream.Accept (TokenClass.Comma)) {
							break;
						}
					}
				} else {
					wildcard = true;
				}
				stream.Expect (TokenClass.Keyword, "from");
				string module = ParseModuleName (stream);
				return new NodeUseStatement (module, items, wildcard);
			}
			return new NodeUseStatement (ident);
		}

		private static string ParseModuleName (TokenStream stream)
		{
			Token initIdent = stream.Expect (TokenClass.Identifier);

			if (stream.Match (TokenClass.Dot)) {
				stream.Expect (TokenClass.Dot);
				StringBuilder accum = new StringBuilder ();
				accum.Append (initIdent.Value);
				while (stream.Match (TokenClass.Identifier)) {
					Token ident = stream.Expect (TokenClass.Identifier);
					accum.Append (Path.DirectorySeparatorChar);
					accum.Append (ident.Value);
				}
				return accum.ToString ();

			} else {
				return initIdent.Value;
			}
		}
	}
}

