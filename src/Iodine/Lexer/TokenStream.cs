using System;
using System.Collections.Generic;

namespace Iodine
{
	public class TokenStream
	{
		private ErrorLog errorLog;
		private List<Token> tokens = new List<Token> ();

		public bool EndOfStream {
			get {
				return this.tokens.Count <= Position;
			}
		}

		public int Position {
			private set;
			get;
		}

		public Location Location {
			get {
				if (this.peekToken () != null)
					return this.peekToken ().Location;
				else {
					return this.peekToken (-1).Location;
				}
			}
		}

		public ErrorLog ErrorLog {
			get {
				return this.errorLog;
			}
		}

		public TokenStream (ErrorLog errorLog)
		{
			this.errorLog = errorLog;
		}

		public void AddToken (Token token)
		{
			this.tokens.Add (token);
		}

		public bool Match (TokenClass clazz)
		{
			return peekToken () != null && peekToken ().Class == clazz;
		}

		public bool Match (TokenClass clazz1, TokenClass clazz2)
		{
			return peekToken () != null && peekToken ().Class == clazz1 &&
				peekToken (1) != null && peekToken (1).Class == clazz2;
		}

		public bool Match (TokenClass clazz, string val)
		{
			return peekToken () != null && peekToken ().Class == clazz &&
				peekToken ().Value == val;
		}

		public bool Accept (TokenClass clazz)
		{
			if (peekToken () != null && peekToken ().Class == clazz) {
				readToken ();
				return true;
			}
			return false;
		}

		public bool Accept (TokenClass clazz, ref Token token)
		{
			if (peekToken () != null && peekToken ().Class == clazz) {
				token = readToken ();
				return true;
			}
			return false;
		}

		public bool Accept (TokenClass clazz, string val) 
		{
			if (peekToken () != null && peekToken ().Class == clazz && peekToken ().Value == val) {
				readToken ();
				return true;
			}
			return false;
		}

		public Token Expect (TokenClass clazz)
		{
			Token ret = null;
			if (Accept (clazz, ref ret)) {
				return ret;
			}

			Token offender = readToken ();
		
			if (offender != null) {
				errorLog.AddError (ErrorType.ParserError, offender.Location, "Unexpected '{0}' (Expected '{1}')",
					offender.ToString (), Token.ClassToString (clazz));
			} else {
				errorLog.AddError (ErrorType.ParserError, offender.Location, "Unexpected end of file (Expected {0})",
					Token.ClassToString (clazz));
				throw new Exception ("");
			}
			return new Token (clazz, "", Location);
		}

		public Token Expect (TokenClass clazz, string val)
		{
			Token ret = peekToken ();
			if (Accept (clazz, val)) {
				return ret;
			}

			Token offender = readToken ();

			if (offender != null) {
				errorLog.AddError (ErrorType.ParserError, offender.Location, 
					"Unexpected '{0}' (Expected '{1}')", offender.ToString (), Token.ClassToString (
						clazz));
			} else {
				errorLog.AddError (ErrorType.ParserError, offender.Location, 
					"Unexpected end of file (Expected {0})", Token.ClassToString (clazz));
				throw new Exception ("");
			}
			return new Token (clazz, "", Location);
		}

		public void MakeError ()
		{
			this.errorLog.AddError (ErrorType.ParserError, peekToken ().Location, "Unexpected {0}",
				readToken ().ToString ());
		}

		private Token peekToken () 
		{
			return peekToken (0);
		}

		private Token peekToken (int n) 
		{
			if (this.Position + n >= this.tokens.Count) {
				return null;
			}
			return this.tokens [this.Position + n];
		}

		private Token readToken () 
		{
			if (this.Position >= this.tokens.Count) {
				return null;
			}
			return this.tokens [this.Position++];
		}
	}
}

