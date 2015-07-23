using System;
using System.Collections.Generic;

namespace Iodine
{
	public class NodeFuncDecl : AstNode
	{
		public string Name {
			protected set;
			get;
		}

		public IList<string> Parameters {
			private set;
			get;
		}

		public bool InstanceMethod {
			private set;
			get;
		}

		public bool Variadic {
			private set;
			get;
		}

		public NodeFuncDecl (Location location, string name, bool isInstanceMethod, bool isVariadic,
			IList<string> parameters)
			: base (location)
		{
			this.Name = name;
			this.Parameters = parameters;
			this.InstanceMethod = isInstanceMethod;
			this.Variadic = isVariadic;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream, bool prototype = false, NodeClassDecl cdecl =
			null)
		{
			if (stream.Accept (TokenClass.Operator, "@")) {
				/*
				 * Function decorators in the form of 
				 * @myDecorator
				 * func foo () {
				 * }
				 * are merely syntatic sugar for
				 * func foo () {
				 * }
				 * foo = myDecorator (foo)
				 */
				AstNode expr = NodeExpr.Parse (stream); // Decorator expression 
				/* This is the original function which is to be decorated */
				NodeFuncDecl idecl = NodeFuncDecl.Parse (stream, prototype, cdecl) as NodeFuncDecl;
				/* We must construct an arglist which will be passed to the decorator */
				NodeArgList args = new NodeArgList (stream.Location);
				args.Add (new NodeIdent (stream.Location, idecl.Name));
				/*
				 * Since two values can not be returned, we must return a single node containing both
				 * the function declaration and call to the decorator 
				 */
				Ast nodes = new Ast (stream.Location);
				nodes.Add (idecl);
				nodes.Add (new NodeExpr (stream.Location, new NodeBinOp (stream.Location,
					BinaryOperation.Assign, new NodeIdent (stream.Location, idecl.Name), new NodeCall (
						stream.Location, expr, args))));
				return nodes;
			}
			stream.Expect (TokenClass.Keyword, "func");
			bool isInstanceMethod;
			bool isVariadic;
			Token ident = stream.Expect (TokenClass.Identifier);
			List<string> parameters = ParseFuncParameters (stream, out isInstanceMethod, out isVariadic);
			NodeFuncDecl decl = new NodeFuncDecl (stream.Location, ident != null ? ident.Value : "",
				isInstanceMethod, isVariadic, parameters);
			if (!prototype) {
				stream.Expect (TokenClass.OpenBrace);
				NodeScope scope = new NodeScope (stream.Location);

				if (stream.Match (TokenClass.Keyword, "super")) {
					scope.Add (NodeSuperCall.Parse (stream, cdecl));
				}

				while (!stream.Match (TokenClass.CloseBrace)) {
					scope.Add (NodeStmt.Parse (stream));
				}

				decl.Add (scope);
				stream.Expect (TokenClass.CloseBrace);
			}
			return decl;
		}

		private static List<string> ParseFuncParameters (TokenStream stream, out bool isInstanceMethod,
			out bool isVariadic)
		{
			isVariadic = false;
			List<string> ret = new List<string> ();
			stream.Expect (TokenClass.OpenParan);
			if (stream.Accept (TokenClass.Keyword, "self")) {
				isInstanceMethod = true;
				if (!stream.Accept (TokenClass.Comma)) {
					stream.Expect (TokenClass.CloseParan);
					return ret;
				}
			} else {
				isInstanceMethod = false;
			}
			while (!stream.Match (TokenClass.CloseParan)) {
				if (stream.Accept (TokenClass.Keyword, "params")) {
					isVariadic = true;
					Token ident = stream.Expect (TokenClass.Identifier);
					ret.Add (ident.Value);
					stream.Expect (TokenClass.CloseParan);
					return ret;
				}
				Token param = stream.Expect (TokenClass.Identifier);
				ret.Add (param.Value);
				if (!stream.Accept (TokenClass.Comma)) {
					break;
				}
			}
			stream.Expect (TokenClass.CloseParan);
			return ret;
		}
	}
}

