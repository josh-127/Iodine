using System;

namespace Iodine
{
	public class NodeTrue : AstNode
	{
		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

