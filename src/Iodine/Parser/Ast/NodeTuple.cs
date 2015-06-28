using System;

namespace Iodine
{
	public class NodeTuple : AstNode
	{

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (AstNode firstVal, TokenStream stream)
		{
			NodeTuple tuple = new NodeTuple ();
			tuple.Add (firstVal);
			while (!stream.Match (TokenClass.CloseParan)) {
				tuple.Add (NodeExpr.Parse (stream));
				if (!stream.Accept (TokenClass.Comma)) {
					break;
				}
			}
			stream.Expect (TokenClass.CloseParan);
			return tuple;
		}
	}
}

