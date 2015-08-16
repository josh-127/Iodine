using System;

namespace Iodine.Compiler.Ast
{
	public class NodeBreak : AstNode
	{
		public NodeBreak (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

