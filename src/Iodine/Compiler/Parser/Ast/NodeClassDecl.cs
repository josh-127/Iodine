using System;
using System.Collections.Generic;

namespace Iodine.Compiler.Ast
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

		public NodeClassDecl (Location location, string name, List<string> baseClass)
			: base (location)
		{
			this.Name = name;
			this.Base = baseClass;
			NodeFuncDecl dummyCtor = new NodeFuncDecl (location, name, true, false, new List<string> ());
			dummyCtor.Add (new NodeStmt (location));
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
					baseClass.Add (attr);
				} while (stream.Accept (TokenClass.Dot));
			}

			NodeClassDecl clazz = new NodeClassDecl (stream.Location, name, baseClass);

			stream.Expect (TokenClass.OpenBrace);

			while (!stream.Match (TokenClass.CloseBrace)) {
				if (stream.Match (TokenClass.Keyword, "func")  || stream.Match (TokenClass.Operator,
					"@")) {
					NodeFuncDecl func = NodeFuncDecl.Parse (stream, false, clazz) as NodeFuncDecl;
					if (func.Name == name) {
						clazz.Constructor = func;
					} else {
						clazz.Add (func);
					}
				} else if (stream.Match (TokenClass.Keyword, "class")) {
					NodeClassDecl subclass = NodeClassDecl.Parse (stream) as NodeClassDecl;
					clazz.Add (subclass);
				} else {
					stream.Expect (TokenClass.Keyword, "func");
				}
			}

			stream.Expect (TokenClass.CloseBrace);

			return clazz;
		}
	}
}
