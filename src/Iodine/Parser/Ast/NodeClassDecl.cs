using System;
using System.Collections.Generic;

namespace Iodine
{
	public class NodeClassDecl : AstNode
	{
		public string Name {
			private set;
			get;
		}

		public List<string> Base {
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
			
		public NodeClassDecl (string name, List<string> baseClass)
		{
			this.Name = name;
			this.Base = baseClass;
			NodeFuncDecl dummyCtor = new NodeFuncDecl (name, true, false, new List<string> ());
			dummyCtor.Add (new NodeStmt ());
			this.Add (dummyCtor);
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "class");
			string name = stream.Expect (TokenClass.Identifier).Value;

			List<string> baseClass = new List<string> ();
			if (stream.Accept (TokenClass.Colon)) {
				do {
					string attr = stream.Expect (TokenClass.Identifier).Value;
					if (attr != null) baseClass.Add (attr);
				} while (stream.Accept (TokenClass.Dot));
			}

			NodeClassDecl clazz = new NodeClassDecl (name, baseClass);

			stream.Expect (TokenClass.OpenBrace);

			while (!stream.Match (TokenClass.CloseBrace)) {
				if (stream.Match (TokenClass.Keyword, "func")) {
					NodeFuncDecl func = NodeFuncDecl.Parse (stream, clazz) as NodeFuncDecl;
					if (func.Name == name) {
						clazz.Constructor = func;
					} else {
						clazz.Add (func);
					}
				} else {
					stream.Expect (TokenClass.Keyword, "func");
				}
			}

			stream.Expect (TokenClass.CloseBrace);

			return clazz;
		}
	}
}

