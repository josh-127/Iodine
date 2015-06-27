using System;

namespace Iodine
{
	public class NodeBinOp : AstNode
	{

		public BinaryOperation Operation
		{
			private set;
			get;
		}

		public AstNode Left
		{
			get
			{
				return this.Children[0];
			}
		}

		public AstNode Right
		{
			get
			{
				return this.Children[1];
			}
		}

		public NodeBinOp (BinaryOperation op, AstNode left, AstNode right)
		{
			this.Operation = op;
			this.Add (left);
			this.Add (right);
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

