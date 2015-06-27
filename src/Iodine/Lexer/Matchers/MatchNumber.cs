using System;
using System.Text;

namespace Iodine
{
	public class MatchNumber : IMatcher
	{
		public bool IsMatchImpl (InputStream inputStream)
		{
			return IsNum ((char)inputStream.PeekChar ());
		}

		public Token ScanToken (ErrorLog errLog, InputStream inputStream)
		{
			StringBuilder accum = new StringBuilder ();
			while (IsNum ((char)inputStream.PeekChar ())) {
				accum.Append ((char)inputStream.ReadChar ());
			}

			string val = accum.ToString ();

			if (val.Contains (".")) {
				return Token.Create (TokenClass.FloatLiteral, val, inputStream);
			}

			return Token.Create (TokenClass.IntLiteral, val, inputStream);

		}

		private static bool IsNum (char c)
		{
			return char.IsDigit (c);
		}
	}
}

