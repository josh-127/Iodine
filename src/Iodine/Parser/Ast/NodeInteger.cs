using System;

namespace Iodine
{
	public class NodeInteger : AstNode
	{
		public long Value
		{
			private set;
			get;
		}

		public NodeInteger (long value)
		{
			this.Value = value;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

