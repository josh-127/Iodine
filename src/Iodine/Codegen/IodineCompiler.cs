using System;

namespace Iodine
{
	public class IodineCompiler
	{
		private ErrorLog errorLog;
		private SymbolTable symbolTable;
		private string file ;

		public IodineCompiler (ErrorLog errLog, SymbolTable symbolTable, string file)
		{
			this.errorLog = errLog;
			this.symbolTable = symbolTable;
			this.file = file;
		}

		public IodineModule CompileAst (IodineModule module, Ast ast)
		{
			ModuleCompiler compiler = new ModuleCompiler (errorLog, symbolTable, module, file);
			ast.Visit (compiler);
			module.Initializer.FinalizeLabels ();
			return module;
		}
	}
}

