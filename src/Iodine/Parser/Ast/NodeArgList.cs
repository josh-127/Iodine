using System;

namespace Iodine
{
	public class NodeArgList : AstNode
	{
		public bool Packed {
			protected set;
			get;
		}

		public NodeArgList (Location location)
			: base (location)
		{

		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
			
		public static AstNode Parse (TokenStream stream)
		{
			NodeArgList argList = new NodeArgList(stream.Location);
			stream.Expect (TokenClass.OpenParan);
			while (!stream.Match (TokenClass.CloseParan)) {
				if (stream.Accept (TokenClass.Operator, "*")) {
					argList.Packed = true;
					argList.Add (NodeExpr.Parse (stream));
					break;
				}
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

