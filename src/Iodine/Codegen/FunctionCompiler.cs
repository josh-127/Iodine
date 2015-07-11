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
		private Stack<IodineLabel> continueLabels = new Stack<IodineLabel>();

		public FunctionCompiler (ErrorLog errorLog, SymbolTable symbolTable, IodineMethod methodBuilder)
		{
			this.errorLog = errorLog;
			this.symbolTable = symbolTable;
			this.methodBuilder = methodBuilder;
		}

		public FunctionCompiler (ErrorLog errorLog, SymbolTable symbolTable, IodineMethod methodBuilder,
			Stack<IodineLabel> breakLabels, Stack<IodineLabel> continueLabels)
		{
			this.errorLog = errorLog;
			this.symbolTable = symbolTable;
			this.methodBuilder = methodBuilder;
			this.breakLabels = breakLabels;
			this.continueLabels = continueLabels;
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
						methodBuilder.EmitInstruction (ident.Location, Opcode.StoreLocal, sym.Index);
						methodBuilder.EmitInstruction (ident.Location, Opcode.LoadLocal, sym.Index);
					} else {
						int globalIndex = methodBuilder.Module.DefineConstant (new IodineName
							(ident.Value));
						methodBuilder.EmitInstruction (ident.Location, Opcode.StoreGlobal, globalIndex);
						methodBuilder.EmitInstruction (ident.Location, Opcode.LoadGlobal, globalIndex);
					}
				} else if (binop.Left is NodeGetAttr) {
					NodeGetAttr getattr = binop.Left as NodeGetAttr;
					getattr.Target.Visit (this);
					int attrIndex = methodBuilder.Module.DefineConstant (new IodineName(getattr.Field));
					methodBuilder.EmitInstruction (getattr.Location, Opcode.StoreAttribute, attrIndex);
					getattr.Target.Visit (this);
					methodBuilder.EmitInstruction (getattr.Location, Opcode.LoadAttribute, attrIndex);
				} else if (binop.Left is NodeIndexer) {
					NodeIndexer indexer = binop.Left as NodeIndexer;
					indexer.Target.Visit (this);
					indexer.Index.Visit (this);
					methodBuilder.EmitInstruction (indexer.Location, Opcode.StoreIndex);
					binop.Left.Visit (this);
				}
			} else if (binop.Operation == BinaryOperation.InstanceOf) {
				binop.Right.Visit (this);
				binop.Left.Visit (this);
				methodBuilder.EmitInstruction (binop.Location, Opcode.InstanceOf);
			} else {
				IodineLabel shortCircuitTrueLabel = methodBuilder.CreateLabel ();
				IodineLabel shortCircuitFalseLabel = methodBuilder.CreateLabel ();
				IodineLabel endLabel = methodBuilder.CreateLabel ();
				binop.Left.Visit (this);
				switch (binop.Operation) {
				case BinaryOperation.BoolAnd:
					methodBuilder.EmitInstruction (binop.Location, Opcode.Dup);
					methodBuilder.EmitInstruction (binop.Location, Opcode.JumpIfFalse,
						shortCircuitFalseLabel);
					break;
				case BinaryOperation.BoolOr:
					methodBuilder.EmitInstruction (binop.Location, Opcode.Dup);
					methodBuilder.EmitInstruction (binop.Location, Opcode.JumpIfTrue,
						shortCircuitTrueLabel);
					break;
				}
				binop.Right.Visit (this);
				methodBuilder.EmitInstruction (binop.Location, Opcode.BinOp, (int)binop.Operation);
				methodBuilder.EmitInstruction (binop.Location, Opcode.Jump, endLabel);
				methodBuilder.MarkLabelPosition (shortCircuitTrueLabel);
				methodBuilder.EmitInstruction (binop.Location, Opcode.Pop);
				methodBuilder.EmitInstruction (binop.Location, Opcode.LoadTrue);
				methodBuilder.EmitInstruction (binop.Location, Opcode.Jump, endLabel);
				methodBuilder.MarkLabelPosition (shortCircuitFalseLabel);
				methodBuilder.EmitInstruction (binop.Location, Opcode.Pop);
				methodBuilder.EmitInstruction (binop.Location, Opcode.LoadFalse);
				methodBuilder.MarkLabelPosition (endLabel);
			}
		}

		public void Accept (NodeUnaryOp unaryop)
		{
			visitSubnodes (unaryop);
			methodBuilder.EmitInstruction (unaryop.Location, Opcode.UnaryOp, (int)unaryop.Operation);
		}

		public void Accept (NodeIdent ident)
		{
			if (symbolTable.IsSymbolDefined (ident.Value)) {
				Symbol sym = symbolTable.GetSymbol (ident.Value);
				if (sym.Type == SymbolType.Local) {
					methodBuilder.EmitInstruction (ident.Location, Opcode.LoadLocal, sym.Index);
				} else {
					methodBuilder.EmitInstruction (ident.Location, Opcode.LoadGlobal,
						methodBuilder.Module.DefineConstant (new IodineName (ident.Value)));
				}
			} else {
				methodBuilder.EmitInstruction (ident.Location, Opcode.LoadGlobal,
					methodBuilder.Module.DefineConstant (new IodineName (ident.Value)));
			}
		}

		public void Accept (NodeCall call)
		{
			call.Arguments.Visit (this);
			call.Target.Visit (this);
			methodBuilder.EmitInstruction (call.Target.Location, Opcode.Invoke, 
				call.Arguments.Children.Count);
		}

		public void Accept (NodeArgList arglist)
		{
			visitSubnodes (arglist);
		}

		public void Accept (NodeGetAttr getAttr)
		{
			getAttr.Target.Visit (this);
			methodBuilder.EmitInstruction (getAttr.Location, Opcode.LoadAttribute,
				methodBuilder.Module.DefineConstant (new IodineName (getAttr.Field)));
		}

		public void Accept (NodeInteger integer)
		{
			methodBuilder.EmitInstruction (integer.Location, Opcode.LoadConst, 
				methodBuilder.Module.DefineConstant (new IodineInteger (integer.Value)));
		}

		public void Accept (NodeFloat num)
		{
			methodBuilder.EmitInstruction (num.Location, Opcode.LoadConst, 
				methodBuilder.Module.DefineConstant (new IodineFloat (num.Value)));
		}

		public void Accept (NodeIfStmt ifStmt)
		{
			IodineLabel elseLabel = methodBuilder.CreateLabel ();
			IodineLabel endLabel = methodBuilder.CreateLabel ();
			ifStmt.Condition.Visit (this);
			methodBuilder.EmitInstruction (ifStmt.Body.Location, Opcode.JumpIfFalse, elseLabel);
			ifStmt.Body.Visit (this);
			methodBuilder.EmitInstruction (ifStmt.ElseBody.Location, Opcode.Jump, endLabel);
			methodBuilder.MarkLabelPosition (elseLabel);
			ifStmt.ElseBody.Visit (this);
			methodBuilder.MarkLabelPosition (endLabel);
		}

		public void Accept (NodeWhileStmt whileStmt)
		{
			IodineLabel whileLabel = methodBuilder.CreateLabel ();
			IodineLabel breakLabel = methodBuilder.CreateLabel ();
			breakLabels.Push (breakLabel);
			continueLabels.Push (whileLabel);
			methodBuilder.MarkLabelPosition (whileLabel);
			whileStmt.Condition.Visit (this);
			methodBuilder.EmitInstruction (whileStmt.Condition.Location, Opcode.JumpIfFalse,
				breakLabel);
			whileStmt.Body.Visit (this);
			methodBuilder.EmitInstruction (whileStmt.Body.Location, Opcode.Jump, whileLabel);
			methodBuilder.MarkLabelPosition (breakLabel);
			breakLabels.Pop ();
			continueLabels.Pop ();
		}

		public void Accept (NodeForStmt forStmt)
		{
			IodineLabel forLabel = methodBuilder.CreateLabel ();
			IodineLabel breakLabel = methodBuilder.CreateLabel ();
			breakLabels.Push (breakLabel);
			continueLabels.Push (forLabel);
			forStmt.Initializer.Visit (this);
			methodBuilder.MarkLabelPosition (forLabel);
			forStmt.Condition.Visit (this);
			methodBuilder.EmitInstruction (forStmt.Condition.Location, Opcode.JumpIfFalse, breakLabel);
			forStmt.Body.Visit (this);
			forStmt.AfterThought.Visit (this);
			methodBuilder.EmitInstruction (forStmt.AfterThought.Location, Opcode.Jump, forLabel);
			methodBuilder.MarkLabelPosition (breakLabel);
			breakLabels.Pop ();
			continueLabels.Pop ();
		}

		public void Accept (NodeForeach foreachStmt) 
		{
			IodineLabel foreachLabel = methodBuilder.CreateLabel ();
			IodineLabel breakLabel = methodBuilder.CreateLabel ();
			breakLabels.Push (breakLabel);
			continueLabels.Push (foreachLabel);
			foreachStmt.Iterator.Visit (this);
			int tmp = methodBuilder.CreateTemporary ();
			methodBuilder.EmitInstruction (foreachStmt.Iterator.Location, Opcode.Dup);
			methodBuilder.EmitInstruction (foreachStmt.Iterator.Location, Opcode.StoreLocal, tmp);
			methodBuilder.EmitInstruction (foreachStmt.Iterator.Location, Opcode.IterReset);
			methodBuilder.MarkLabelPosition (foreachLabel);
			methodBuilder.EmitInstruction (foreachStmt.Iterator.Location, Opcode.LoadLocal, tmp);
			methodBuilder.EmitInstruction (foreachStmt.Iterator.Location, Opcode.IterMoveNext);
			methodBuilder.EmitInstruction (foreachStmt.Iterator.Location, Opcode.JumpIfFalse,
				breakLabel);
			methodBuilder.EmitInstruction (foreachStmt.Iterator.Location, Opcode.LoadLocal, tmp);
			methodBuilder.EmitInstruction (foreachStmt.Iterator.Location, Opcode.IterGetNext);
			methodBuilder.EmitInstruction (foreachStmt.Iterator.Location, Opcode.StoreLocal,
				symbolTable.GetSymbol
				(foreachStmt.Item).Index);
			foreachStmt.Body.Visit (this);
			methodBuilder.EmitInstruction (foreachStmt.Body.Location, Opcode.Jump, foreachLabel);
			methodBuilder.MarkLabelPosition (breakLabel);
			breakLabels.Pop ();
			continueLabels.Pop ();
		}

		public void Accept (NodeFuncDecl funcDecl)
		{
		}

		public void Accept (NodeScope scope)
		{
			symbolTable.CurrentScope = symbolTable.CurrentScope.ChildScopes[currentScope++];

			FunctionCompiler scopeCompiler = new FunctionCompiler (errorLog, symbolTable, methodBuilder,
				breakLabels, continueLabels);
			foreach (AstNode node in scope) {
				node.Visit (scopeCompiler);
			}

			symbolTable.CurrentScope = symbolTable.CurrentScope.ParentScope;
		}

		public void Accept (NodeString str)
		{
			visitSubnodes (str);
			methodBuilder.EmitInstruction (str.Location, Opcode.LoadConst, 
				methodBuilder.Module.DefineConstant (new IodineString (str.Value)));
			if (str.Children.Count != 0) {
				methodBuilder.EmitInstruction (str.Location, Opcode.LoadAttribute,
					methodBuilder.Module.DefineConstant (new IodineName ("format")));
				methodBuilder.EmitInstruction (str.Location, Opcode.Invoke, str.Children.Count);
			}
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
			methodBuilder.EmitInstruction (returnStmt.Location, Opcode.Return);
		}

		public void Accept (NodeIndexer indexer)
		{
			indexer.Target.Visit (this);
			indexer.Index.Visit (this);
			methodBuilder.EmitInstruction (indexer.Location, Opcode.LoadIndex);
		}

		public void Accept (NodeList list)
		{
			visitSubnodes (list);
			methodBuilder.EmitInstruction (list.Location, Opcode.BuildList, list.Children.Count);
		}

		public void Accept (NodeSelf self)
		{
			methodBuilder.EmitInstruction (self.Location, Opcode.LoadSelf);
		}

		public void Accept (NodeTrue ntrue)
		{
			methodBuilder.EmitInstruction (ntrue.Location, Opcode.LoadTrue);
		}

		public void Accept (NodeFalse nfalse)
		{
			methodBuilder.EmitInstruction (nfalse.Location, Opcode.LoadFalse);
		}

		public void Accept (NodeNull nil)
		{
			methodBuilder.EmitInstruction (nil.Location, Opcode.LoadNull);
		}

		public void Accept (NodeLambda lambda)
		{
			symbolTable.CurrentScope = symbolTable.CurrentScope.ChildScopes[currentScope++];
			IodineMethod anonMethod = new IodineMethod (methodBuilder, methodBuilder.Module, null, lambda.InstanceMethod, 
				lambda.Parameters.Count, methodBuilder.LocalCount);
			FunctionCompiler compiler = new FunctionCompiler (errorLog, symbolTable, anonMethod);
			for (int i = 0; i < lambda.Parameters.Count; i++) {
				anonMethod.Parameters[lambda.Parameters[i]] = symbolTable.GetSymbol
					(lambda.Parameters [i]).Index;
			}
			lambda.Children[0].Visit (compiler);
			anonMethod.EmitInstruction (lambda.Location, Opcode.LoadNull);
			anonMethod.Variadic = lambda.Variadic;
			anonMethod.FinalizeLabels ();
			symbolTable.CurrentScope = symbolTable.CurrentScope.ParentScope;
			methodBuilder.EmitInstruction (lambda.Location, Opcode.LoadConst,
				methodBuilder.Module.DefineConstant (anonMethod));
			methodBuilder.EmitInstruction (lambda.Location, Opcode.BuildClosure);
		}


		public void Accept (NodeTryExcept tryExcept)
		{
			IodineLabel exceptLabel = methodBuilder.CreateLabel ();
			IodineLabel endLabel = methodBuilder.CreateLabel ();
			methodBuilder.EmitInstruction (tryExcept.Location, Opcode.PushExceptionHandler, exceptLabel);
			tryExcept.TryBody.Visit (this);
			methodBuilder.EmitInstruction (tryExcept.TryBody.Location, Opcode.PopExceptionHandler);
			methodBuilder.EmitInstruction (tryExcept.TryBody.Location, Opcode.Jump, endLabel);
			methodBuilder.MarkLabelPosition (exceptLabel);
			tryExcept.TypeList.Visit (this);
			if (tryExcept.TypeList.Children.Count > 0) {
				methodBuilder.EmitInstruction (tryExcept.ExceptBody.Location, Opcode.BeginExcept,
					tryExcept.TypeList.Children.Count);
			}
			if (tryExcept.ExceptionIdentifier != null) {
				methodBuilder.EmitInstruction (tryExcept.ExceptBody.Location, Opcode.LoadException);
				methodBuilder.EmitInstruction (tryExcept.ExceptBody.Location, Opcode.StoreLocal,
					symbolTable.GetSymbol (tryExcept.ExceptionIdentifier).Index);
			}
			tryExcept.ExceptBody.Visit (this);
			methodBuilder.MarkLabelPosition (endLabel);
		}

		public void Accept (NodeRaiseStmt raise)
		{
			raise.Value.Visit (this);
			methodBuilder.EmitInstruction (Opcode.Raise);
		}

		public void Accept (NodeTuple tuple)
		{
			visitSubnodes (tuple);
			methodBuilder.EmitInstruction (tuple.Location, Opcode.BuildTuple, tuple.Children.Count);
		}

		public void Accept (NodeConstant constant)
		{
		}

		public void Accept (NodeSuperCall super)
		{
			List<string> parent = super.Parent.Base;
			super.Arguments.Visit (this);
			methodBuilder.EmitInstruction (super.Location, Opcode.LoadGlobal,
				methodBuilder.Module.DefineConstant (new IodineName (parent[0])));
			for (int i = 1; i < parent.Count; i++) {
				methodBuilder.EmitInstruction (super.Location, Opcode.LoadAttribute,
					methodBuilder.Module.DefineConstant (new IodineName (parent[0])));
			}
			methodBuilder.EmitInstruction (super.Location, Opcode.InvokeSuper,
				super.Arguments.Children.Count);
		}

		public void Accept (NodeBreak brk) 
		{
			methodBuilder.EmitInstruction (brk.Location, Opcode.Jump, breakLabels.Peek ());
		}

		public void Accept (NodeContinue cont) 
		{
			methodBuilder.EmitInstruction (cont.Location, Opcode.Jump, continueLabels.Peek ());
		}

		public void Accept (NodeEnumDecl enumDecl)
		{
		}

		private void visitSubnodes (AstNode root)
		{
			foreach (AstNode node in root) {
				node.Visit (this);
			}
		} 
	}
}

