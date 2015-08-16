using System;

namespace Iodine.Compiler.Ast
{
	public class NodeContinue : AstNode
	{
		public NodeContinue (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

