using System;

namespace Iodine
{
	public class SemanticAnalyser
	{
		private ErrorLog errorLog;

		public SemanticAnalyser (ErrorLog errorLog)
		{
			this.errorLog = errorLog;
		}

		public SymbolTable Analyse (Ast ast)
		{
			SymbolTable retTable = new SymbolTable ();
			RootVisitor visitor = new RootVisitor (errorLog, retTable);
			ast.Visit (visitor);
			return retTable;
		}
	}
}

