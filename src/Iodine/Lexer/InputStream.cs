using System;

namespace Iodine
{
	public class InputStream
	{
		private int position;
		private int sourceLen;
		private string source;

		public int Line
		{
			private set;
			get;
		}

		public int Column
		{
			private set;
			get;
		}

		public InputStream (string source)
		{
			this.source = source;
			this.position = 0;
			this.sourceLen = source.Length;
		}

		public void EatWhiteSpaces ()
		{
			while (char.IsWhiteSpace ((char)this.PeekChar ())) {
				this.ReadChar ();
			}
		}

		public bool MatchString (string str) 
		{
			for (int i = 0; i < str.Length; i++) {
				if (PeekChar (i) != str [i]) {
					return false;
				}
			}
			return true;
		}

		public void ReadChars (int n)
		{
			for (int i = 0; i < n; i++) {
				ReadChar ();
			}
		}

		public int ReadChar ()
		{
			if (position >= sourceLen) {
				return -1;
			}

			if (source[position] == '\n') {
				this.Line++;
				this.Column = 0;
			} else {
				this.Column++;
			}
			return source[position++];
		}

		public int PeekChar ()
		{
			return PeekChar(0);
		}

		public int PeekChar (int n)
		{
			if (position + n >= sourceLen) {
				return -1;
			}
			return source[position + n];
		}
	}
}

