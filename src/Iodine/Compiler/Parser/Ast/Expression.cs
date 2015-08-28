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
			AstNode expr = ParseBoolOr (stream);
			while (stream.Match (TokenClass.Operator)) {
				switch (stream.Current.Value) {
				case "=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Assign,
						expr, ParseBoolOr (stream));
					continue;
				case "+=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Assign, expr,
						new BinaryExpression (stream.Location, BinaryOperation.Add, expr, ParseBoolOr (stream)));
					continue;
				case "-=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Assign, expr,
						new BinaryExpression (stream.Location, BinaryOperation.Sub, expr, ParseBoolOr (stream)));
					continue;
				case "*=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Assign, expr,
						new BinaryExpression (stream.Location, BinaryOperation.Mul, expr, ParseBoolOr (stream)));
					continue;
				case "/=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Assign, expr,
						new BinaryExpression (stream.Location, BinaryOperation.Div, expr, ParseBoolOr (stream)));
					continue;
				case "%=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Assign, expr,
						new BinaryExpression (stream.Location, BinaryOperation.Mod, expr, ParseBoolOr (stream)));
					continue;
				case "^=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Assign, expr,
						new BinaryExpression (stream.Location, BinaryOperation.Xor, expr, ParseBoolOr (stream)));
					continue;
				case "&=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Assign, expr,
						new BinaryExpression (stream.Location, BinaryOperation.And, expr, ParseBoolOr (stream)));
					continue;
				case "|=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Assign, expr,
						new BinaryExpression (stream.Location, BinaryOperation.Or, expr, ParseBoolOr (stream)));
					continue;
				case "<<=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Assign, expr,
						new BinaryExpression (stream.Location, BinaryOperation.LeftShift, expr, ParseBoolOr (stream)));
					continue;
				case ">>=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Assign, expr,
						new BinaryExpression (stream.Location, BinaryOperation.RightShift, expr, ParseBoolOr (stream)));
					continue;
				default:
					break;
				}
				break;
			}
			return expr;
		}

		private static AstNode ParseBoolOr (TokenStream stream) 
		{
			AstNode expr = ParseBoolAnd (stream);
			while (stream.Match (TokenClass.Operator)) {
				switch (stream.Current.Value) {
				case "||":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.BoolOr, expr,
						ParseBoolAnd (stream));
					continue;
				case "??":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.NullCoalescing, expr,
						ParseBoolAnd (stream));
					continue;
				default:
					break;
				}
				break;
			}
			return expr;
		}

		private static AstNode ParseBoolAnd (TokenStream stream)
		{
			AstNode expr = ParseOr (stream);
			while (stream.Accept (TokenClass.Operator, "&&")) {
				expr = new BinaryExpression (stream.Location, BinaryOperation.BoolAnd, expr, ParseOr (stream));
			}
			return expr;
		}

		public static AstNode ParseOr (TokenStream stream)
		{
			AstNode expr = ParseXor (stream);
			while (stream.Accept (TokenClass.Operator, "|")) {
				expr = new BinaryExpression (stream.Location, BinaryOperation.Or, expr, ParseXor (stream));
			}
			return expr;
		}

		public static AstNode ParseXor (TokenStream stream)
		{
			AstNode expr = ParseAnd (stream);
			while (stream.Accept (TokenClass.Operator, "^")) {
				expr = new BinaryExpression (stream.Location, BinaryOperation.Or, expr, ParseAnd (stream));
			}
			return expr;
		}

		public static AstNode ParseAnd (TokenStream stream)
		{
			AstNode expr = ParseEquals (stream);
			while (stream.Accept (TokenClass.Operator, "&")) {
				expr = new BinaryExpression (stream.Location, BinaryOperation.Add, expr,
					ParseEquals (stream));
			}
			return expr;
		}

		public static AstNode ParseEquals (TokenStream stream)
		{
			AstNode expr = ParseRelationalOp (stream);
			while (stream.Match (TokenClass.Operator)) {
				switch (stream.Current.Value) {
				case "==":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Equals, expr,
						ParseRelationalOp (stream));
					continue;
				case "!=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.NotEquals, expr,
						ParseRelationalOp (stream));
					continue;
				default:
					break;
				}
				break;
			}
			return expr;
		}

		public static AstNode ParseRelationalOp (TokenStream stream)
		{
			AstNode expr = ParseBitshift (stream);
			while (stream.Match (TokenClass.Operator)) {
				switch (stream.Current.Value) {
				case ">":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.GreaterThan, expr,
						ParseBitshift (stream));
					continue;
				case "<":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.LessThan, expr,
						ParseBitshift (stream));
					continue;
				case ">=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.LessThanOrEqu, expr,
						ParseBitshift (stream));
					continue;
				case "<=":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.GreaterThanOrEqu, expr,
						ParseBitshift (stream));
					continue;
				case "is":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.InstanceOf, expr,
						ParseBitshift (stream));
					continue;
				case "isnot":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.NotInstanceOf, expr,
						ParseBitshift (stream));
					continue;
				case "as":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.DynamicCast, expr,
						ParseBitshift (stream));
					continue;
				default:
					break;
				}
				break;
			}
			return expr;
		}

		public static AstNode ParseBitshift (TokenStream stream)
		{
			AstNode expr = ParseAddSub (stream);
			while (stream.Match (TokenClass.Operator)) {
				switch (stream.Current.Value) {
				case "<<":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.LeftShift, expr,
						ParseAddSub (stream));
					continue;
				case ">>":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.RightShift, expr,
						ParseAddSub (stream));
					continue;
				default:
					break;
				}
				break;
			}
			return expr;
		}

		public static AstNode ParseAddSub (TokenStream stream)
		{
			AstNode expr = ParseMulDivMod (stream);
			while (stream.Match (TokenClass.Operator)) {
				switch (stream.Current.Value) {
				case "+":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Add, expr,
						ParseMulDivMod (stream));
					continue;
				case "-":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Sub, expr,
						ParseMulDivMod (stream));
					continue;
				default:
					break;
				}
				break;
			}
			return expr;
		}

		public static AstNode ParseMulDivMod (TokenStream stream)
		{
			AstNode expr = ParseIncDecNot (stream);
			while (stream.Match (TokenClass.Operator)) {
				switch (stream.Current.Value) {
				case "*":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Mul, expr,
						ParseIncDecNot (stream));
					continue;
				case "/":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Div, expr,
						ParseIncDecNot (stream));
					continue;
				case "%":
					stream.Accept (TokenClass.Operator);
					expr = new BinaryExpression (stream.Location, BinaryOperation.Mod, expr,
						ParseIncDecNot (stream));
					continue;
				default:
					break;
				}
				break;
			}
			return expr;
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

