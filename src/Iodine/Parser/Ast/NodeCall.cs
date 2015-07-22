using System;

namespace Iodine
{
	public class NodeCall : AstNode
	{
		public AstNode Target {
			get {
				return this.Children[0];
			}
		}

		public NodeArgList Arguments {
			get {
				return (NodeArgList)this.Children[1];
			}
		}

		public NodeCall (Location location, AstNode target, AstNode args)
			: base (location)
		{
			this.Add (target);
			this.Add (args);
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
			
	}
}

