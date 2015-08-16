using System;

namespace Iodine.Compiler
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

		public Location Location {
			private set;
			get;
		}

		public Token (TokenClass clazz, string value, Location location)
		{
			this.Class = clazz;
			this.Value = value;
			this.Location = location;
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
			return new Token (clazz, ClassToString (clazz), stream.Location);
		}

		public static Token Create (TokenClass clazz, string value, InputStream stream) 
		{
			return new Token (clazz, value, stream.Location);
		}
	}
}

