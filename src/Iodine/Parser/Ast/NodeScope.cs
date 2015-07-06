using System;

namespace Iodine
{
	public class NodeScope : AstNode
	{
		public NodeScope (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			NodeScope ret = new NodeScope (stream.Location);
			stream.Expect (TokenClass.OpenBrace);

			while (!stream.Match (TokenClass.CloseBrace)) {
				ret.Add (NodeStmt.Parse (stream));
			}

			stream.Expect (TokenClass.CloseBrace);
			return ret;
		}
	}
}

