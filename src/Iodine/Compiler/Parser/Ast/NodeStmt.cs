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
	public class NodeStmt : AstNode
	{
		public NodeStmt (Location location)
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
				return NodeClassDecl.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "enum")) {
				return NodeEnumDecl.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "interface")) {
				return NodeInterfaceDecl.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "func") ||
				stream.Match (TokenClass.Operator, "@")) {
				return NodeFuncDecl.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "if")) {
				return NodeIfStmt.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "switch")) {
				return NodeSwitchStmt.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "for")) {
				return NodeForStmt.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "foreach")) {
				return NodeForeach.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "while")) {
				return NodeWhileStmt.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "do")) {
				return NodeDoStmt.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "use")) {
				return NodeUseStatement.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "return")) {
				return NodeReturnStmt.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "yield")) {
				return NodeYieldStmt.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "raise")) {
				return NodeRaiseStmt.Parse (stream);
			} else if (stream.Match (TokenClass.Keyword, "try")) {
				return NodeTryExcept.Parse (stream);
			} else if (stream.Accept (TokenClass.Keyword, "break")) {
				return new NodeBreak (stream.Location);
			} else if (stream.Accept (TokenClass.Keyword, "continue")) {
				return new NodeContinue (stream.Location);
			} else if (stream.Accept (TokenClass.Keyword, "super")) {
				stream.ErrorLog.AddError (ErrorType.ParserError, stream.Location,
					"super () constructor must be called first!");
				return NodeSuperCall.Parse (stream, new NodeClassDecl (stream.Location, "", null));
			} else if (stream.Match (TokenClass.OpenBrace)) {
				return NodeScope.Parse (stream);
			} else if (stream.Accept (TokenClass.SemiColon)) {
				return new NodeStmt (stream.Location);
			} else {
				AstNode node = NodeExpr.Parse (stream);
				if (node == null) {
					stream.MakeError ();
				}
				return new NodeExpr (stream.Location, node);
			}
		}
	}
}

