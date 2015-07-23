using System;

namespace Iodine
{
	public class NodeDecoratedFuncDecl : AstNode
	{
		public NodeFuncDecl Function {
			private set;
			get;
		}

		public NodeDecoratedFuncDecl (NodeFuncDecl decl)
			: base (decl.Location)
		{
			this.Function = decl;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	
	}

}

