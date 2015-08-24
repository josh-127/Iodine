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
	public class MatchKeyword : IMatcher
	{
		public bool IsMatchImpl (InputStream inputStream)
		{
			return isKeyword (inputStream);
		}

		public Token ScanToken (ErrorLog errLog, InputStream inputStream)
		{
			StringBuilder accum = new StringBuilder ();
			while (char.IsLetter ((char)inputStream.PeekChar ())) {
				accum.Append ((char)inputStream.ReadChar ());
			}

			return Token.Create (TokenClass.Keyword, accum.ToString (), inputStream);
		}

		private static bool isKeyword (InputStream inputStream)
		{
			return matchString(inputStream, "if") ||
				matchString (inputStream, "else") ||
				matchString (inputStream, "while") ||
				matchString (inputStream, "do") ||
				matchString (inputStream, "for") ||
				matchString (inputStream, "func") ||
				matchString (inputStream, "class") ||
				matchString (inputStream, "use") ||
				matchString (inputStream, "self") ||
				matchString (inputStream, "foreach") ||
				matchString (inputStream, "in") ||
				matchString (inputStream, "true") ||
				matchString (inputStream, "false") ||
				matchString (inputStream, "null") ||
				matchString (inputStream, "lambda") ||
				matchString (inputStream, "try") ||
				matchString (inputStream, "except") ||
				matchString (inputStream, "break") ||
				matchString (inputStream, "from") ||
				matchString (inputStream, "continue") ||
				matchString (inputStream, "params") ||
				matchString (inputStream, "keyword") ||
				matchString (inputStream, "super") ||
				matchString (inputStream, "is") ||
				matchString (inputStream, "isnot") ||
				matchString (inputStream, "as") ||
				matchString (inputStream, "enum") ||
				matchString (inputStream, "raise") ||
				matchString (inputStream, "interface") ||
				matchString (inputStream, "switch") ||
				matchString (inputStream, "case") ||
				matchString (inputStream, "yield") ||
				matchString (inputStream, "default") ||
				matchString (inputStream, "return");
		}

		private static bool matchString (InputStream inputStream, string str)
		{
			for (int i = 0; i < str.Length; i++) {
				if (((char)inputStream.PeekChar (i)) != str[i]) {
					return false;
				}
			}
			return !char.IsLetterOrDigit ((char)inputStream.PeekChar (str.Length));
		}
	}
}

