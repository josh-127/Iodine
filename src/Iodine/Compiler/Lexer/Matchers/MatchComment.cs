using System;
using System.Text;

namespace Iodine.Compiler
{
	public class MathComment : IMatcher
	{
		public bool IsMatchImpl (InputStream inputStream)
		{
			return (char)inputStream.PeekChar () == '#';
		}

		public Token ScanToken (ErrorLog errLog, InputStream inputStream)
		{
			int ch = inputStream.ReadChar ();
			while (ch != -1 && ch != '\n') {
				ch = inputStream.ReadChar ();
			}
			return null;
		}

		private static bool IsIdentChar (char c)
		{
			return char.IsLetterOrDigit (c) || c == '_';
		}
	}
}

