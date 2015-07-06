using System;

namespace Iodine
{
	public class Parser
	{
		private TokenStream tokenStream;

		public Parser (TokenStream tokenStream)
		{
			this.tokenStream = tokenStream;
		}

		public Ast Parse ()
		{
			try {
				return (Ast)Ast.Parse (this.tokenStream);
			} catch (Exception) {
				//this.tokenStream.ErrorLog.AddError (ErrorType.ParserError, "");
				return new Ast (this.tokenStream.Location);
			}
		}
	}
}

