using System;
using System.Collections.Generic;

namespace Iodine
{
	public class Lexer
	{
		private InputStream input;
		private ErrorLog errorLog;

		static List<IMatcher> matchers = new List<IMatcher> ();

		static Lexer ()
		{
			matchers.Add (new MatchKeyword ());
			matchers.Add (new MatchNumber ());
			matchers.Add (new MatchStringLit ());
			matchers.Add (new MatchGrouping ());
			matchers.Add (new MatchOperator ());
			matchers.Add (new MathComment ());
			matchers.Add (new MatchIdent ());
		}

		public Lexer (ErrorLog errorLog, string source, string file = "") 
		{
			this.errorLog = errorLog;
			this.input = new InputStream (source, file);
		}

		public TokenStream Scan ()
		{
			TokenStream retStream = new TokenStream (this.errorLog);
			this.input.EatWhiteSpaces ();
			while (this.input.PeekChar () != -1) {
				bool matchFound = false;
				foreach (IMatcher matcher in matchers) {
					if (matcher.IsMatchImpl (this.input)) {
						Token token = matcher.ScanToken (this.errorLog, this.input);
						if (token != null) {
							retStream.AddToken (token);
						}
						matchFound = true;
						break;
					}
				}

				if (!matchFound) {
					errorLog.AddError (ErrorType.LexerError, this.input.Location, "Unexpected '{0}'", 
						(char)this.input.ReadChar ());
				}

				this.input.EatWhiteSpaces ();
			}
			return retStream;
		}
	}
}

