using System;

namespace Iodine
{
	public class NodeSelf : AstNode
	{
		public NodeSelf (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

