using System;
using System.Collections.Generic;

namespace Iodine
{
	public class TokenStream
	{
		private int position = 0;
		private ErrorLog errorLog;
		private List<Token> tokens = new List<Token> ();

		public bool EndOfStream
		{
			get
			{
				return this.tokens.Count <= position;
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
				errorLog.AddError (ErrorType.ParserError, "Unexpected '{0}' at {1}:{2} (Expected '{3}')",
					offender.ToString (), offender.Column, offender.Line, clazz.ToString ());
			} else {
				errorLog.AddError (ErrorType.ParserError, "Unexpected end of file (Expected {0})",
					clazz.ToString ());
			}
			return null;
		}

		public Token Expect (TokenClass clazz, string val)
		{
			Token ret = peekToken ();
			if (Accept (clazz, val)) {
				return ret;
			}

			Token offender = readToken ();

			if (offender != null) {
				errorLog.AddError (ErrorType.ParserError, "Unexpected '{0}' at {1}:{2} (Expected '{3}')",
					offender.ToString (), offender.Column, offender.Line, clazz.ToString ());
			} else {
				errorLog.AddError (ErrorType.ParserError, "Unexpected end of file (Expected {1})",
					clazz.ToString ());
			}
			return null;
		}

		private Token peekToken () 
		{
			if (this.position >= this.tokens.Count) {
				return null;
			}
			return this.tokens [this.position];
		}

		private Token readToken () 
		{
			if (this.position >= this.tokens.Count) {
				return null;
			}
			return this.tokens [this.position++];
		}
	}
}

