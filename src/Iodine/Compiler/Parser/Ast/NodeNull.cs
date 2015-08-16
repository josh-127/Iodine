using System;

namespace Iodine.Compiler.Ast
{
	public class NodeNull : AstNode
	{
		public NodeNull (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

