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
using System.Text;
using System.Collections.Generic;

namespace Iodine.Compiler.Ast
{
	public class ClassDeclaration : AstNode
	{
		public string Name {
			private set;
			get;
		}

		public List<string> Base {
			private set;
			get;
		}

		public FunctionDeclaration Constructor {
			get {
				return (FunctionDeclaration)Children [0];
			}
			set {
				this.Children [0] = value;
			}
		}

		public ClassDeclaration (Location location, string name, List<string> baseClass)
			: base (location)
		{
			Name = name;
			Base = baseClass;
			FunctionDeclaration dummyCtor = new FunctionDeclaration (location, name, true, false, false, new List<string> ());
			dummyCtor.Add (new Statement (location));
			Add (dummyCtor);
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "class");
			string name = stream.Expect (TokenClass.Identifier).Value;

			List<string> baseClass = new List<string> ();
			if (stream.Accept (TokenClass.Colon)) {
				do {
					baseClass.Add (ParseClassName (stream));
				} while (stream.Accept (TokenClass.Comma));
			}

			ClassDeclaration clazz = new ClassDeclaration (stream.Location, name, baseClass);

			stream.Expect (TokenClass.OpenBrace);

			while (!stream.Match (TokenClass.CloseBrace)) {
				if (stream.Match (TokenClass.Keyword, "func") || stream.Match (TokenClass.Operator,
					    "@")) {
					FunctionDeclaration func = FunctionDeclaration.Parse (stream, false, clazz) as FunctionDeclaration;
					if (func.Name == name) {
						clazz.Constructor = func;
					} else {
						clazz.Add (func);
					}
				} else if (stream.Match (TokenClass.Keyword, "class")) {
					ClassDeclaration subclass = ClassDeclaration.Parse (stream) as ClassDeclaration;
					clazz.Add (subclass);
				} else {
					stream.Expect (TokenClass.Keyword, "func");
				}
			}

			stream.Expect (TokenClass.CloseBrace);

			return clazz;
		}

		private static string ParseClassName (TokenStream stream)
		{
			StringBuilder ret = new StringBuilder ();
			do {
				string attr = stream.Expect (TokenClass.Identifier).Value;
				ret.Append (attr);
				if (stream.Match (TokenClass.Dot))
					ret.Append ('.');
			} while (stream.Accept (TokenClass.Dot));
			return ret.ToString ();
		}
	}
}
