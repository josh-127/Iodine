using System;

namespace Iodine
{
	public class NodeFalse : AstNode
	{
		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

