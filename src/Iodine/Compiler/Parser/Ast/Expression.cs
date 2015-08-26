/**
  * Copyright (c) 2015, GruntTheDivine All rights reserved.

  * Redistribution and use in source and binary forms, with or without modification,
  * are permitted provided that the following conditions are met:
  * 
  *  * Redistributions of source code must retain the above copyright notice, this list
  *    of conditions and the following disclaimer.
  * 
  *  * Redistributions in binary form must reproduce the above copyright notice, this
  *    list of conditions and the following disclaimer in the documentation and/or
  *    other materials provided with the distribution.

  * Neither the name of the copyright holder nor the names of its contributors may be
  * used to endorse or promote products derived from this software without specific
  * prior written permission.
  * 
  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
  * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
  * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
  * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
  * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
  * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
  * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
  * CONTRACT ,STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
  * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
  * DAMAGE.
**/

using System;

namespace Iodine.Compiler.Ast
{
	public class Expression : AstNode
	{
		public Expression (Location location, AstNode child)
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
				return new BinaryExpression (stream.Location, BinaryOperation.Assign, left, ParseAssign (stream));
			} else if (stream.Accept (TokenClass.Operator, "+=")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Assign, left,
					new BinaryExpression (stream.Location, BinaryOperation.Add, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "-=")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Assign, left,
					new BinaryExpression (stream.Location, BinaryOperation.Sub, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "*=")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Assign, left,
					new BinaryExpression (stream.Location, BinaryOperation.Mul, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "/=")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Assign, left,
					new BinaryExpression (stream.Location, BinaryOperation.Div, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "%=")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Assign, left,
					new BinaryExpression (stream.Location, BinaryOperation.Mod, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "^=")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Assign, left,
					new BinaryExpression (stream.Location, BinaryOperation.Xor, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "&=")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Assign, left,
					new BinaryExpression (stream.Location, BinaryOperation.And, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "|=")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Assign, left,
					new BinaryExpression (stream.Location, BinaryOperation.Or, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, "<<=")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Assign, left,
					new BinaryExpression (stream.Location, BinaryOperation.LeftShift, left, ParseAssign (stream)));
			} else if (stream.Accept (TokenClass.Operator, ">>=")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Assign, left,
					new BinaryExpression (stream.Location, BinaryOperation.RightShift, left, ParseAssign (stream)));
			}
			return left;
		}

		private static AstNode ParseBoolOr (TokenStream stream) 
		{
			AstNode left = ParseBoolAnd (stream);
			if (stream.Accept (TokenClass.Operator, "||")) {
				return new BinaryExpression (stream.Location, BinaryOperation.BoolOr, left, ParseBoolOr (stream));
			} else if (stream.Accept (TokenClass.Operator, "??")) {
				return new BinaryExpression (stream.Location, BinaryOperation.NullCoalescing, left, ParseBoolOr (stream));
			}
			return left;
		}

		private static AstNode ParseBoolAnd (TokenStream stream)
		{
			AstNode left = ParseOr (stream);
			if (stream.Accept (TokenClass.Operator, "&&")) {
				return new BinaryExpression (stream.Location, BinaryOperation.BoolAnd, left, ParseBoolAnd (stream));
			}
			return left;
		}

		public static AstNode ParseOr (TokenStream stream)
		{
			AstNode left = ParseXor (stream);
			if (stream.Accept (TokenClass.Operator, "|")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Or, left, ParseOr (stream));
			}
			return left;
		}

		public static AstNode ParseXor (TokenStream stream)
		{
			AstNode left = ParseAnd (stream);
			if (stream.Accept (TokenClass.Operator, "^")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Xor, left, ParseXor (stream));
			}
			return left;
		}

		public static AstNode ParseAnd (TokenStream stream)
		{
			AstNode left = ParseEquals (stream);
			if (stream.Accept (TokenClass.Operator, "&")) {
				return new BinaryExpression (stream.Location, BinaryOperation.And, left, ParseAnd (stream));
			}
			return left;
		}

		public static AstNode ParseEquals (TokenStream stream)
		{
			AstNode left = ParseRelationalOp (stream);
			if (stream.Accept (TokenClass.Operator, "==")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Equals, left, ParseEquals (stream));
			} else if (stream.Accept (TokenClass.Operator, "!=")) {
				return new BinaryExpression (stream.Location, BinaryOperation.NotEquals, left, ParseEquals (stream));
			}
			return left;
		}

		public static AstNode ParseRelationalOp (TokenStream stream)
		{
			AstNode left = ParseBitshift (stream);
			if (stream.Accept (TokenClass.Operator, ">")) {
				return new BinaryExpression (stream.Location, BinaryOperation.GreaterThan, left, ParseRelationalOp (stream));
			} else if (stream.Accept (TokenClass.Operator, "<")) {
				return new BinaryExpression (stream.Location, BinaryOperation.LessThan, left, ParseRelationalOp (stream));
			} else if (stream.Accept (TokenClass.Operator, ">=")) {
				return new BinaryExpression (stream.Location, BinaryOperation.GreaterThanOrEqu, left,
					ParseRelationalOp (stream));
			} else if (stream.Accept (TokenClass.Operator, "<=")) {
				return new BinaryExpression (stream.Location, BinaryOperation.LessThanOrEqu, left,
					ParseRelationalOp (stream));
			} else if (stream.Accept (TokenClass.Keyword, "is")) {
				return new BinaryExpression (stream.Location, BinaryOperation.InstanceOf, left,
					ParseRelationalOp (stream));
			} else if (stream.Accept (TokenClass.Keyword, "isnot")) {
				return new BinaryExpression (stream.Location, BinaryOperation.NotInstanceOf, left,
					ParseRelationalOp (stream));
			} else if (stream.Accept (TokenClass.Keyword, "as")) {
				return new BinaryExpression (stream.Location, BinaryOperation.DynamicCast, left,
					ParseRelationalOp (stream));
			}
			return left;
		}

		public static AstNode ParseBitshift (TokenStream stream)
		{
			AstNode left = ParseAddSub (stream);
			if (stream.Accept (TokenClass.Operator, "<<")) {
				return new BinaryExpression (stream.Location, BinaryOperation.LeftShift, left, ParseBitshift (
					stream));
			} else  if (stream.Accept (TokenClass.Operator, ">>")) {
				return new BinaryExpression (stream.Location, BinaryOperation.RightShift, left, ParseBitshift (
					stream));
			}
			return left;
		}

		public static AstNode ParseAddSub (TokenStream stream)
		{
			AstNode left = ParseMulDivMod (stream);
			if (stream.Accept (TokenClass.Operator, "+")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Add, left, ParseAddSub (stream));
			} else if (stream.Accept (TokenClass.Operator, "-")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Sub, left, ParseAddSub (stream));
			}
			return left;
		}

		public static AstNode ParseMulDivMod (TokenStream stream)
		{
			AstNode left = ParseIncDecNot (stream);
			if (stream.Accept (TokenClass.Operator, "*")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Mul, left, ParseMulDivMod (stream));
			} else  if (stream.Accept (TokenClass.Operator, "/")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Div, left, ParseMulDivMod (stream));
			} else  if (stream.Accept (TokenClass.Operator, "%")) {
				return new BinaryExpression (stream.Location, BinaryOperation.Mod, left, ParseMulDivMod (stream));
			}
			return left;
		}

		public static AstNode ParseIncDecNot (TokenStream stream)
		{
			if (stream.Accept (TokenClass.Operator, "++")) {
				return new UnaryExpression (stream.Location, UnaryOperation.PrefixIncrement, ParseIncDecNot (
					stream));
			} else if (stream.Accept (TokenClass.Operator, "--")) {
				return new UnaryExpression (stream.Location, UnaryOperation.PrefixDecrement, ParseIncDecNot (
					stream));
			} else if (stream.Accept (TokenClass.Operator, "-")) {
				return new UnaryExpression (stream.Location, UnaryOperation.Negate, ParseIncDecNot (stream));
			} else if (stream.Accept (TokenClass.Operator, "~")) {
				return new UnaryExpression (stream.Location, UnaryOperation.Not, ParseIncDecNot (stream));
			} else if (stream.Accept (TokenClass.Operator, "!")) {
				return new UnaryExpression (stream.Location, UnaryOperation.BoolNot, ParseIncDecNot (stream));
			}
			return ParsePostIncDec (stream);
		}

		public static AstNode ParsePostIncDec (TokenStream stream)
		{
			AstNode value = ParseCallSubscriptAccess (stream);
			if (stream.Accept (TokenClass.Operator, "++")) {
				return new UnaryExpression (stream.Location, UnaryOperation.PostfixIncrement, value);
			} else if (stream.Accept (TokenClass.Operator, "--")) {
				return new UnaryExpression (stream.Location, UnaryOperation.PostfixDecrement, value);
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
				return ParseCallSubscriptAccess (stream, new CallExpression (stream.Location, lvalue,
					ArgumentList.Parse (stream)));
			} else if (stream.Match (TokenClass.OpenBracket)) {
				return ParseCallSubscriptAccess (stream, IndexerExpression.Parse (lvalue, stream));
			} else if (stream.Match (TokenClass.Operator, ".")) {
				return ParseCallSubscriptAccess (stream, GetExpression.Parse (lvalue, stream));
			}
			return lvalue;
		}

		public static AstNode ParseTerm (TokenStream stream)
		{
			Token token = null;
			if (stream.Accept (TokenClass.Identifier, ref token)) {
				return new NameExpression (stream.Location, token.Value);
			} else if (stream.Accept (TokenClass.IntLiteral, ref token)) {
				return new IntegerExpression (stream.Location, long.Parse (token.Value));
			} else if (stream.Accept (TokenClass.FloatLiteral, ref token)) {
				return new FloatExpression (stream.Location, double.Parse (token.Value));
			} else if (stream.Accept (TokenClass.InterpolatedStringLiteral, ref token)) {
				AstNode val = StringExpression.Parse (stream.Location, token.Value);
				if (val == null) {
					stream.MakeError ();
					return new StringExpression (stream.Location, "");
				}
				return val;
			} else if (stream.Accept (TokenClass.StringLiteral, ref token)) {
				return new StringExpression (stream.Location, token.Value);
			} else if (stream.Match (TokenClass.OpenBracket)) {
				return ListExpression.Parse (stream);
			} else if (stream.Match (TokenClass.OpenBrace)) {
				return HashExpression.Parse (stream);
			} else if (stream.Accept (TokenClass.OpenParan)) {
				AstNode expr = Expression.Parse (stream);
				if (stream.Accept (TokenClass.Comma)) {
					return TupleExpression.Parse (expr, stream);
				}
				stream.Expect (TokenClass.CloseParan);
				return expr;
			} else if (stream.Accept (TokenClass.Keyword, "self")) {
				return new SelfStatement (stream.Location);
			} else if (stream.Accept (TokenClass.Keyword, "true")) {
				return new TrueExpression (stream.Location);
			} else if (stream.Accept (TokenClass.Keyword, "false")) {
				return new FalseExpression (stream.Location);
			} else if (stream.Accept (TokenClass.Keyword, "null")) {
				return new NullExpression (stream.Location);
			} else if (stream.Match (TokenClass.Keyword, "lambda")) {
				return LambdaExpression.Parse (stream);
			}
			stream.MakeError ();
			return null;
		}
	}
}

