using System;
using System.Text;

namespace Iodine
{
	public class MatchStringLit : IMatcher
	{
		public bool IsMatchImpl (InputStream inputStream)
		{
			return inputStream.PeekChar () == '\"';
		}

		public Token ScanToken (ErrorLog errLog, InputStream inputStream)
		{
			StringBuilder accum = new StringBuilder ();
			inputStream.ReadChar ();
			while (inputStream.PeekChar () != -1 && inputStream.PeekChar () != '\"') {
				accum.Append ((char)inputStream.ReadChar ());
			}

			if (inputStream.ReadChar () == -1) {
				errLog.AddError (ErrorType.LexerError, "Unterminated string literal!");
				return null;
			} else {
				return Token.Create (TokenClass.StringLiteral, accum.ToString (), inputStream);
			}

		}
	}
}

