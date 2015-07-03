using System;

namespace Iodine
{
	public class NodeUnaryOp : AstNode
	{
		public UnaryOperation Operation {
			private set;
			get;
		}

		public AstNode Value {
			get {
				return this.Children[0];
			}
		}

		public NodeUnaryOp (UnaryOperation op, AstNode val)
		{
			this.Operation = op;
			this.Add (val);
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

