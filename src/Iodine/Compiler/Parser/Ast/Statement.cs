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
using Iodine.Compiler;

namespace Iodine.Compiler.Ast
{
	public class Statement : AstNode
	{
		public Statement (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream) 
		{
			if (stream.Match (TokenClass.Keyword, "class")) {
				return ClassDeclaration.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "enum")) {
				return EnumDeclaration.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "interface")) {
				return InterfaceDeclaration.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "func") ||
				stream.Match (TokenClass.Operator, "@")) {
				return FunctionDeclaration.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "if")) {
				return IfStatement.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "switch")) {
				return SwitchStatement.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "for")) {
				return ForStatement.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "foreach")) {
				return ForeachStatement.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "while")) {
				return WhileStatement.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "do")) {
				return DoStatement.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "use")) {
				return UseStatement.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "return")) {
				return ReturnStatement.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "yield")) {
				return YieldStatement.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "raise")) {
				return RaiseStatement.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "try")) {
				return TryExceptStatement.Parse (stream);
			} else if (stream.Accept (TokenClass.Keyword, "break")) {
				return new BreakStatement (stream.Location);
			} else if (stream.Accept (TokenClass.Keyword, "continue")) {
				return new ContinueStatement (stream.Location);
			} else if (stream.Accept (TokenClass.Keyword, "super")) {
				stream.ErrorLog.AddError (ErrorType.ParserError, stream.Location,
					"super () constructor must be called first!");
				return SuperCallExpression.Parse (stream, new ClassDeclaration (stream.Location, "", null));
			} else if (stream.Match (TokenClass.OpenBrace)) {
				return CodeBlock.Parse (stream);
			} else if (stream.Accept (TokenClass.SemiColon)) {
				return new Statement (stream.Location);
			} else {
				AstNode node = Expression.Parse (stream);
				if (node == null) {
					stream.MakeError ();
				}
				return new Expression (stream.Location, node);
			}
		}
	}
}

