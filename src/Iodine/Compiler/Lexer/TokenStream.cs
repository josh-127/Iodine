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
	public class TokenStream
	{
		private ErrorLog errorLog;
		private List<Token> tokens = new List<Token> ();

		public bool EndOfStream {
			get {
				return this.tokens.Count <= Position;
			}
		}

		public int Position {
			private set;
			get;
		}

		public Location Location {
			get {
				if (this.peekToken () != null)
					return this.peekToken ().Location;
				else {
					return this.peekToken (-1).Location;
				}
			}
		}

		public ErrorLog ErrorLog {
			get {
				return this.errorLog;
			}
		}

		public TokenStream (ErrorLog errorLog)
		{
			this.errorLog = errorLog;
		}

		public void AddToken (Token token)
		{
			this.tokens.Add (token);
		}

		public bool Match (TokenClass clazz)
		{
			return peekToken () != null && peekToken ().Class == clazz;
		}

		public bool Match (TokenClass clazz1, TokenClass clazz2)
		{
			return peekToken () != null && peekToken ().Class == clazz1 &&
			peekToken (1) != null && peekToken (1).Class == clazz2;
		}

		public bool Match (TokenClass clazz, string val)
		{
			return peekToken () != null && peekToken ().Class == clazz &&
			peekToken ().Value == val;
		}

		public bool Accept (TokenClass clazz)
		{
			if (peekToken () != null && peekToken ().Class == clazz) {
				readToken ();
				return true;
			}
			return false;
		}

		public bool Accept (TokenClass clazz, ref Token token)
		{
			if (peekToken () != null && peekToken ().Class == clazz) {
				token = readToken ();
				return true;
			}
			return false;
		}

		public bool Accept (TokenClass clazz, string val)
		{
			if (peekToken () != null && peekToken ().Class == clazz && peekToken ().Value == val) {
				readToken ();
				return true;
			}
			return false;
		}

		public Token Expect (TokenClass clazz)
		{
			Token ret = null;
			if (Accept (clazz, ref ret)) {
				return ret;
			}

			Token offender = readToken ();
		
			if (offender != null) {
				errorLog.AddError (ErrorType.ParserError, offender.Location, "Unexpected '{0}' (Expected '{1}')",
					offender.ToString (), Token.ClassToString (clazz));
			} else {
				errorLog.AddError (ErrorType.ParserError, offender.Location, "Unexpected end of file (Expected {0})",
					Token.ClassToString (clazz));
				throw new Exception ("");
			}
			return new Token (clazz, "", Location);
		}

		public Token Expect (TokenClass clazz, string val)
		{
			Token ret = peekToken ();
			if (Accept (clazz, val)) {
				return ret;
			}

			Token offender = readToken ();

			if (offender != null) {
				errorLog.AddError (ErrorType.ParserError, offender.Location, 
					"Unexpected '{0}' (Expected '{1}')", offender.ToString (), Token.ClassToString (
					clazz));
			} else {
				errorLog.AddError (ErrorType.ParserError, offender.Location, 
					"Unexpected end of file (Expected {0})", Token.ClassToString (clazz));
				throw new Exception ("");
			}
			return new Token (clazz, "", Location);
		}

		public void MakeError ()
		{
			this.errorLog.AddError (ErrorType.ParserError, peekToken ().Location, "Unexpected {0}",
				readToken ().ToString ());
		}

		private Token peekToken ()
		{
			return peekToken (0);
		}

		private Token peekToken (int n)
		{
			if (this.Position + n >= this.tokens.Count) {
				return null;
			}
			return this.tokens [this.Position + n];
		}

		private Token readToken ()
		{
			if (this.Position >= this.tokens.Count) {
				return null;
			}
			return this.tokens [this.Position++];
		}
	}
}

