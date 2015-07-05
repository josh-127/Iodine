using System;
using System.IO;

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
			ModuleCompiler compiler = new ModuleCompiler (errorLog, symbolTable, module);
			ast.Visit (compiler);
			module.Initializer.FinalizeLabels ();

			foreach (AstNode node in ast) {
				if (node is NodeUseStatement) {
					compileUseStatement (module, node as NodeUseStatement);
				}
			}

			return module;
		}

		private void compileUseStatement (IodineModule module, NodeUseStatement useStmt) 
		{
			module.Imports.Add (useStmt.Module);
			IodineModule import = !useStmt.Relative ? IodineModule.LoadModule (errorLog, useStmt.Module) :
				IodineModule.LoadModule (errorLog, String.Format ("{0}{1}{2}", Path.GetDirectoryName (this.file),
					Path.DirectorySeparatorChar, useStmt.Module));
			if (import != null) {
				module.SetAttribute (System.IO.Path.GetFileNameWithoutExtension (useStmt.Module), import);
				if (useStmt.Wildcard) {
					foreach (string attr in import.Attributes.Keys) {
						module.SetAttribute (attr, import.GetAttribute (attr));
					}
				} else {
					foreach (string item in useStmt.Imports) {
						if (import.HasAttribute (item)) {
							module.SetAttribute (item, import.GetAttribute (item));
						} else {
							errorLog.AddError (ErrorType.ParserError, "Could not import {0} from {1}", 
								item, useStmt.Module);
						}
					}
				}
			} else {
				errorLog.AddError (ErrorType.ParserError, "Could not import module {0}", useStmt.Module);
			}
		}
	}
}

