using System;

namespace Iodine.Compiler.Ast
{
	public class NodeUnaryOp : AstNode
	{
		public UnaryOperation Operation {
			private set;
			get;
		}

		public AstNode Value {
			get {
				return this.Children [0];
			}
		}

		public NodeUnaryOp (Location location, UnaryOperation op, AstNode val)
			: base (location)
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

