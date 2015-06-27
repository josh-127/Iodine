using System;
using System.Collections.Generic;

namespace Iodine
{
	public class NodeClassDecl : AstNode
	{
		public string Name
		{
			private set;
			get;
		}

		public string Base
		{
			private set;
			get;
		}

		public NodeFuncDecl Constructor
		{
		    set
			{
				this.Children[0] = value;
			}
			get
			{
				return (NodeFuncDecl)this.Children[0];
			}
		}
			
		public NodeClassDecl (string name, string baseClass)
		{
			this.Name = name;
			this.Base = baseClass;
			//NodeFuncDecl dummyCtor = new NodeFuncDecl (name, true, new List<string> ());
			//dummyCtor.Add (new Ast ());
			this.Add (new NodeFuncDecl (name, true, new List<string> ()));
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "class");
			string name = stream.Expect (TokenClass.Identifier).Value;
			string baseClass = null;
			if (stream.Accept (TokenClass.Colon)) {
				baseClass = stream.Expect (TokenClass.Identifier).Value;
			}

			NodeClassDecl clazz = new NodeClassDecl (name, baseClass);

			stream.Expect (TokenClass.OpenBrace);

			while (!stream.Match (TokenClass.CloseBrace)) {
				if (stream.Match (TokenClass.Keyword, "func")) {
					NodeFuncDecl func = NodeFuncDecl.Parse (stream) as NodeFuncDecl;
					if (func.Name == name) {
						clazz.Constructor = func;
					}
					clazz.Add (func);
				} else {
					stream.Expect (TokenClass.Keyword, "func");
				}
			}

			stream.Expect (TokenClass.CloseBrace);

			return clazz;
		}
	}
}

