using System;

namespace Iodine
{
	public class NodeContinue : AstNode
	{
		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

