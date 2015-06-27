using System;

namespace Iodine
{
	public class NodeArgList : AstNode
	{
		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			NodeArgList argList = new NodeArgList();
			stream.Expect (TokenClass.OpenParan);
			while (!stream.Match (TokenClass.CloseParan)) {
				argList.Add (NodeExpr.Parse (stream));
				if (!stream.Accept (TokenClass.Comma)) {
					break;
				}
			}
			stream.Expect (TokenClass.CloseParan);
			return argList;

		}
	}
}

