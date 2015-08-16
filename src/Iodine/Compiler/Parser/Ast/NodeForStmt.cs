using System;

namespace Iodine.Compiler.Ast
{
	public class NodeForStmt : AstNode
	{
		public AstNode Initializer {
			get {
				return this.Children[0];
			}
		}

		public AstNode Condition {
			get {
				return this.Children[1];
			}
		}

		public AstNode AfterThought {
			get {
				return this.Children[2];
			}

		}

		public AstNode Body {
			get {
				return this.Children[3];
			}
		}

		public NodeForStmt (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			NodeForStmt ret = new NodeForStmt (stream.Location);
			stream.Expect (TokenClass.Keyword, "for");
			stream.Expect (TokenClass.OpenParan);
			ret.Add (NodeExpr.Parse (stream));
			stream.Expect (TokenClass.SemiColon);
			ret.Add (NodeExpr.Parse (stream));
			stream.Expect (TokenClass.SemiColon);
			ret.Add (NodeExpr.Parse (stream));
			stream.Expect (TokenClass.CloseParan);
			ret.Add (NodeStmt.Parse (stream));
			return ret;
		}
	}
}

