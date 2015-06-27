using System;

namespace Iodine
{
	public class NodeScope : AstNode
	{
		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			NodeScope ret = new NodeScope ();
			stream.Expect (TokenClass.OpenBrace);

			while (!stream.Match (TokenClass.CloseBrace)) {
				ret.Add (NodeStmt.Parse (stream));
			}

			stream.Expect (TokenClass.CloseBrace);
			return ret;
		}
	}
}

