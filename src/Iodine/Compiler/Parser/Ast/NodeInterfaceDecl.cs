using System;
using System.Collections.Generic;

namespace Iodine.Compiler.Ast
{
	public class NodeInterfaceDecl : AstNode
	{
		public string Name {
			private set;
			get;
		}

		public NodeInterfaceDecl (Location location, string name)
			: base (location)
		{
			this.Name = name;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "interface");
			string name = stream.Expect (TokenClass.Identifier).Value;

			NodeInterfaceDecl contract = new NodeInterfaceDecl (stream.Location, name);

			stream.Expect (TokenClass.OpenBrace);

			while (!stream.Match (TokenClass.CloseBrace)) {
				if (stream.Match (TokenClass.Keyword, "func")) {
					NodeFuncDecl func = NodeFuncDecl.Parse (stream, true) as NodeFuncDecl;
					contract.Add (func);
				} else {
					stream.ErrorLog.AddError (ErrorType.ParserError, stream.Location, 
						"Interface may only contain function prototypes!");
				}
				while (stream.Accept (TokenClass.SemiColon));
			}

			stream.Expect (TokenClass.CloseBrace);

			return contract;
		}
	}
}

