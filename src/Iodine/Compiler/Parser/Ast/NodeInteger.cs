using System;

namespace Iodine.Compiler.Ast
{
	public class NodeInteger : AstNode
	{
		public long Value {
			private set;
			get;
		}

		public NodeInteger (Location location, long value)
			: base (location)
		{
			this.Value = value;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

