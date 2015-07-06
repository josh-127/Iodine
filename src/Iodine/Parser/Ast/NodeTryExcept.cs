using System;

namespace Iodine
{
	public class NodeTryExcept : AstNode
	{
		public string ExceptionIdentifier {
			private set;
			get;
		}

		public AstNode TryBody {
			get {
				return this.Children[0];
			}
		}

		public AstNode ExceptBody {
			get {
				return this.Children[1];
			}
		}

		public AstNode TypeList {
			get {
				return this.Children[2];
			}
		}

		public NodeTryExcept (Location location, string ident)
			: base (location)
		{
			this.ExceptionIdentifier = ident;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			NodeTryExcept retVal = null;
			stream.Expect (TokenClass.Keyword, "try");
			AstNode tryBody = NodeStmt.Parse (stream);
			AstNode typeList = new NodeArgList (stream.Location);
			stream.Expect (TokenClass.Keyword, "except");
			if (stream.Accept (TokenClass.OpenParan)) {
				Token ident = stream.Expect (TokenClass.Identifier);
				if (stream.Accept (TokenClass.Keyword, "as")) {
					typeList = ParseTypeList (stream);
				}
				stream.Expect (TokenClass.CloseParan);
				retVal = new NodeTryExcept (stream.Location, ident.Value);
			} else {
				retVal = new NodeTryExcept (stream.Location, null);
			}
			retVal.Add (tryBody);
			retVal.Add (NodeStmt.Parse (stream));
			retVal.Add (typeList);
			return retVal;
		}

		private static NodeArgList ParseTypeList (TokenStream stream)
		{
			NodeArgList argList = new NodeArgList (stream.Location);
			while (!stream.Match (TokenClass.CloseParan)) {
				argList.Add (NodeExpr.Parse (stream));
				if (!stream.Accept (TokenClass.Comma)) {
					break;
				}
			}
			return argList;
		}
	}
}

