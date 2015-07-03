using System;

namespace Iodine
{
	public class Token
	{
		public TokenClass Class {
			private set;
			get;
		}

		public string Value {
			private set;
			get;
		}

		public int Line {
			private set;
			get;
		}

		public int Column {
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

		public override string ToString ()
		{
			return this.Value.ToString ();
		}

		public static string ClassToString (TokenClass clazz)
		{
			switch (clazz) {
			case TokenClass.CloseBrace:
				return "}";
			case TokenClass.OpenBrace:
				return "{";
			case TokenClass.CloseParan:
				return ")";
			case TokenClass.OpenParan:
				return "(";
			case TokenClass.Dot:
				return ".";
			case TokenClass.Comma:
				return ",";
			case TokenClass.OpenBracket:
				return "[";
			case TokenClass.CloseBracket:
				return "]";
			case TokenClass.SemiColon:
				return ";";
			case TokenClass.Colon:
				return ":";
			default:
				return clazz.ToString ();
			}
		}

		public static Token Create (TokenClass clazz, InputStream stream) 
		{
			return new Token (clazz, ClassToString (clazz), stream.Line, stream.Column);
		}

		public static Token Create (TokenClass clazz, string value, InputStream stream) 
		{
			return new Token (clazz, value, stream.Line, stream.Column);
		}
	}
}

