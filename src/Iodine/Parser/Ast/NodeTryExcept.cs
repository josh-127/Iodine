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

		public NodeTryExcept (string ident)
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
			stream.Expect (TokenClass.Keyword, "except");
			if (stream.Accept (TokenClass.OpenParan)) {
				Token ident = stream.Expect (TokenClass.Identifier);
				stream.Expect (TokenClass.CloseParan);
				if (ident == null) retVal = new NodeTryExcept (null);
				retVal = new NodeTryExcept (ident.Value);
			} else {
				retVal = new NodeTryExcept (null);
			}
			retVal.Add (tryBody);
			retVal.Add (NodeStmt.Parse (stream));
			return retVal;
		}
	}
}

