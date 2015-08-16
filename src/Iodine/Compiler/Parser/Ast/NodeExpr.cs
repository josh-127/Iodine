using System;

namespace Iodine.Compiler.Ast
{
	public class NodeExpr : AstNode
	{
		public NodeExpr (Location location, AstNode child)
			: base (location)
		{
			this.Add (child);
		}

		public override void Visit (IAstVisitor visitor) {
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream) {
			return ParseAssign (stream);
		}

		private static AstNode ParseAssign (TokenStream stream)
		{
			AstNode left = ParseBoolOr (stream);
			if (stream.Accept (TokenClass.Operator, "=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Assign, left, ParseAssign (stream));
			} else if (stream.Accept (TokenClass.Operator, "+=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Assign, left,
					new NodeBinOp (stream.Location, BinaryOperation.Add, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "-=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Assign, left,
					new NodeBinOp (stream.Location, BinaryOperation.Sub, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "*=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Assign, left,
					new NodeBinOp (stream.Location, BinaryOperation.Mul, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "/=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Assign, left,
					new NodeBinOp (stream.Location, BinaryOperation.Div, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "%=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Assign, left,
					new NodeBinOp (stream.Location, BinaryOperation.Mod, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "^=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Assign, left,
					new NodeBinOp (stream.Location, BinaryOperation.Xor, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "&=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Assign, left,
					new NodeBinOp (stream.Location, BinaryOperation.And, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "|=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Assign, left,
					new NodeBinOp (stream.Location, BinaryOperation.Or, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "<<=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Assign, left,
					new NodeBinOp (stream.Location, BinaryOperation.LeftShift, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, ">>=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Assign, left,
					new NodeBinOp (stream.Location, BinaryOperation.RightShift, left, ParseAssign (stream)));
			}
			return left;
		}

		private static AstNode ParseBoolOr (TokenStream stream) 
		{
			AstNode left = ParseBoolAnd (stream);
			if (stream.Accept (TokenClass.Operator, "||")) {
				return new NodeBinOp (stream.Location, BinaryOperation.BoolOr, left, ParseBoolOr (stream));
			}
			return left;
		}

		private static AstNode ParseBoolAnd (TokenStream stream)
		{
			AstNode left = ParseOr (stream);
			if (stream.Accept (TokenClass.Operator, "&&")) {
				return new NodeBinOp (stream.Location, BinaryOperation.BoolAnd, left, ParseBoolAnd (stream));
			}
			return left;
		}

		public static AstNode ParseOr (TokenStream stream)
		{
			AstNode left = ParseXor (stream);
			if (stream.Accept (TokenClass.Operator, "|")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Or, left, ParseOr (stream));
			}
			return left;
		}

		public static AstNode ParseXor (TokenStream stream)
		{
			AstNode left = ParseAnd (stream);
			if (stream.Accept (TokenClass.Operator, "^")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Xor, left, ParseXor (stream));
			}
			return left;
		}

		public static AstNode ParseAnd (TokenStream stream)
		{
			AstNode left = ParseEquals (stream);
			if (stream.Accept (TokenClass.Operator, "&")) {
				return new NodeBinOp (stream.Location, BinaryOperation.And, left, ParseAnd (stream));
			}
			return left;
		}

		public static AstNode ParseEquals (TokenStream stream)
		{
			AstNode left = ParseRelationalOp (stream);
			if (stream.Accept (TokenClass.Operator, "==")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Equals, left, ParseEquals (stream));
			} else if (stream.Accept (TokenClass.Operator, "!=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.NotEquals, left, ParseEquals (stream));
			}
			return left;
		}

		public static AstNode ParseRelationalOp (TokenStream stream)
		{
			AstNode left = ParseBitshift (stream);
			if (stream.Accept (TokenClass.Operator, ">")) {
				return new NodeBinOp (stream.Location, BinaryOperation.GreaterThan, left, ParseRelationalOp (stream));
			} else if (stream.Accept (TokenClass.Operator, "<")) {
				return new NodeBinOp (stream.Location, BinaryOperation.LessThan, left, ParseRelationalOp (stream));
			} else if (stream.Accept (TokenClass.Operator, ">=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.GreaterThanOrEqu, left,
					ParseRelationalOp (stream));
			} else if (stream.Accept (TokenClass.Operator, "<=")) {
				return new NodeBinOp (stream.Location, BinaryOperation.LessThanOrEqu, left,
					ParseRelationalOp (stream));
			} else if (stream.Accept (TokenClass.Keyword, "is")) {
				return new NodeBinOp (stream.Location, BinaryOperation.InstanceOf, left,
					ParseRelationalOp (stream));
			}
			return left;
		}

		public static AstNode ParseBitshift (TokenStream stream)
		{
			AstNode left = ParseAddSub (stream);
			if (stream.Accept (TokenClass.Operator, "<<")) {
				return new NodeBinOp (stream.Location, BinaryOperation.LeftShift, left, ParseBitshift (
					stream));
			} else  if (stream.Accept (TokenClass.Operator, ">>")) {
				return new NodeBinOp (stream.Location, BinaryOperation.RightShift, left, ParseBitshift (
					stream));
			}
			return left;
		}

		public static AstNode ParseAddSub (TokenStream stream)
		{
			AstNode left = ParseMulDivMod (stream);
			if (stream.Accept (TokenClass.Operator, "+")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Add, left, ParseAddSub (stream));
			} else if (stream.Accept (TokenClass.Operator, "-")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Sub, left, ParseAddSub (stream));
			}
			return left;
		}

		public static AstNode ParseMulDivMod (TokenStream stream)
		{
			AstNode left = ParseIncDecNot (stream);
			if (stream.Accept (TokenClass.Operator, "*")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Mul, left, ParseMulDivMod (stream));
			} else  if (stream.Accept (TokenClass.Operator, "/")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Div, left, ParseMulDivMod (stream));
			} else  if (stream.Accept (TokenClass.Operator, "%")) {
				return new NodeBinOp (stream.Location, BinaryOperation.Mod, left, ParseMulDivMod (stream));
			}
			return left;
		}

		public static AstNode ParseIncDecNot (TokenStream stream)
		{
			if (stream.Accept (TokenClass.Operator, "++")) {
				return new NodeUnaryOp (stream.Location, UnaryOperation.PrefixIncrement, ParseIncDecNot (
					stream));
			} else if (stream.Accept (TokenClass.Operator, "--")) {
				return new NodeUnaryOp (stream.Location, UnaryOperation.PrefixDecrement, ParseIncDecNot (
					stream));
			} else if (stream.Accept (TokenClass.Operator, "-")) {
				return new NodeUnaryOp (stream.Location, UnaryOperation.Negate, ParseIncDecNot (stream));
			} else if (stream.Accept (TokenClass.Operator, "~")) {
				return new NodeUnaryOp (stream.Location, UnaryOperation.Not, ParseIncDecNot (stream));
			} else if (stream.Accept (TokenClass.Operator, "!")) {
				return new NodeUnaryOp (stream.Location, UnaryOperation.BoolNot, ParseIncDecNot (stream));
			}
			return ParsePostIncDec (stream);
		}

		public static AstNode ParsePostIncDec (TokenStream stream)
		{
			AstNode value = ParseCallSubscriptAccess (stream);
			if (stream.Accept (TokenClass.Operator, "++")) {
				return new NodeUnaryOp (stream.Location, UnaryOperation.PostfixIncrement, value);
			} else if (stream.Accept (TokenClass.Operator, "--")) {
				return new NodeUnaryOp (stream.Location, UnaryOperation.PostfixDecrement, value);
			}
			return value;
		}

		public static AstNode ParseCallSubscriptAccess (TokenStream stream)
		{
			return ParseCallSubscriptAccess (stream, ParseTerm (stream));
		}

		public static AstNode ParseCallSubscriptAccess (TokenStream stream, AstNode lvalue)
		{
			if (stream.Match (TokenClass.OpenParan)) {
				return ParseCallSubscriptAccess (stream, new NodeCall (stream.Location, lvalue,
					NodeArgList.Parse (stream)));
			} else if (stream.Match (TokenClass.OpenBracket)) {
				return ParseCallSubscriptAccess (stream, NodeIndexer.Parse (lvalue, stream));
			} else if (stream.Match (TokenClass.Dot)) {
				return ParseCallSubscriptAccess (stream, NodeGetAttr.Parse (lvalue, stream));
			}
			return lvalue;
		}

		public static AstNode ParseTerm (TokenStream stream)
		{
			Token token = null;
			if (stream.Accept (TokenClass.Identifier, ref token)) {
				return new NodeIdent (stream.Location, token.Value);
			} else if (stream.Accept (TokenClass.IntLiteral, ref token)) {
				return new NodeInteger (stream.Location, long.Parse (token.Value));
			} else if (stream.Accept (TokenClass.FloatLiteral, ref token)) {
				return new NodeFloat (stream.Location, double.Parse (token.Value));
			} else if (stream.Accept (TokenClass.InterpolatedStringLiteral, ref token)) {
				AstNode val = NodeString.Parse (stream.Location, token.Value);
				if (val == null) {
					stream.MakeError ();
					return new NodeString (stream.Location, "");
				}
				return val;
			} else if (stream.Accept (TokenClass.StringLiteral, ref token)) {
				return new NodeString (stream.Location, token.Value);
			} else if (stream.Match (TokenClass.OpenBrace)) {
				return NodeList.Parse (stream);
			} else if (stream.Accept (TokenClass.OpenParan)) {
				AstNode expr = NodeExpr.Parse (stream);
				if (stream.Accept (TokenClass.Comma)) {
					return NodeTuple.Parse (expr, stream);
				}
				stream.Expect (TokenClass.CloseParan);
				return expr;
			} else if (stream.Accept (TokenClass.Keyword, "self")) {
				return new NodeSelf (stream.Location);
			} else if (stream.Accept (TokenClass.Keyword, "true")) {
				return new NodeTrue (stream.Location);
			} else if (stream.Accept (TokenClass.Keyword, "false")) {
				return new NodeFalse (stream.Location);
			} else if (stream.Accept (TokenClass.Keyword, "null")) {
				return new NodeNull (stream.Location);
			} else if (stream.Match (TokenClass.Keyword, "lambda")) {
				return NodeLambda.Parse (stream);
			}
			return null;
		}
	}
}

