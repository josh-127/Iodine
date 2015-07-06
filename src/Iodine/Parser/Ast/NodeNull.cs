using System;

namespace Iodine
{
	public class NodeNull : AstNode
	{
		public NodeNull (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

