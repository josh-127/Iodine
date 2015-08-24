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
using System.Collections.Generic;

namespace Iodine.Compiler.Ast
{
	public class NodeLambda : AstNode
	{
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

		public bool AcceptsKeywordArguments {
			private set;
			get;
		}

		public NodeLambda (Location location,
			bool isInstanceMethod,
			bool variadic,
			bool acceptsKeywordArguments,
			IList<string> parameters)
			: base (location)
		{
			Parameters = parameters;
			InstanceMethod = isInstanceMethod;
			Variadic = variadic;
			AcceptsKeywordArguments = acceptsKeywordArguments;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "lambda");
			bool isInstanceMethod;
			bool isVariadic;
			bool acceptsKwargs;

			List<string> parameters = ParseFuncParameters (stream,
				out isInstanceMethod,
				out isVariadic,
				out acceptsKwargs);
			
			stream.Expect (TokenClass.Operator, "=>");
			NodeLambda decl = new NodeLambda (stream.Location, isInstanceMethod, isVariadic, acceptsKwargs, parameters);
			decl.Add (NodeStmt.Parse (stream));
			return decl;
		}

		private static List<string> ParseFuncParameters (TokenStream stream, out bool isInstanceMethod,
			out bool isVariadic,
			out bool hasKeywordArgs)
		{
			isVariadic = false;
			hasKeywordArgs = false;
			isInstanceMethod = false;
			List<string> ret = new List<string> ();
			stream.Expect (TokenClass.OpenParan);
			if (stream.Accept (TokenClass.Keyword, "self")) {
				isInstanceMethod = true;
				if (!stream.Accept (TokenClass.Comma)) {
					stream.Expect (TokenClass.CloseParan);
					return ret;
				}
			}
			while (!stream.Match (TokenClass.CloseParan)) {
				if (!hasKeywordArgs && stream.Accept (TokenClass.Operator, "*")) {
					if (stream.Accept (TokenClass.Operator, "*")) {
						hasKeywordArgs = true;
						Token ident = stream.Expect (TokenClass.Identifier);
						ret.Add (ident.Value);
					} else {
						isVariadic = true;
						Token ident = stream.Expect (TokenClass.Identifier);
						ret.Add (ident.Value);
					}
				} else {
					if (hasKeywordArgs) {
						stream.ErrorLog.AddError (ErrorType.ParserError, stream.Location,
							"Argument after keyword arguments!");
					}
					if (isVariadic) {
						stream.ErrorLog.AddError (ErrorType.ParserError, stream.Location,
							"Argument after params keyword!");
					}
					Token param = stream.Expect (TokenClass.Identifier);
					ret.Add (param.Value);
				}
				if (!stream.Accept (TokenClass.Comma)) {
					break;
				}
			}
			stream.Expect (TokenClass.CloseParan);
			return ret;
		}
	}
}

