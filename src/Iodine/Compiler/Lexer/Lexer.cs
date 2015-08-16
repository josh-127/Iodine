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

namespace Iodine.Compiler
{
	/// <summary>
	/// Iodine lexer class, tokenizes our source into a list of Token objects represented as a TokenStream object.
	/// </summary>
	public class Lexer
	{
		private InputStream input;
		private ErrorLog errorLog;

		static List<IMatcher> matchers = new List<IMatcher> ();

		static Lexer ()
		{
			matchers.Add (new MatchKeyword ());
			matchers.Add (new MatchNumber ());
			matchers.Add (new MatchStringLit ());
			matchers.Add (new MatchGrouping ());
			matchers.Add (new MatchOperator ());
			matchers.Add (new MathComment ());
			matchers.Add (new MatchIdent ());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Iodine.Compiler.Lexer"/> class.
		/// </summary>
		/// <param name="errorLog">Error log.</param>
		/// <param name="source">Source.</param>
		/// <param name="file">File.</param>
		public Lexer (ErrorLog errorLog, string source, string file = "") 
		{
			this.errorLog = errorLog;
			this.input = new InputStream (source, file);
		}

		/// <summary>
		/// Scan the source code.
		/// <returns>A TokenStream object containing the tokenized form of the input source code. </returns>
		/// </summary>
		public TokenStream Scan ()
		{
			TokenStream retStream = new TokenStream (this.errorLog);
			this.input.EatWhiteSpaces ();
			while (this.input.PeekChar () != -1) {
				bool matchFound = false;
				foreach (IMatcher matcher in matchers) {
					if (matcher.IsMatchImpl (this.input)) {
						Token token = matcher.ScanToken (this.errorLog, this.input);
						if (token != null) {
							retStream.AddToken (token);
						}
						matchFound = true;
						break;
					}
				}

				if (!matchFound) {
					errorLog.AddError (ErrorType.LexerError, this.input.Location, "Unexpected '{0}'", 
						(char)this.input.ReadChar ());
				}

				this.input.EatWhiteSpaces ();
			}
			return retStream;
		}
	}
}

