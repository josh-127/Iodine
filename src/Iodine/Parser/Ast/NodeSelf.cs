using System;

namespace Iodine
{
	public class NodeSelf : AstNode
	{
		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

