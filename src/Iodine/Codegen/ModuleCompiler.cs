using System;

namespace Iodine
{
	public class ModuleCompiler : IAstVisitor
	{
		private ErrorLog errorLog;
		private SymbolTable symbolTable;
		private int currentScope = 0;
		private IodineModule module;

		public ModuleCompiler (ErrorLog errorLog, SymbolTable symbolTable, IodineModule module)
		{
			this.errorLog = errorLog;
			this.symbolTable = symbolTable;
			this.module = module;
		}

		public void Accept (AstNode ast)
		{
			this.visitSubnodes (ast);
		}

		public void Accept (Ast ast)
		{
			visitSubnodes (ast);
		}

		public void Accept (NodeExpr expr) { }
		public void Accept (NodeStmt stmt) { }
		public void Accept (NodeBinOp binop) { }
		public void Accept (NodeUnaryOp unaryop) { }
		public void Accept (NodeIdent ident) { }
		public void Accept (NodeCall call) { }
		public void Accept (NodeArgList arglist) { }
		public void Accept (NodeGetAttr getAttr) { }
		public void Accept (NodeInteger integer) { }
		public void Accept (NodeString str) { }
		public void Accept (NodeIfStmt ifStmt) { }
		public void Accept (NodeWhileStmt whileStmt) { }
		public void Accept (NodeForStmt forStmt) { }
		public void Accept (NodeForeach foreachStmt) { }
		public void Accept (NodeTuple tuple) { }

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
			IodineModule import = IodineModule.LoadModule (errorLog, useStmt.Module);
			if (import != null) {
				module.SetAttribute (useStmt.Module, import);
				foreach (string item in useStmt.Imports) {
					if (import.HasAttribute (item)) {
						module.SetAttribute (item, import.GetAttribute (item));
					} else {
						errorLog.AddError (ErrorType.ParserError, "Could not import {0} from {1}", 
							item, useStmt.Module);
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

		public void Accept (NodeReturnStmt returnStmt) { }
		public void Accept (NodeIndexer indexer) { }
		public void Accept (NodeList list) { }
		public void Accept (NodeSelf self) { }
		public void Accept (NodeTrue ntrue) { }
		public void Accept (NodeFalse nfalse) { }
		public void Accept (NodeNull nil) { }
		public void Accept (NodeLambda lambda) { }
		public void Accept (NodeTryExcept tryExcept) {}
		public void Accept (NodeBreak brk) { }

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

