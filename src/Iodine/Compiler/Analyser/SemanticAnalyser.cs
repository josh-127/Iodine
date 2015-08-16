using System;
using Iodine.Compiler.Ast;

namespace Iodine.Compiler
{
	public class SemanticAnalyser
	{
		private ErrorLog errorLog;

		public SemanticAnalyser (ErrorLog errorLog)
		{
			this.errorLog = errorLog;
		}

		public SymbolTable Analyse (AstRoot ast)
		{
			SymbolTable retTable = new SymbolTable ();
			RootVisitor visitor = new RootVisitor (errorLog, retTable);
			ast.Visit (visitor);
			return retTable;
		}
	}
}

