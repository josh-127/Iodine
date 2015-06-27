using System;

namespace Iodine
{
	public class NodeBreak : AstNode
	{
		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

