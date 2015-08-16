using System;

namespace Iodine.Compiler.Ast
{
	public class NodeList : AstNode
	{
		public NodeList (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.OpenBrace);
			NodeList ret = new NodeList (stream.Location);
			while (!stream.Match (TokenClass.CloseBrace)) {
				ret.Add (NodeExpr.Parse (stream));
				if (!stream.Accept (TokenClass.Comma)) {
					break;
				}
			}
			stream.Expect (TokenClass.CloseBrace);
			return ret;
		}
	}
}

