using System;

namespace Iodine
{
	public class NodeIdent : AstNode
	{
		public string Value
		{
			private set;
			get;
		}

		public NodeIdent (string value)
		{
			this.Value = value;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

