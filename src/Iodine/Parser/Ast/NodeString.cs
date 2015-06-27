using System;

namespace Iodine
{
	public class NodeString : AstNode
	{
		public string Value
		{
			private set;
			get;
		}

		public NodeString (string value)
		{
			this.Value = value;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

