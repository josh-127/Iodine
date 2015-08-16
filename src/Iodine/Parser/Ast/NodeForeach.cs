using System;

namespace Iodine.Compiler.Ast
{
	public class NodeForeach : AstNode
	{
		public string Item {
			private set;
			get;
		}

		public AstNode Iterator {
			get {
				return this.Children[0];
			}
		}

		public AstNode Body {
			get {
				return this.Children[1];
			}
		}

		public NodeForeach (Location location, string item, AstNode iterator, AstNode body)
			: base (location)
		{
			this.Item = item;
			this.Add (iterator);
			this.Add (body);
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream) 
		{
			stream.Expect (TokenClass.Keyword, "foreach");
			stream.Expect (TokenClass.OpenParan);
			Token identifier = stream.Expect (TokenClass.Identifier);
			stream.Expect (TokenClass.Keyword, "in");
			AstNode expr = NodeExpr.Parse (stream);
			stream.Expect (TokenClass.CloseParan);
			AstNode body = NodeStmt.Parse (stream);

			return new NodeForeach (stream.Location, identifier.Value, expr, body);

		}
	}
}

