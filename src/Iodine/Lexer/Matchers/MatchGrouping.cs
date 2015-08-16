using System;
using System.Text;

namespace Iodine.Compiler
{
	public class MatchGrouping : IMatcher
	{
		public bool IsMatchImpl (InputStream inputStream)
		{
			return IsGrouping ((char)inputStream.PeekChar ());
		}

		public Token ScanToken (ErrorLog errLog, InputStream inputStream)
		{
			switch ((char)inputStream.ReadChar ()) {
			case '{':
				return Token.Create (TokenClass.OpenBrace, inputStream);
			case '}':
				return Token.Create (TokenClass.CloseBrace, inputStream);
			case '(':
				return Token.Create (TokenClass.OpenParan, inputStream);
			case ')':
				return Token.Create (TokenClass.CloseParan, inputStream);
			case '[':
				return Token.Create (TokenClass.OpenBracket, inputStream);
			case ']':
				return Token.Create (TokenClass.CloseBracket, inputStream);
			case ';':
				return Token.Create (TokenClass.SemiColon, inputStream);
			case ':':
				return Token.Create (TokenClass.Colon, inputStream);
			case ',':
				return Token.Create (TokenClass.Comma, inputStream);
			case '.':
				return Token.Create (TokenClass.Dot, inputStream);
			default:
				return null;
			}
		}

		private static bool IsGrouping (char c)
		{
			return "{}()[];:,.".Contains (c.ToString ());
		}
	}
}

