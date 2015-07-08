using System;
using System.Collections.Generic;

namespace Iodine
{
	public class NodeEnumDecl : AstNode
	{
		public string Name {
			private set;
			get;
		}

		public Dictionary<string, int> Items {
			private set;
			get;
		}

		public NodeFuncDecl Constructor {
			get {
				return (NodeFuncDecl)this.Children[0];
			}
			set {
				this.Children[0] = value;
			}
		}

		public NodeEnumDecl (Location location, string name)
			: base (location)
		{
			this.Name = name;
			this.Items = new Dictionary<string, int> ();
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "enum");
			string name = stream.Expect (TokenClass.Identifier).Value;
			NodeEnumDecl decl = new NodeEnumDecl (stream.Location, name);

			stream.Expect (TokenClass.OpenBrace);
			int defaultVal = -1;

			while (!stream.Match (TokenClass.CloseBrace)) {
				string ident = stream.Expect (TokenClass.Identifier).Value;
				if (stream.Accept (TokenClass.Operator, "=")) {
					string val = stream.Expect (TokenClass.IntLiteral).Value;
					int numVal = 0;
					if (val != "") {
						numVal = Int32.Parse (val);
					}
					decl.Items[ident] = numVal;
				} else {
					decl.Items[ident] = defaultVal--;
				}
				if (!stream.Accept (TokenClass.Comma)) {
					break;
				}
			}

			stream.Expect (TokenClass.CloseBrace);

			return decl;
		}
	}
}

