using System;

namespace Iodine.Compiler.Ast
{
	public class NodeIdent : AstNode
	{
		public string Value {
			private set;
			get;
		}

		public NodeIdent (Location location, string value)
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

