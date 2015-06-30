using System;
using System.IO;
namespace Iodine
{
	public class ModuleCompiler : IAstVisitor
	{
		private ErrorLog errorLog;
		private SymbolTable symbolTable;
		private int currentScope = 0;
		private IodineModule module;
		private FunctionCompiler functionCompiler;
		private string file;

		public ModuleCompiler (ErrorLog errorLog, SymbolTable symbolTable, IodineModule module,
			string file)
		{
			this.errorLog = errorLog;
			this.symbolTable = symbolTable;
			this.module = module;
			this.file = file;
			this.functionCompiler = new FunctionCompiler (errorLog, symbolTable, module.Initializer);
		}

		public void Accept (AstNode ast)
		{
			this.visitSubnodes (ast);
		}

		public void Accept (Ast ast)
		{
			visitSubnodes (ast);
		}

		public void Accept (NodeExpr expr)
		{
			expr.Visit (functionCompiler);
		}

		public void Accept (NodeStmt stmt)
		{
			stmt.Visit (functionCompiler);
		}

		public void Accept (NodeBinOp binop)
		{
			binop.Visit (functionCompiler);
		}

		public void Accept (NodeUnaryOp unaryop)
		{
			unaryop.Visit (functionCompiler);
		}

		public void Accept (NodeIdent ident)
		{
			ident.Visit (functionCompiler);
		}

		public void Accept (NodeCall call)
		{
			call.Visit (functionCompiler);
		}
			
		public void Accept (NodeArgList arglist)
		{
			arglist.Visit (functionCompiler);
		}

		public void Accept (NodeGetAttr getAttr)
		{
			getAttr.Visit (functionCompiler);
		}

		public void Accept (NodeInteger integer)
		{
			integer.Visit (functionCompiler);
		}

		public void Accept (NodeFloat num)
		{
			num.Visit (functionCompiler);
		}

		public void Accept (NodeString str)
		{
			str.Visit (functionCompiler);
		}

		public void Accept (NodeIfStmt ifStmt)
		{
			ifStmt.Visit (functionCompiler);
		}

		public void Accept (NodeWhileStmt whileStmt)
		{
			whileStmt.Visit (functionCompiler);
		}

		public void Accept (NodeForStmt forStmt) 
		{
			forStmt.Visit (functionCompiler);
		}

		public void Accept (NodeForeach foreachStmt)
		{
			foreachStmt.Visit (functionCompiler);
		}

		public void Accept (NodeTuple tuple)
		{
			tuple.Visit (functionCompiler);
		}

		public void Accept (NodeContinue cont)
		{
			cont.Visit (functionCompiler);
		}

		public void Accept (NodeFuncDecl funcDecl)
		{
			module.AddMethod (compileMethod (funcDecl));
		}

		public void Accept (NodeScope scope)
		{
			visitSubnodes (scope);
		}

		public void Accept (NodeUseStatement useStmt)
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

		public void Accept (NodeClassDecl classDecl)
		{
			IodineClass clazz = new IodineClass (classDecl.Name, compileMethod (classDecl.Constructor));

			for (int i = 1; i < classDecl.Children.Count; i++) {
				NodeFuncDecl func = classDecl.Children [i] as NodeFuncDecl;
				if (func.InstanceMethod)
					clazz.AddInstanceMethod (compileMethod (func));
				else
					clazz.SetAttribute (func.Name, compileMethod (func));
			}

			module.SetAttribute (clazz.Name, clazz);
		}

		public void Accept (NodeConstant constant)
		{
			if (constant.Value is NodeString)
				module.SetAttribute (constant.Name, new IodineString (((NodeString)constant.Value
					).Value));
			else if (constant.Value is NodeInteger)
				module.SetAttribute (constant.Name, new IodineInteger (((NodeInteger)constant.Value
					).Value));
		}

		public void Accept (NodeReturnStmt returnStmt)
		{
			returnStmt.Visit (functionCompiler);
		}

		public void Accept (NodeIndexer indexer)
		{
			indexer.Visit (functionCompiler);
		}

		public void Accept (NodeList list)
		{
			list.Visit (functionCompiler);
		}

		public void Accept (NodeSelf self)
		{
			self.Visit (functionCompiler);
		}

		public void Accept (NodeTrue ntrue)
		{
			ntrue.Visit (functionCompiler);
		}

		public void Accept (NodeFalse nfalse) 
		{
			nfalse.Visit (functionCompiler);
		}

		public void Accept (NodeNull nil)
		{
			nil.Visit (functionCompiler);
		}

		public void Accept (NodeLambda lambda)
		{
			lambda.Visit (functionCompiler);
		}

		public void Accept (NodeTryExcept tryExcept)
		{
			tryExcept.Visit (functionCompiler);
		}

		public void Accept (NodeBreak brk)
		{
			brk.Visit (functionCompiler);
		}

		private void visitSubnodes (AstNode root)
		{
			foreach (AstNode node in root) {
				node.Visit (this);
			}
		}

		private IodineMethod compileMethod (NodeFuncDecl funcDecl) 
		{
			symbolTable.CurrentScope = symbolTable.CurrentScope.ChildScopes[currentScope++];
			IodineMethod methodBuilder = new IodineMethod (module, funcDecl.Name, funcDecl.InstanceMethod,
				funcDecl.Parameters.Count, symbolTable.CurrentScope.SymbolCount);
			FunctionCompiler compiler = new FunctionCompiler (this.errorLog, this.symbolTable, 
				methodBuilder);
			methodBuilder.Variadic = funcDecl.Variadic;
			for (int i = 0; i < funcDecl.Parameters.Count; i++) {
				methodBuilder.Parameters [funcDecl.Parameters [i]] = symbolTable.GetSymbol
					(funcDecl.Parameters[i]).Index;
			}
			compiler.Accept (funcDecl.Children[0]);
			symbolTable.CurrentScope = symbolTable.CurrentScope.ParentScope;
			methodBuilder.EmitInstruction (Opcode.LoadNull);
			methodBuilder.FinalizeLabels ();

			return methodBuilder;
		}
	}
}

