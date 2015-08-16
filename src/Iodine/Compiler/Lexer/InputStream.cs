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

namespace Iodine.Compiler
{
	/// <summary>
	/// Input stream.
	/// </summary>
	public class InputStream
	{
		private int position;
		private int sourceLen;
		private string source;
		private string file;

		/// <summary>
		/// Gets the location.
		/// </summary>
		/// <value>The location.</value>
		public Location Location {
			private set;
			get;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Iodine.Compiler.InputStream"/> class.
		/// </summary>
		/// <param name="source">Input source code.</param>
		/// <param name="file">Input file name.</param>
		public InputStream (string source, string file)
		{
			this.source = source;
			this.position = 0;
			this.sourceLen = source.Length;
			this.file = file;
		}

		/// <summary>
		/// Skips over whitespace characters
		/// </summary>
		public void EatWhiteSpaces ()
		{
			while (char.IsWhiteSpace ((char)this.PeekChar ())) {
				this.ReadChar ();
			}
		}

		/// <summary>
		/// Returns true if the current substring at position matches str
		/// </summary>
		/// <returns><c>true</c>, if string was matched, <c>false</c> otherwise.</returns>
		/// <param name="str">String.</param>
		public bool MatchString (string str)
		{
			for (int i = 0; i < str.Length; i++) {
				if (PeekChar (i) != str [i]) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Reads n chars.
		/// </summary>
		/// <param name="n">How many characters to read.</param>
		public void ReadChars (int n)
		{
			for (int i = 0; i < n; i++) {
				ReadChar ();
			}
		}

		/// <summary>
		/// Reads a single character
		/// </summary>
		/// <returns>The char.</returns>
		public int ReadChar ()
		{
			if (position >= sourceLen) {
				return -1;
			}

			if (source [position] == '\n') {
				this.Location = new Location (this.Location.Line + 1, 0, this.file); 
			} else {
				this.Location = new Location (this.Location.Line, this.Location.Column + 1,
					this.file); 
			}
			return source [position++];
		}

		/// <summary>
		/// Returns the current character at position without advancing position
		/// </summary>
		/// <returns>The char.</returns>
		public int PeekChar ()
		{
			return PeekChar (0);
		}

		/// <summary>
		/// Returns the current character at position without advancing position
		/// </summary>
		/// <returns>The char.</returns>
		/// <param name="n">How far to peek.</param>
		public int PeekChar (int n)
		{
			if (position + n >= sourceLen) {
				return -1;
			}
			return source [position + n];
		}
	}
}

