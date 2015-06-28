using System;
using System.Collections.Generic;

namespace Iodine
{
	public class NodeFuncDecl : AstNode
	{
		public string Name
		{
			private set;
			get;
		}

		public IList<string> Parameters
		{
			private set;
			get;
		}

		public bool InstanceMethod
		{
			private set;
			get;
		}

		public bool Variadic
		{
			private set;
			get;
		}

		public NodeFuncDecl (string name, bool isInstanceMethod, bool isVariadic, IList<string> parameters)
		{
			this.Name = name;
			this.Parameters = parameters;
			this.InstanceMethod = isInstanceMethod;
			this.Variadic = isVariadic;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "func");
			bool isInstanceMethod;
			bool isVariadic;
			Token ident = stream.Expect (TokenClass.Identifier);
			List<string> parameters = ParseFuncParameters (stream, out isInstanceMethod, out isVariadic);
			NodeFuncDecl decl = new NodeFuncDecl (ident.Value, isInstanceMethod, isVariadic, parameters);
			decl.Add (NodeStmt.Parse (stream));
			return decl;
		}

		private static List<string> ParseFuncParameters (TokenStream stream, out bool isInstanceMethod,
			out bool isVariadic)
		{
			isVariadic = false;
			List<string> ret = new List<string> ();
			stream.Expect (TokenClass.OpenParan);
			if (stream.Accept (TokenClass.Keyword, "self")) {
				isInstanceMethod = true;
				if (!stream.Accept (TokenClass.Comma)) {
					stream.Expect (TokenClass.CloseParan);
					return ret;
				}
			} else {
				isInstanceMethod = false;
			}
			while (!stream.Match (TokenClass.CloseParan)) {
				if (stream.Accept (TokenClass.Keyword, "params")) {
					isVariadic = true;
					Token ident = stream.Expect (TokenClass.Identifier);
					if (ident != null)
						ret.Add (ident.Value);
					stream.Expect (TokenClass.CloseParan);
					return ret;
				}
				Token param = stream.Expect (TokenClass.Identifier);
				ret.Add (param.Value);
				if (!stream.Accept (TokenClass.Comma)) {
					break;
				}
			}
			stream.Expect (TokenClass.CloseParan);
			return ret;
		}
	}
}

