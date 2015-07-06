using System;

namespace Iodine
{
	public class NodeContinue : AstNode
	{
		public NodeContinue (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

