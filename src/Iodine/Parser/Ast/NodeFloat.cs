using System;

namespace Iodine
{
	public class NodeFloat : AstNode
	{
		public double Value
		{
			private set;
			get;
		}

		public NodeFloat (double value)
		{
			this.Value = value;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

