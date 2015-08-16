using System;

namespace Iodine.Compiler.Ast
{
	public class NodeFalse : AstNode
	{
		public NodeFalse (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

