using System;
using System.Text;

namespace Iodine
{
	public class MatchOperator : IMatcher
	{
		public bool IsMatchImpl (InputStream inputStream)
		{
			return IsOperator ((char)inputStream.PeekChar ());
		}

		public Token ScanToken (ErrorLog errLog, InputStream inputStream)
		{
			if (inputStream.MatchString (">>")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, ">>", inputStream);
			} else if (inputStream.MatchString ("<<")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "<<", inputStream);
			} else if (inputStream.MatchString ("&&")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "&&", inputStream);
			} else if (inputStream.MatchString ("||")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "||", inputStream);
			} else if (inputStream.MatchString ("==")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "==", inputStream);
			} else if (inputStream.MatchString ("!=")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "!=", inputStream);
			} else if (inputStream.MatchString ("<=")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "<=", inputStream);
			} else if (inputStream.MatchString (">=")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, ">=", inputStream);
			} else if (inputStream.MatchString ("=>")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "=>", inputStream);
			} else if (inputStream.MatchString ("--")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "--", inputStream);
			} else if (inputStream.MatchString ("++")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "++", inputStream);
			} else if (inputStream.MatchString ("+=")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "+=", inputStream);
			} else if (inputStream.MatchString ("-=")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "-=", inputStream);
			} else if (inputStream.MatchString ("/=")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "/=", inputStream);
			} else if (inputStream.MatchString ("*=")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "*=", inputStream);
			} else if (inputStream.MatchString ("%=")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "%=", inputStream);
			} else if (inputStream.MatchString ("^=")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "^=", inputStream);
			}  else if (inputStream.MatchString ("&=")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "&=", inputStream);
			} else if (inputStream.MatchString ("|=")) {
				inputStream.ReadChars(2);
				return Token.Create (TokenClass.Operator, "|=", inputStream);
			} else if (inputStream.MatchString ("<<=")) {
				inputStream.ReadChars(3);
				return Token.Create (TokenClass.Operator, "<<=", inputStream);
			} else if (inputStream.MatchString (">>=")) {
				inputStream.ReadChars(3);
				return Token.Create (TokenClass.Operator, ">>=", inputStream);
			} 

			return Token.Create (TokenClass.Operator, ((char)inputStream.ReadChar ()).ToString(), 
				inputStream);
		}

		private static bool IsOperator (char c)
		{
			return "+-*/.-=<>~!&^|%".Contains (c.ToString ());
		}
	}
}

