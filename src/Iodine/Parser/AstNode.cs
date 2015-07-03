using System;
using System.Collections;
using System.Collections.Generic;

namespace Iodine
{
	public abstract class AstNode : IEnumerable<AstNode>
	{
		private List<AstNode> children = new List<AstNode> ();

		public IList<AstNode> Children {
			get {
				return this.children;
			}
		}

		public abstract void Visit (IAstVisitor visitor);

		public void Add (AstNode node)
		{
			this.children.Add (node);
		}

		public IEnumerator<AstNode> GetEnumerator ()
		{
			foreach (AstNode node in this.children) {
				yield return node;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}

	public class Ast : AstNode
	{
		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static Ast Parse (TokenStream inputStream)
		{
			Ast root = new Ast ();
			while (!inputStream.EndOfStream) {
				root.Add (NodeStmt.Parse (inputStream));
			}
			return root;
		}
	}
}

