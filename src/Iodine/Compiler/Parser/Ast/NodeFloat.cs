using System;

namespace Iodine.Compiler.Ast
{
	public class NodeFloat : AstNode
	{
		public double Value {
			private set;
			get;
		}

		public NodeFloat (Location location, double value)
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

