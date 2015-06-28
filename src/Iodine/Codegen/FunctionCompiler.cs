using System;
using System.Collections.Generic;

namespace Iodine
{
	public class FunctionCompiler : IAstVisitor
	{
		private ErrorLog errorLog;
		private SymbolTable symbolTable;
		private IodineMethod methodBuilder;
		private int currentScope = 0;
		private Stack<IodineLabel> breakLabels = new Stack<IodineLabel>();

		public FunctionCompiler (ErrorLog errorLog, SymbolTable symbolTable, IodineMethod methodBuilder)
		{
			this.errorLog = errorLog;
			this.symbolTable = symbolTable;
			this.methodBuilder = methodBuilder;
		}

		public FunctionCompiler (ErrorLog errorLog, SymbolTable symbolTable, IodineMethod methodBuilder,
			Stack<IodineLabel> breakLabels)
		{
			this.errorLog = errorLog;
			this.symbolTable = symbolTable;
			this.methodBuilder = methodBuilder;
			this.breakLabels = breakLabels;
		}

		public void Accept (AstNode ast)
		{
			visitSubnodes (ast);
		}

		public void Accept (Ast ast)
		{
			visitSubnodes (ast);
		}

		public void Accept (NodeExpr expr)
		{
			visitSubnodes (expr);
			methodBuilder.EmitInstruction (Opcode.Pop);
		}

		public void Accept (NodeStmt stmt)
		{
			visitSubnodes (stmt);
		}

		public void Accept (NodeBinOp binop)
		{
			if (binop.Operation == BinaryOperation.Assign) {
				binop.Right.Visit (this);
				if (binop.Left is NodeIdent) {
					NodeIdent ident = (NodeIdent)binop.Left;
					Symbol sym = symbolTable.GetSymbol (ident.Value);
					if (sym.Type == SymbolType.Local) {
						methodBuilder.EmitInstruction (Opcode.StoreLocal, sym.Index);
						methodBuilder.EmitInstruction (Opcode.LoadLocal, sym.Index);
					} else {
						int globalIndex = methodBuilder.Module.DefineConstant (new IodineName
							(ident.Value));
						methodBuilder.EmitInstruction (Opcode.StoreGlobal, globalIndex);
						methodBuilder.EmitInstruction (Opcode.LoadGlobal, globalIndex);
					}
				} else if (binop.Left is NodeGetAttr) {
					((NodeGetAttr)binop.Left).Target.Visit (this);
					int attrIndex = methodBuilder.Module.DefineConstant (new IodineName(((NodeGetAttr)binop.Left).Field));
					methodBuilder.EmitInstruction (Opcode.StoreAttribute, attrIndex);
					((NodeGetAttr)binop.Left).Target.Visit (this);
					methodBuilder.EmitInstruction (Opcode.LoadAttribute, attrIndex);
				} else if (binop.Left is NodeIndexer) {
					((NodeIndexer)binop.Left).Target.Visit (this);
					((NodeIndexer)binop.Left).Index.Visit (this);
					methodBuilder.EmitInstruction (Opcode.StoreIndex);
					binop.Left.Visit (this);
				}
			} else {
				binop.Right.Visit (this);
				binop.Left.Visit (this);
				methodBuilder.EmitInstruction (Opcode.BinOp, (int)binop.Operation);
			}
		}

		public void Accept (NodeUnaryOp unaryop)
		{
			visitSubnodes (unaryop);
			methodBuilder.EmitInstruction (Opcode.UnaryOp, (int)unaryop.Operation);
		}

		public void Accept (NodeIdent ident)
		{
			if (symbolTable.IsSymbolDefined (ident.Value)) {
				Symbol sym = symbolTable.GetSymbol (ident.Value);
				if (sym.Type == SymbolType.Local) {
					methodBuilder.EmitInstruction (Opcode.LoadLocal, sym.Index);
				} else {
					methodBuilder.EmitInstruction (Opcode.LoadGlobal,
						methodBuilder.Module.DefineConstant (new IodineName (ident.Value)));
				}
			} else {
				methodBuilder.EmitInstruction (Opcode.LoadGlobal,
					methodBuilder.Module.DefineConstant (new IodineName (ident.Value)));
			}
		}

		public void Accept (NodeCall call)
		{
			call.Arguments.Visit (this);
			if (call.Target is NodeIdent && ((NodeIdent)call.Target).Value == "print") {
				methodBuilder.EmitInstruction (Opcode.Print, call.Arguments.Children.Count);
			} else {
				call.Target.Visit (this);
				methodBuilder.EmitInstruction (Opcode.Invoke, call.Arguments.Children.Count);
			}
		}

		public void Accept (NodeArgList arglist)
		{
			visitSubnodes (arglist);
		}

		public void Accept (NodeGetAttr getAttr)
		{
			getAttr.Target.Visit (this);
			methodBuilder.EmitInstruction (Opcode.LoadAttribute, methodBuilder.Module.DefineConstant (
				new IodineName (getAttr.Field)
			));
		}

		public void Accept (NodeInteger integer)
		{
			methodBuilder.EmitInstruction (Opcode.LoadConst, 
				methodBuilder.Module.DefineConstant (new IodineInteger (integer.Value)));
		}

		public void Accept (NodeIfStmt ifStmt)
		{
			IodineLabel elseLabel = methodBuilder.CreateLabel ();
			IodineLabel endLabel = methodBuilder.CreateLabel ();
			ifStmt.Condition.Visit (this);
			methodBuilder.EmitInstruction (Opcode.JumpIfFalse, elseLabel);
			ifStmt.Body.Visit (this);
			methodBuilder.EmitInstruction (Opcode.Jump, endLabel);
			methodBuilder.MarkLabelPosition (elseLabel);
			ifStmt.ElseBody.Visit (this);
			methodBuilder.MarkLabelPosition (endLabel);
		}

		public void Accept (NodeWhileStmt whileStmt)
		{
			IodineLabel whileLabel = methodBuilder.CreateLabel ();
			IodineLabel breakLabel = methodBuilder.CreateLabel ();
			breakLabels.Push (breakLabel);
			methodBuilder.MarkLabelPosition (whileLabel);
			whileStmt.Condition.Visit (this);
			methodBuilder.EmitInstruction (Opcode.JumpIfFalse, breakLabel);
			whileStmt.Body.Visit (this);
			methodBuilder.EmitInstruction (Opcode.Jump, whileLabel);
			methodBuilder.MarkLabelPosition (breakLabel);
			breakLabels.Pop ();
		}

		public void Accept (NodeForStmt forStmt)
		{
			IodineLabel forLabel = methodBuilder.CreateLabel ();
			IodineLabel breakLabel = methodBuilder.CreateLabel ();
			breakLabels.Push (breakLabel);
			forStmt.Initializer.Visit (this);
			methodBuilder.MarkLabelPosition (forLabel);
			forStmt.Condition.Visit (this);
			methodBuilder.EmitInstruction (Opcode.JumpIfFalse, breakLabel);
			forStmt.Body.Visit (this);
			forStmt.AfterThought.Visit (this);
			methodBuilder.EmitInstruction (Opcode.Jump, forLabel);
			methodBuilder.MarkLabelPosition (breakLabel);
			breakLabels.Pop ();
		}

		public void Accept (NodeForeach foreachStmt) 
		{
			IodineLabel foreachLabel = methodBuilder.CreateLabel ();
			IodineLabel breakLabel = methodBuilder.CreateLabel ();
			breakLabels.Push (breakLabel);
			foreachStmt.Iterator.Visit (this);
			int tmp = methodBuilder.CreateTemporary ();
			methodBuilder.EmitInstruction (Opcode.Dup);
			methodBuilder.EmitInstruction (Opcode.StoreLocal, tmp);
			methodBuilder.EmitInstruction (Opcode.IterReset);
			methodBuilder.MarkLabelPosition (foreachLabel);
			methodBuilder.EmitInstruction (Opcode.LoadLocal, tmp);
			methodBuilder.EmitInstruction (Opcode.IterMoveNext);
			methodBuilder.EmitInstruction (Opcode.JumpIfFalse, breakLabel);
			methodBuilder.EmitInstruction (Opcode.LoadLocal, tmp);
			methodBuilder.EmitInstruction (Opcode.IterGetNext);
			methodBuilder.EmitInstruction (Opcode.StoreLocal,  symbolTable.GetSymbol
				(foreachStmt.Item).Index);
			foreachStmt.Body.Visit (this);
			methodBuilder.EmitInstruction (Opcode.Jump, foreachLabel);
			methodBuilder.MarkLabelPosition (breakLabel);
			breakLabels.Pop ();
		}

		public void Accept (NodeFuncDecl funcDecl)
		{
		}

		public void Accept (NodeScope scope)
		{
			symbolTable.CurrentScope = symbolTable.CurrentScope.ChildScopes[currentScope++];

			FunctionCompiler scopeCompiler = new FunctionCompiler (errorLog, symbolTable, methodBuilder,
				breakLabels);
			foreach (AstNode node in scope) {
				node.Visit (scopeCompiler);
			}

			symbolTable.CurrentScope = symbolTable.CurrentScope.ParentScope;
		}

		public void Accept (NodeString str)
		{
			methodBuilder.EmitInstruction (Opcode.LoadConst, 
				methodBuilder.Module.DefineConstant (new IodineString (str.Value)));
		}

		public void Accept (NodeUseStatement useStmt)
		{
			
		}

		public void Accept (NodeClassDecl classDecl)
		{
		}

		public void Accept (NodeReturnStmt returnStmt)
		{
			visitSubnodes (returnStmt);
			methodBuilder.EmitInstruction (Opcode.Return);
		}

		public void Accept (NodeIndexer indexer)
		{
			indexer.Target.Visit (this);
			indexer.Index.Visit (this);
			methodBuilder.EmitInstruction (Opcode.LoadIndex);
		}

		public void Accept (NodeList list)
		{
			visitSubnodes (list);
			methodBuilder.EmitInstruction (Opcode.BuildList, list.Children.Count);
		}

		public void Accept (NodeSelf self)
		{
			methodBuilder.EmitInstruction (Opcode.LoadSelf);
		}

		public void Accept (NodeTrue ntrue)
		{
			methodBuilder.EmitInstruction (Opcode.LoadTrue);
		}

		public void Accept (NodeFalse nfalse)
		{
			methodBuilder.EmitInstruction (Opcode.LoadFalse);
		}

		public void Accept (NodeNull nil)
		{
			methodBuilder.EmitInstruction (Opcode.LoadNull);
		}

		public void Accept (NodeLambda lambda)
		{
			IodineMethod anonMethod = new IodineMethod (methodBuilder.Module, null, lambda.InstanceMethod, 
				lambda.Parameters.Count, methodBuilder.LocalCount);
			FunctionCompiler compiler = new FunctionCompiler (errorLog, symbolTable, anonMethod);
			symbolTable.CurrentScope = symbolTable.CurrentScope.ChildScopes[currentScope++];
			for (int i = 0; i < lambda.Parameters.Count; i++) {
				anonMethod.Parameters[lambda.Parameters[i]] = symbolTable.GetSymbol
					(lambda.Parameters [i]).Index;
			}
			lambda.Children[0].Visit (compiler);
			anonMethod.FinalizeLabels ();
			symbolTable.CurrentScope = symbolTable.CurrentScope.ParentScope;
			methodBuilder.EmitInstruction (Opcode.LoadConst, methodBuilder.Module.DefineConstant (
				anonMethod));
			methodBuilder.EmitInstruction (Opcode.BuildClosure);
		}


		public void Accept (NodeTryExcept tryExcept)
		{
			IodineLabel exceptLabel = methodBuilder.CreateLabel ();
			IodineLabel endLabel = methodBuilder.CreateLabel ();
			methodBuilder.EmitInstruction (Opcode.PushExceptionHandler, exceptLabel);
			tryExcept.TryBody.Visit (this);
			methodBuilder.EmitInstruction (Opcode.PopExceptionHandler);
			methodBuilder.EmitInstruction (Opcode.Jump, endLabel);
			methodBuilder.MarkLabelPosition (exceptLabel);
			if (tryExcept.ExceptionIdentifier != null) {
				methodBuilder.EmitInstruction (Opcode.LoadException);
				methodBuilder.EmitInstruction (Opcode.StoreLocal, symbolTable.GetSymbol (
					tryExcept.ExceptionIdentifier).Index);
			}
			tryExcept.ExceptBody.Visit (this);
			methodBuilder.MarkLabelPosition (endLabel);
		}

		public void Accept (NodeTuple tuple)
		{
			visitSubnodes (tuple);
			methodBuilder.EmitInstruction (Opcode.BuildTuple, tuple.Children.Count);
		}

		public void Accept (NodeConstant constant)
		{
		}


		public void Accept (NodeBreak brk) 
		{
			methodBuilder.EmitInstruction (Opcode.Jump, breakLabels.Peek ());
		}

		private void visitSubnodes (AstNode root)
		{
			foreach (AstNode node in root) {
				node.Visit (this);
			}
		} 
	}
}

