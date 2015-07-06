using System;
using System.Text;

namespace Iodine
{
	public class MatchStringLit : IMatcher
	{
		public bool IsMatchImpl (InputStream inputStream)
		{
			return inputStream.PeekChar () == '\"' || inputStream.PeekChar () == '\'';
		}

		public Token ScanToken (ErrorLog errLog, InputStream inputStream)
		{
			char quote = (char)inputStream.ReadChar ();
			TokenClass type = quote == '\"' ? TokenClass.InterpolatedStringLiteral : TokenClass.StringLiteral;
			string accum = scanUntil (quote, inputStream);
			if (inputStream.ReadChar () == -1) {
				errLog.AddError (ErrorType.LexerError, inputStream.Location, 
					"Unterminated string literal!");
				return null;
			} else {
				return Token.Create (type, accum, inputStream);
			}

		}

		private string scanUntil (char terminator, InputStream inputStream)
		{
			StringBuilder accum = new StringBuilder ();
			while (inputStream.PeekChar () != -1 && inputStream.PeekChar () != terminator) {
				if (inputStream.PeekChar () == '\\') {
					inputStream.ReadChar ();
					accum.Append (scanEscapeChar ((char)inputStream.ReadChar ()));
				} else {
					accum.Append ((char)inputStream.ReadChar ());
				}
			}
			return accum.ToString ();
		}

		private static char scanEscapeChar (char c)
		{
			switch (c) {
			case '"':
				return '"';
			case 'n':
				return '\n';
			case 'b':
				return '\b';
			case 'r':
				return '\r';
			case 't':
				return '\t';
			case '\\':
				return '\\';
			}
			return '0';
		}
	}
}

