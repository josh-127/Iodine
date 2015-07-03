using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Iodine
{
	public class NodeUseStatement : AstNode
	{
		public string Module {
			private set;
			get;
		}

		public List<string> Imports {
			private set;
			get;
		}

		public bool Wildcard {
			private set;
			get;
		}

		public bool Relative {
			private set;
			get;
		}

		public NodeUseStatement (string module, bool relative = false)
		{
			this.Module = module;
			this.Relative = relative;
			this.Imports = new List<string> ();
		}

		public NodeUseStatement (string module, List<string> imports, bool wildcard,
			bool relative = false)
		{
			this.Module = module;
			this.Imports = imports;
			this.Wildcard = wildcard;
			this.Relative = relative;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static NodeUseStatement Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "use");
			bool relative = stream.Accept (TokenClass.Dot);
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

				relative = stream.Accept (TokenClass.Dot);
				string module = ParseModuleName (stream);
				return new NodeUseStatement (module, items, wildcard, relative);
			}
			return new NodeUseStatement (ident, relative);
		}

		private static string ParseModuleName (TokenStream stream)
		{
			Token initIdent = stream.Expect (TokenClass.Identifier);

			if (stream.Match (TokenClass.Dot)) {
				StringBuilder accum = new StringBuilder ();
				accum.Append (initIdent.Value);
				while (stream.Accept (TokenClass.Dot)) {
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

