using System;
using System.IO;
using System.Collections.Generic;
using Iodine.Compiler.Ast;

namespace Iodine.Compiler
{
	public class IodineCompiler
	{
		private static List<IBytecodeOptimization> Optimizations = new List<IBytecodeOptimization> ();

		static IodineCompiler ()
		{
			Optimizations.Add (new ControlFlowOptimization ());
			Optimizations.Add (new InstructionOptimization ());
		}

		private ErrorLog errorLog;
		private SymbolTable symbolTable;
		private string file;

		public IodineCompiler (ErrorLog errLog, SymbolTable symbolTable, string file)
		{
			this.errorLog = errLog;
			this.symbolTable = symbolTable;
			this.file = file;
		}

		public IodineModule CompileAst (IodineModule module, AstRoot ast)
		{
			ModuleCompiler compiler = new ModuleCompiler (errorLog, symbolTable, module);
			ast.Visit (compiler);
			module.Initializer.FinalizeLabels ();

			foreach (AstNode node in ast) {
				if (node is NodeUseStatement) {
					compileUseStatement (module, node as NodeUseStatement);
				}
			}

			optimizeObject (module);	
			return module;
		}

		private void optimizeObject (IodineObject obj)
		{
			foreach (IodineObject attr in obj.Attributes.Values) {
				if (attr is IodineMethod) {
					IodineMethod method = attr as IodineMethod;
					foreach (IBytecodeOptimization opt in Optimizations) {
						opt.PerformOptimization (method);
					}
				}
			}
		}

		private void compileUseStatement (IodineModule module, NodeUseStatement useStmt)
		{
		}
	}
}

