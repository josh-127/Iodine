using System;

namespace Iodine
{
	public class NodeNull : AstNode
	{
		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

