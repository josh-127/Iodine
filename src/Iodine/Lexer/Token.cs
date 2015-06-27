using System;

namespace Iodine
{
	public class Token
	{
		public TokenClass Class
		{
			private set;
			get;
		}

		public string Value
		{
			private set;
			get;
		}

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

		public Token (TokenClass clazz, string value, int line, int col)
		{
			this.Class = clazz;
			this.Value = value;
			this.Line = line;
			this.Column = Column;
		}

		public static Token Create (TokenClass clazz, InputStream stream) 
		{
			return new Token (clazz, null, stream.Line, stream.Column);
		}

		public static Token Create (TokenClass clazz, string value, InputStream stream) 
		{
			return new Token (clazz, value, stream.Line, stream.Column);
		}
	}
}

