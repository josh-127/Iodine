using System;

namespace Iodine
{
	public class IodineCompiler
	{
		private ErrorLog errorLog;
		private SymbolTable symbolTable;

		public IodineCompiler (ErrorLog errLog, SymbolTable symbolTable)
		{
			this.errorLog = errLog;
			this.symbolTable = symbolTable;
		}

		public IodineModule CompileAst (Ast ast)
		{
			IodineModule module = new IodineModule ("");
			ModuleCompiler compiler = new ModuleCompiler (errorLog, symbolTable, module);
			ast.Visit (compiler);
			module.Initializer.FinalizeLabels ();
			return module;
		}
	}
}

