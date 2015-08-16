using System;
using System.Text;

namespace Iodine.Compiler
{
	public class MatchIdent : IMatcher
	{
		public bool IsMatchImpl (InputStream inputStream)
		{
			return char.IsLetter ((char)inputStream.PeekChar ()) ||
				(char)inputStream.PeekChar () == '_';
		}

		public Token ScanToken (ErrorLog errLog, InputStream inputStream)
		{
			StringBuilder accum = new StringBuilder ();
			while (IsIdentChar ((char)inputStream.PeekChar ())) {
				accum.Append ((char)inputStream.ReadChar ());
			}

			return Token.Create (TokenClass.Identifier, accum.ToString (), inputStream);
		}

		private static bool IsIdentChar (char c)
		{
			return char.IsLetterOrDigit (c) || c == '_';
		}
	}
}

