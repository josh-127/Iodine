using System;
using System.Collections.Generic;

namespace Iodine
{
	public class NodeLambda : AstNode
	{
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

		public NodeLambda (bool isInstanceMethod, IList<string> parameters)
		{
			this.Parameters = parameters;
			this.InstanceMethod = isInstanceMethod;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "lambda");
			bool isInstanceMethod;
			List<string> parameters = ParseFuncParameters (stream, out isInstanceMethod);
			stream.Expect (TokenClass.Operator, "=>");
			NodeLambda decl = new NodeLambda (isInstanceMethod, parameters);
			decl.Add (NodeStmt.Parse (stream));
			return decl;
		}

		private static List<string> ParseFuncParameters (TokenStream stream, out bool isInstanceMethod)
		{
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

