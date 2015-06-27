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
			return (Ast)Ast.Parse (this.tokenStream);
		}
	}
}

