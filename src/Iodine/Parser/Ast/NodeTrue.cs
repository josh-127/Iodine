using System;

namespace Iodine.Compiler.Ast
{
	public class NodeTrue : AstNode
	{
		public NodeTrue (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}

