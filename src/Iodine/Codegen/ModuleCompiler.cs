using System;
using System.IO;
using System.Collections.Generic;

namespace Iodine
{
	public class ModuleCompiler : IAstVisitor
	{
		private ErrorLog errorLog;
		private SymbolTable symbolTable;
		private IodineModule module;
		private FunctionCompiler functionCompiler;

		public ModuleCompiler (ErrorLog errorLog, SymbolTable symbolTable, IodineModule module)
		{
			this.errorLog = errorLog;
			this.symbolTable = symbolTable;
			this.module = module;
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
			visitSubnodes (expr);
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
			scope.Visit (functionCompiler);
		}

		public void Accept (NodeUseStatement useStmt)
		{
			module.Imports.Add (useStmt.Module);
			string import = !useStmt.Relative ? useStmt.Module : String.Format ("{0}{1}{2}",
				                Path.GetDirectoryName (useStmt.Location.File), Path.DirectorySeparatorChar, useStmt.Module);
			
			if (useStmt.Wildcard) {
				module.Initializer.EmitInstruction (Opcode.ImportAll, module.DefineConstant (
					new IodineName (import)));
			} else {
				IodineObject[] items = new IodineObject [useStmt.Imports.Count];
				for (int i = 0; i < items.Length; i++) {
					items [i] = new IodineString (useStmt.Imports [i]);
				}
				module.Initializer.EmitInstruction (Opcode.LoadConst, module.DefineConstant (
					new IodineTuple (items)));
				module.Initializer.EmitInstruction (Opcode.ImportFrom, module.DefineConstant (
					new IodineName (import)));
				module.Initializer.EmitInstruction (Opcode.Import, module.DefineConstant (
					new IodineName (import)));
			}
			
		}

		public void Accept (NodeClassDecl classDecl)
		{
			module.SetAttribute (classDecl.Name, CompileClass (classDecl));
		}

		public void Accept (NodeInterfaceDecl contractDecl)
		{
			IodineInterface contract = new IodineInterface (contractDecl.Name);
			foreach (AstNode node in contractDecl.Children) {
				NodeFuncDecl decl = node as NodeFuncDecl;
				contract.AddMethod (new IodineMethod (module, decl.Name, decl.InstanceMethod,
					decl.Parameters.Count, 0));
			}
			module.SetAttribute (contractDecl.Name, contract);
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

		public void Accept (NodeRaiseStmt raise)
		{
			raise.Value.Visit (this);
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

		public void Accept (NodeSuperCall super)
		{
		}


		public void Accept (NodeEnumDecl enumDecl)
		{
			IodineEnum ienum = new IodineEnum (enumDecl.Name);
			foreach (string name in enumDecl.Items.Keys) {
				ienum.AddItem (name, enumDecl.Items [name]);
			}
			this.module.SetAttribute (enumDecl.Name, ienum);
		}

		public IodineClass CompileClass (NodeClassDecl classDecl)
		{
			IodineClass clazz = new IodineClass (classDecl.Name, compileMethod (classDecl.Constructor));

			for (int i = 1; i < classDecl.Children.Count; i++) {
				if (classDecl.Children [i] is NodeFuncDecl) {
					NodeFuncDecl func = classDecl.Children [i] as NodeFuncDecl;
					if (func.InstanceMethod)
						clazz.AddInstanceMethod (compileMethod (func));
					else
						clazz.SetAttribute (func.Name, compileMethod (func));
				} else if (classDecl.Children [i] is NodeClassDecl) {
					NodeClassDecl subclass = classDecl.Children [i] as NodeClassDecl;
					clazz.SetAttribute (subclass.Name, CompileClass (subclass));
				}
			}
			return clazz;
		}

		private IodineMethod compileMethod (NodeFuncDecl funcDecl)
		{
			symbolTable.NextScope ();
			IodineMethod methodBuilder = new IodineMethod (module, funcDecl.Name, funcDecl.InstanceMethod,
				                             funcDecl.Parameters.Count, symbolTable.CurrentScope.SymbolCount);
			FunctionCompiler compiler = new FunctionCompiler (this.errorLog, this.symbolTable, 
				                            methodBuilder);
			methodBuilder.Variadic = funcDecl.Variadic;
			for (int i = 0; i < funcDecl.Parameters.Count; i++) {
				methodBuilder.Parameters [funcDecl.Parameters [i]] = symbolTable.GetSymbol
					(funcDecl.Parameters [i]).Index;
			}
			funcDecl.Children [0].Visit (compiler);
			methodBuilder.EmitInstruction (Opcode.LoadNull);
			methodBuilder.FinalizeLabels ();
			symbolTable.LeaveScope ();
			return methodBuilder;
		}
	}
}

