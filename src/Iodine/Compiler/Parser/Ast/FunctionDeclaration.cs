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
	public class FunctionDeclaration : AstNode
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

		public bool AcceptsKeywordArgs {
			private set;
			get;
		}

		public FunctionDeclaration (Location location,
		                     string name,
		                     bool isInstanceMethod,
		                     bool isVariadic,
		                     bool hasKeywordArgs,
		                     IList<string> parameters)
			: base (location)
		{
			Name = name;
			Parameters = parameters;
			InstanceMethod = isInstanceMethod;
			Variadic = isVariadic;
			AcceptsKeywordArgs = hasKeywordArgs;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream, bool prototype = false, ClassDeclaration cdecl =
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
				AstNode expr = Expression.Parse (stream); // Decorator expression 
				/* This is the original function which is to be decorated */
				FunctionDeclaration idecl = FunctionDeclaration.Parse (stream, prototype, cdecl) as FunctionDeclaration;
				/* We must construct an arglist which will be passed to the decorator */
				ArgumentList args = new ArgumentList (stream.Location);
				args.Add (new NameExpression (stream.Location, idecl.Name));
				/*
				 * Since two values can not be returned, we must return a single node containing both
				 * the function declaration and call to the decorator 
				 */
				AstRoot nodes = new AstRoot (stream.Location);
				nodes.Add (idecl);
				nodes.Add (new Expression (stream.Location, new BinaryExpression (stream.Location,
					BinaryOperation.Assign,
					new NameExpression (stream.Location, idecl.Name),
					new CallExpression (stream.Location, expr, args))));
				return nodes;
			}
			stream.Expect (TokenClass.Keyword, "func");
			bool isInstanceMethod;
			bool isVariadic;
			bool hasKeywordArgs;
			Token ident = stream.Expect (TokenClass.Identifier);
			List<string> parameters = ParseFuncParameters (stream,
				                          out isInstanceMethod,
				                          out isVariadic,
				                          out hasKeywordArgs);
			
			FunctionDeclaration decl = new FunctionDeclaration (stream.Location, ident != null ? ident.Value : "",
				                    isInstanceMethod, isVariadic, hasKeywordArgs, parameters);
			if (!prototype) {
				stream.Expect (TokenClass.OpenBrace);
				CodeBlock scope = new CodeBlock (stream.Location);

				if (stream.Match (TokenClass.Keyword, "super")) {
					scope.Add (SuperCallExpression.Parse (stream, cdecl));
				}

				while (!stream.Match (TokenClass.CloseBrace)) {
					scope.Add (Statement.Parse (stream));
				}

				decl.Add (scope);
				stream.Expect (TokenClass.CloseBrace);
			}
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

