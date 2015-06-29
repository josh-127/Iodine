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

			if (((char)inputStream.PeekChar ()) == '.') {
				return scanFloat (accum, inputStream);
			}

			return Token.Create (TokenClass.IntLiteral, accum.ToString (), inputStream);

		}

		private Token scanFloat (StringBuilder accum, InputStream stream)
		{
			accum.Append ((char)stream.ReadChar ());
			while (IsNum ((char)stream.PeekChar ())) {
				accum.Append ((char)stream.ReadChar ());
			}
			return Token.Create (TokenClass.FloatLiteral, accum.ToString (), stream);
		}

		private static bool IsNum (char c)
		{
			return char.IsDigit (c);
		}
	}
}

