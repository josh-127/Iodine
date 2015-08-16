using System;

namespace Iodine.Compiler.Ast
{
	public class NodeSelf : AstNode
	{
		public NodeSelf (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

