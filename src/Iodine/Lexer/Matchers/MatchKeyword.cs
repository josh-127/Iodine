using System;
using System.Text;

namespace Iodine
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
				matchString (inputStream, "super") ||
				matchString (inputStream, "is") ||
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

