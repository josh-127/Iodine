using System;
using Iodine.Compiler.Ast;

namespace Iodine.Compiler
{
	public class Parser
	{
		private TokenStream tokenStream;

		public Parser (TokenStream tokenStream)
		{
			this.tokenStream = tokenStream;
		}

		public AstRoot Parse ()
		{
			try {
				return AstRoot.Parse (this.tokenStream);
			} catch (Exception) {
				//this.tokenStream.ErrorLog.AddError (ErrorType.ParserError, "");
				return new AstRoot (this.tokenStream.Location);
			}
		}
	}
}

