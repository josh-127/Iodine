using System;

namespace Iodine
{
	public class NodeSuperCall : AstNode
	{

		public AstNode Arguments {
			get {
				return this.Children [0];
			}
		}

		public NodeClassDecl Parent {
			set;
			get;
		}

		public NodeSuperCall (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static NodeSuperCall Parse (TokenStream stream, NodeClassDecl parent)
		{
			NodeSuperCall ret = new NodeSuperCall (stream.Location);
			stream.Expect (TokenClass.Keyword, "super");
			ret.Parent = parent;
			ret.Add (NodeArgList.Parse (stream));
			while (stream.Accept (TokenClass.SemiColon))
				;
			return ret;
		}
	}
}

