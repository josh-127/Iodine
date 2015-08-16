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

namespace Iodine.Compiler
{
	public class MatchStringLit : IMatcher
	{
		public bool IsMatchImpl (InputStream inputStream)
		{
			return inputStream.PeekChar () == '\"' || inputStream.PeekChar () == '\'';
		}

		public Token ScanToken (ErrorLog errLog, InputStream inputStream)
		{
			char quote = (char)inputStream.ReadChar ();
			TokenClass type = quote == '\"' ? TokenClass.InterpolatedStringLiteral : TokenClass.StringLiteral;
			string accum = scanUntil (quote, inputStream);
			if (inputStream.ReadChar () == -1) {
				errLog.AddError (ErrorType.LexerError, inputStream.Location, 
					"Unterminated string literal!");
				return null;
			} else {
				return Token.Create (type, accum, inputStream);
			}

		}

		private string scanUntil (char terminator, InputStream inputStream)
		{
			StringBuilder accum = new StringBuilder ();
			while (inputStream.PeekChar () != -1 && inputStream.PeekChar () != terminator) {
				if (inputStream.PeekChar () == '\\') {
					inputStream.ReadChar ();
					accum.Append (scanEscapeChar ((char)inputStream.ReadChar ()));
				} else {
					accum.Append ((char)inputStream.ReadChar ());
				}
			}
			return accum.ToString ();
		}

		private static char scanEscapeChar (char c)
		{
			switch (c) {
			case '"':
				return '"';
			case 'n':
				return '\n';
			case 'b':
				return '\b';
			case 'r':
				return '\r';
			case 't':
				return '\t';
			case '\\':
				return '\\';
			}
			return '0';
		}
	}
}

