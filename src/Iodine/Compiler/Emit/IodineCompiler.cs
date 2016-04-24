/**
  * Copyright (c) 2015, GruntTheDivine All rights reserved.

  * Redistribution and use in source and binary forms, with or without modification,
  * are permitted provided that the following conditions are met:
  * 
  *  * Redistributions of source code must retain the above copyright notice, this list
  *    of conditions and the following disclaimer.
  * 
  *  * Redistributions in binary form must reproduce the above copyright notice, this
  *    list of conditions and the following disclaimer in the documentation and/or
  *    other materials provided with the distribution.

  * Neither the name of the copyright holder nor the names of its contributors may be
  * used to endorse or promote products derived from this software without specific
  * prior written permission.
  * 
  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
  * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
  * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
  * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
  * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
  * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
  * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
  * CONTRACT ,STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
  * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
  * DAMAGE.
**/

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Iodine.Compiler;
using Iodine.Compiler.Ast;
using Iodine.Runtime;

namespace Iodine.Compiler
{
    /// <summary>
    /// Responsible for compiling an Iodine abstract syntax tree into iodine bytecode. 
    /// </summary>
    public class IodineCompiler : IodineAstVisitor
    {
        private static List<IBytecodeOptimization> Optimizations = new List<IBytecodeOptimization> ();

        static IodineCompiler ()
        {
            Optimizations.Add (new ControlFlowOptimization ());
            Optimizations.Add (new InstructionOptimization ());
        }

        private Stack<EmitContext> emitContexts = new Stack<EmitContext> ();

        public EmitContext Context {
            get {
                return emitContexts.Peek ();
            }
        }

        private SymbolTable symbolTable;
        private CompilationUnit root;

        private IodineCompiler (SymbolTable symbolTable, CompilationUnit root)
        {
            this.symbolTable = symbolTable;
            this.root = root;
        }

        public static IodineCompiler CreateCompiler (IodineContext context, CompilationUnit root)
        {
            SemanticAnalyser analyser = new SemanticAnalyser (context.ErrorLog);
            SymbolTable table = analyser.Analyse (root);
            return new IodineCompiler (table, root);
        }

        public IodineModule Compile (string moduleName)
        {
            ModuleBuilder moduleBuilder = new ModuleBuilder (moduleName);
            EmitContext context = new EmitContext (symbolTable, moduleBuilder, moduleBuilder.Initializer);

            context.SetCurrentModule (moduleBuilder);

            emitContexts.Push (context);

            root.Visit (this);

            moduleBuilder.Initializer.FinalizeLabels ();

            if (context.ShouldOptimize) {
                OptimizeObject (moduleBuilder);
            }

            DestroyContext ();

            return moduleBuilder;
        }

        private void OptimizeObject (IodineObject obj)
        {
            foreach (IodineObject attr in obj.Attributes.Values) {
                if (attr is MethodBuilder) {
                    MethodBuilder method = attr as MethodBuilder;
                    foreach (IBytecodeOptimization opt in Optimizations) {
                        opt.PerformOptimization (method);
                    }
                }
            }
        }

        private void CreateContext ()
        {
            emitContexts.Push (new EmitContext (Context.SymbolTable,
                Context.CurrentModule,
                Context.CurrentMethod,
                Context.IsInClass,
                Context.CurrentClass
            ));
        }

        private void CreateContext (ClassBuilder clazz)
        {
            emitContexts.Push (new EmitContext (Context.SymbolTable,
                Context.CurrentModule,
                Context.CurrentMethod,
                true,
                clazz
            ));
        }

        private void CreateContext (MethodBuilder methodBuilder)
        {
            emitContexts.Push (new EmitContext (Context.SymbolTable,
                Context.CurrentModule,
                methodBuilder,
                Context.IsInClass,
                Context.CurrentClass
            ));
        }

        private void CreatePatternContext (int temporary)
        {
            emitContexts.Push (new EmitContext (Context.SymbolTable,
                Context.CurrentModule,
                Context.CurrentMethod,
                Context.IsInClass,
                Context.CurrentClass,
                true,
                temporary
            ));
        }

        private void DestroyContext ()
        {
            emitContexts.Pop ();
        }

        public override void Accept (CompilationUnit ast)
        {
            ast.VisitChildren (this);
        }

        #region Declarations

        public ClassBuilder CompileClass (ClassDeclaration classDecl)
        {
            MethodBuilder constructor = CompileGlobalMethod (classDecl.Constructor);
            if (classDecl.Constructor.Body.Statements.Count == 0) {
                if (classDecl.Base.Count > 0) {
                    foreach (string subclass in classDecl.Base) {
                        string[] contract = subclass.Split ('.');
                        constructor.EmitInstruction (classDecl.Location,
                            Opcode.LoadGlobal,
                            Context.CurrentModule.DefineConstant (new IodineName (contract [0]))
                        );
                        for (int j = 1; j < contract.Length; j++) {
                            constructor.EmitInstruction (classDecl.Location,
                                Opcode.LoadAttribute,
                                Context.CurrentModule.DefineConstant (new IodineName (contract [0]))
                            );
                        }
                        constructor.EmitInstruction (classDecl.Location, Opcode.InvokeSuper, 0);
                    }
                }
            }

            MethodBuilder initializer = new MethodBuilder (Context.CurrentModule,
                "__init__",
                false,
                0,
                false,
                false
            );

            ClassBuilder clazz = new ClassBuilder (classDecl.Name, initializer, constructor, Context.CurrentClass);

            CreateContext (clazz);

            foreach (AstNode member in classDecl.Members) {
                if (member is FunctionDeclaration) {
                    FunctionDeclaration func = member as FunctionDeclaration;

                    if (func.Name == "__init__") {
                        CreateContext (initializer);
                        func.Body.Visit (this);
                        DestroyContext ();
                    } else if (func.InstanceMethod) {
                        clazz.AddInstanceMethod (CompileGlobalMethod (func));
                    } else {
                        clazz.SetAttribute (func.Name, CompileGlobalMethod (func));
                    }
                } else if (member is ClassDeclaration) {
                    ClassDeclaration subclass = member as ClassDeclaration;
                    clazz.SetAttribute (subclass.Name, CompileClass (subclass));
                } else if (member is EnumDeclaration) {
                    EnumDeclaration enumeration = member as EnumDeclaration;
                    clazz.SetAttribute (enumeration.Name, CompileEnum (enumeration));
                } else if (member is Expression) {

                    BinaryExpression expr = ((Expression)member).Child as BinaryExpression;

                    NameExpression name = expr.Left as NameExpression;

                    CreateContext (initializer);

                    expr.Right.Visit (this);

                    LoadAssociatedClass ();

                    initializer.EmitInstruction (classDecl.Location,
                        Opcode.StoreAttribute,
                        Context.CurrentModule.DefineConstant (new IodineName (name.Value))
                    );

                    DestroyContext ();
                } else {
                    member.Visit (this);
                }
            }

            DestroyContext ();

            initializer.FinalizeLabels ();
            constructor.FinalizeLabels ();

            return clazz;
        }

        private IodineEnum CompileEnum (EnumDeclaration enumDecl)
        {
            IodineEnum ienum = new IodineEnum (enumDecl.Name);
            foreach (string name in enumDecl.Items.Keys) {
                ienum.AddItem (name, enumDecl.Items [name]);
            }
            return ienum;
        }

        private IodineContract CompileContract (ContractDeclaration contractDecl)
        {
            IodineContract contract = new IodineContract (contractDecl.Name);
            foreach (AstNode node in contractDecl.Members) {
                FunctionDeclaration decl = node as FunctionDeclaration;
                contract.AddMethod (new MethodBuilder (Context.CurrentModule,
                    decl.Name,
                    decl.InstanceMethod,
                    decl.Parameters.Count,
                    decl.Variadic,
                    decl.AcceptsKeywordArgs
                ));
            }
            return contract;
        }

        private IodineTrait CompileTrait (TraitDeclaration traitDecl)
        {
            IodineTrait trait = new IodineTrait (traitDecl.Name);
            foreach (AstNode node in traitDecl.Members) {
                FunctionDeclaration decl = node as FunctionDeclaration;
                trait.AddMethod (new MethodBuilder (Context.CurrentModule,
                    decl.Name,
                    decl.InstanceMethod,
                    decl.Parameters.Count,
                    decl.Variadic,
                    decl.AcceptsKeywordArgs
                ));
            }
            return trait;
        }

        private MethodBuilder CompileGlobalMethod (FunctionDeclaration funcDecl)
        {
            Context.SymbolTable.EnterScope ();

            MethodBuilder methodBuilder = new MethodBuilder (Context.CurrentModule,
                funcDecl.Name,
                funcDecl.InstanceMethod,
                funcDecl.Parameters.Count,
                funcDecl.Variadic,
                funcDecl.AcceptsKeywordArgs
            );

            for (int i = 0; i < funcDecl.Parameters.Count; i++) {
                methodBuilder.Parameters [funcDecl.Parameters [i]] =
					Context.SymbolTable.AddSymbol (funcDecl.Parameters [i]);
            }

            CreateContext (methodBuilder);

            funcDecl.VisitChildren (this);

            DestroyContext ();

            methodBuilder.EmitInstruction (funcDecl.Location, Opcode.LoadNull);

            methodBuilder.FinalizeLabels ();

            Context.SymbolTable.ExitScope ();

            return methodBuilder;
        }

        private void CompileLocalMethod (FunctionDeclaration funcDecl)
        {
            int index = Context.SymbolTable.AddSymbol (funcDecl.Name);

            Context.SymbolTable.EnterScope ();

            MethodBuilder anonMethod = new MethodBuilder (Context.CurrentMethod,
                Context.CurrentModule,
                null,
                funcDecl.InstanceMethod,
                funcDecl.Parameters.Count,
                funcDecl.Variadic,
                funcDecl.AcceptsKeywordArgs
            );

            for (int i = 0; i < funcDecl.Parameters.Count; i++) {
                anonMethod.Parameters [funcDecl.Parameters [i]] = Context.SymbolTable.AddSymbol (funcDecl.Parameters [i]);
            }

            CreateContext (anonMethod);

            funcDecl.VisitChildren (this);

            anonMethod.EmitInstruction (funcDecl.Location, Opcode.LoadNull);

            anonMethod.FinalizeLabels ();

            DestroyContext ();

            Context.CurrentMethod.EmitInstruction (funcDecl.Location,
                Opcode.LoadConst,
                Context.CurrentModule.DefineConstant (anonMethod)
            );

            Context.CurrentMethod.EmitInstruction (funcDecl.Location, Opcode.BuildClosure);
            Context.CurrentMethod.EmitInstruction (funcDecl.Location,
                Opcode.StoreLocal,
                index
            );

            Context.SymbolTable.ExitScope ();
        }

        public override void Accept (ClassDeclaration classDecl)
        {
            ClassBuilder clazz = CompileClass (classDecl);
		
            if (Context.SymbolTable.IsInGlobalScope) {
                Context.CurrentModule.SetAttribute (classDecl.Name, clazz);
            } else {
                Context.CurrentMethod.EmitInstruction (classDecl.Location,
                    Opcode.LoadConst,
                    Context.CurrentModule.DefineConstant (clazz)
                );

                Context.CurrentMethod.EmitInstruction (classDecl.Location,
                    Opcode.StoreLocal,
                    Context.SymbolTable.AddSymbol (clazz.Name)
                );
            }
        }

        public override void Accept (EnumDeclaration enumDecl)
        {
            IodineEnum ienum = CompileEnum (enumDecl);

            if (!Context.SymbolTable.IsInGlobalScope) {
                Context.CurrentMethod.EmitInstruction (enumDecl.Location,
                    Opcode.LoadConst,
                    Context.CurrentModule.DefineConstant (ienum)
                );

                Context.CurrentMethod.EmitInstruction (enumDecl.Location,
                    Opcode.StoreLocal,
                    Context.SymbolTable.AddSymbol (enumDecl.Name)
                );
            } else {
                Context.CurrentModule.SetAttribute (enumDecl.Name, ienum);
            }
        }

        public override void Accept (ContractDeclaration contractDecl)
        {
            IodineContract contract = CompileContract (contractDecl);

            if (!Context.SymbolTable.IsInGlobalScope) {
                Context.CurrentMethod.EmitInstruction (contractDecl.Location,
                    Opcode.LoadConst,
                    Context.CurrentModule.DefineConstant (contract)
                );


                Context.CurrentMethod.EmitInstruction (contractDecl.Location,
                    Opcode.StoreLocal,
                    Context.SymbolTable.AddSymbol (contractDecl.Name)
                );
            } else {
                Context.CurrentModule.SetAttribute (contractDecl.Name, contract);
            }
        }

        public override void Accept (TraitDeclaration traitDecl)
        {
            IodineTrait trait = CompileTrait (traitDecl);

            if (!Context.SymbolTable.IsInGlobalScope) {
                Context.CurrentMethod.EmitInstruction (traitDecl.Location,
                    Opcode.LoadConst,
                    Context.CurrentModule.DefineConstant (trait)
                );


                Context.CurrentMethod.EmitInstruction (traitDecl.Location,
                    Opcode.StoreLocal,
                    Context.SymbolTable.AddSymbol (traitDecl.Name)
                );
            } else {
                Context.CurrentModule.SetAttribute (traitDecl.Name, trait);
            }
        }

        public override void Accept (FunctionDeclaration funcDecl)
        {
            if (!Context.SymbolTable.IsInGlobalScope) {
                CompileLocalMethod (funcDecl);
            } else {
                Context.CurrentModule.SetAttribute (funcDecl.Name, CompileGlobalMethod (funcDecl));
            }
        }

        #endregion

        #region Statements

        public override void Accept (UseStatement useStmt)
        {
            string import = !useStmt.Relative ? useStmt.Module : Path.Combine (
                                Path.GetDirectoryName (useStmt.Location.File),
                                useStmt.Module);
            /*
			 * Implementation detail: The use statement in all reality is simply an 
			 * alias for the function require (); Here we translate the use statement
			 * into a call to the require function
			 */
            if (useStmt.Wildcard) {
                Context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.LoadConst,
                    Context.CurrentModule.DefineConstant (new IodineString (import))
                );
                Context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.BuildTuple, 0);
                Context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.LoadGlobal,
                    Context.CurrentModule.DefineConstant (new IodineName ("require"))
                );
                Context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.Invoke, 2);
                Context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.Pop);
            } else {
                IodineObject[] items = new IodineObject [useStmt.Imports.Count];

                Context.CurrentModule.Initializer.EmitInstruction (useStmt.Location,
                    Opcode.LoadConst,
                    Context.CurrentModule.DefineConstant (new IodineString (import))
                );

                if (items.Length > 0) {
                    for (int i = 0; i < items.Length; i++) {
                        items [i] = new IodineString (useStmt.Imports [i]);
                        Context.CurrentMethod.EmitInstruction (useStmt.Location,
                            Opcode.LoadConst,
                            Context.CurrentModule.DefineConstant (new IodineString (useStmt.Imports [i]))
                        );
                    }
                    Context.CurrentMethod.EmitInstruction (useStmt.Location, Opcode.BuildTuple, items.Length);
                }
                Context.CurrentMethod.EmitInstruction (useStmt.Location, Opcode.LoadGlobal,
                    Context.CurrentModule.DefineConstant (new IodineName ("require"))
                );

                Context.CurrentMethod.EmitInstruction (useStmt.Location, Opcode.Invoke,
                    items.Length == 0 ? 1 : 2
                );

                Context.CurrentMethod.EmitInstruction (useStmt.Location, Opcode.Pop);
            }

        }

        public override void Accept (CodeBlock scope)
        {
            Context.SymbolTable.EnterScope ();
            scope.VisitChildren (this);
            Context.SymbolTable.ExitScope ();
        }

        public override void Accept (StatementList stmtList)
        {
            stmtList.VisitChildren (this);
        }

        public override void Accept (Statement stmt)
        {
            stmt.VisitChildren (this);
        }

        public override void Accept (GivenStatement switchStmt)
        {
            switchStmt.GivenValue.Visit (this);

            int temporary = Context.CurrentMethod.CreateTemporary ();

            Context.CurrentMethod.EmitInstruction (switchStmt.GivenValue.Location, Opcode.StoreLocal, temporary);

            IodineLabel endSwitch = Context.CurrentMethod.CreateLabel ();

            foreach (WhenStatement caseStmt in switchStmt.WhenStatements) {
                IodineLabel nextLabel = Context.CurrentMethod.CreateLabel ();

                CreatePatternContext (temporary);
                caseStmt.Values.Visit (this);
                DestroyContext ();

                Context.CurrentMethod.EmitInstruction (caseStmt.Values.Location, Opcode.JumpIfFalse, nextLabel);

                caseStmt.Body.Visit (this);

                Context.CurrentMethod.EmitInstruction (caseStmt.Body.Location, Opcode.Jump, endSwitch);

                Context.CurrentMethod.MarkLabelPosition (nextLabel);
            }

            switchStmt.DefaultStatement.Visit (this);

            Context.CurrentMethod.MarkLabelPosition (endSwitch);
        }

        public override void Accept (TryExceptStatement tryExcept)
        {
            IodineLabel exceptLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel endLabel = Context.CurrentMethod.CreateLabel ();

            Context.CurrentMethod.EmitInstruction (tryExcept.Location, Opcode.PushExceptionHandler, exceptLabel);

            tryExcept.TryBody.Visit (this);

            Context.CurrentMethod.EmitInstruction (tryExcept.TryBody.Location, Opcode.PopExceptionHandler);
            Context.CurrentMethod.EmitInstruction (tryExcept.TryBody.Location, Opcode.Jump, endLabel);
            Context.CurrentMethod.MarkLabelPosition (exceptLabel);

            tryExcept.TypeList.Visit (this);

            if (tryExcept.TypeList.Arguments.Count > 0) {
                Context.CurrentMethod.EmitInstruction (tryExcept.ExceptBody.Location,
                    Opcode.BeginExcept,
                    tryExcept.TypeList.Arguments.Count
                );
            }

            if (tryExcept.ExceptionIdentifier != null) {
                Context.CurrentMethod.EmitInstruction (tryExcept.ExceptBody.Location, Opcode.LoadException);
                Context.CurrentMethod.EmitInstruction (tryExcept.ExceptBody.Location,
                    Opcode.StoreLocal,
                    Context.SymbolTable.AddSymbol (tryExcept.ExceptionIdentifier)
                );
            }

            tryExcept.ExceptBody.Visit (this);

            Context.CurrentMethod.MarkLabelPosition (endLabel);
        }

        public override void Accept (WithStatement withStmt)
        {
            Context.SymbolTable.EnterScope ();

            withStmt.Expression.Visit (this);

            Context.CurrentMethod.EmitInstruction (withStmt.Location, Opcode.BeginWith);

            withStmt.Body.Visit (this);

            Context.CurrentMethod.EmitInstruction (withStmt.Location, Opcode.EndWith);

            Context.SymbolTable.ExitScope ();
        }

        public override void Accept (IfStatement ifStmt)
        {
            IodineLabel elseLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel endLabel = Context.CurrentMethod.CreateLabel ();
            ifStmt.Condition.Visit (this);
            Context.CurrentMethod.EmitInstruction (ifStmt.Body.Location, Opcode.JumpIfFalse, elseLabel);
            ifStmt.Body.Visit (this);
            Context.CurrentMethod.EmitInstruction (ifStmt.ElseBody != null
				? ifStmt.ElseBody.Location
				: ifStmt.Location,
                Opcode.Jump,
                endLabel
            );
            Context.CurrentMethod.MarkLabelPosition (elseLabel);
            if (ifStmt.ElseBody != null) {
                ifStmt.ElseBody.Visit (this);
            }
            Context.CurrentMethod.MarkLabelPosition (endLabel);
        }

        public override void Accept (WhileStatement whileStmt)
        {
            IodineLabel whileLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel breakLabel = Context.CurrentMethod.CreateLabel ();

            Context.BreakLabels.Push (breakLabel);
            Context.ContinueLabels.Push (whileLabel);

            Context.CurrentMethod.MarkLabelPosition (whileLabel);

            whileStmt.Condition.Visit (this);

            Context.CurrentMethod.EmitInstruction (whileStmt.Condition.Location,
                Opcode.JumpIfFalse,
                breakLabel
            );

            whileStmt.Body.Visit (this);

            Context.CurrentMethod.EmitInstruction (whileStmt.Body.Location, Opcode.Jump, whileLabel);
            Context.CurrentMethod.MarkLabelPosition (breakLabel);

            Context.BreakLabels.Pop ();
            Context.ContinueLabels.Pop ();
        }

        public override void Accept (DoStatement doStmt)
        {
            IodineLabel doLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel breakLabel = Context.CurrentMethod.CreateLabel ();

            Context.BreakLabels.Push (breakLabel);
            Context.ContinueLabels.Push (doLabel);

            Context.CurrentMethod.MarkLabelPosition (doLabel);

            doStmt.Body.Visit (this);
            doStmt.Condition.Visit (this);

            Context.CurrentMethod.EmitInstruction (doStmt.Condition.Location,
                Opcode.JumpIfTrue,
                doLabel
            );
            Context.CurrentMethod.MarkLabelPosition (breakLabel);

            Context.BreakLabels.Pop ();
            Context.ContinueLabels.Pop ();
        }

        public override void Accept (ForStatement forStmt)
        {
            IodineLabel forLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel breakLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel skipAfterThought = Context.CurrentMethod.CreateLabel ();

            Context.BreakLabels.Push (breakLabel);
            Context.ContinueLabels.Push (forLabel);

            forStmt.Initializer.Visit (this);

            Context.CurrentMethod.EmitInstruction (forStmt.Location, Opcode.Jump, skipAfterThought);
            Context.CurrentMethod.MarkLabelPosition (forLabel);

            forStmt.AfterThought.Visit (this);

            Context.CurrentMethod.MarkLabelPosition (skipAfterThought);

            forStmt.Condition.Visit (this);

            Context.CurrentMethod.EmitInstruction (forStmt.Condition.Location, Opcode.JumpIfFalse, breakLabel);
            forStmt.Body.Visit (this);
            forStmt.AfterThought.Visit (this);
            Context.CurrentMethod.EmitInstruction (forStmt.AfterThought.Location, Opcode.Jump, skipAfterThought);
            Context.CurrentMethod.MarkLabelPosition (breakLabel);
            Context.BreakLabels.Pop ();
            Context.ContinueLabels.Pop ();
        }

        public override void Accept (ForeachStatement foreachStmt)
        {
            IodineLabel foreachLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel breakLabel = Context.CurrentMethod.CreateLabel ();
            int tmp = Context.CurrentMethod.CreateTemporary (); 

            Context.BreakLabels.Push (breakLabel);
            Context.ContinueLabels.Push (foreachLabel);

            foreachStmt.Iterator.Visit (this);

            Context.SymbolTable.EnterScope ();

            Context.CurrentMethod.EmitInstruction (foreachStmt.Iterator.Location, Opcode.GetIter);
            Context.CurrentMethod.EmitInstruction (foreachStmt.Iterator.Location, Opcode.Dup);
            Context.CurrentMethod.EmitInstruction (foreachStmt.Iterator.Location, Opcode.StoreLocal, tmp);
            Context.CurrentMethod.EmitInstruction (foreachStmt.Iterator.Location, Opcode.IterReset);
            Context.CurrentMethod.MarkLabelPosition (foreachLabel);
            Context.CurrentMethod.EmitInstruction (foreachStmt.Iterator.Location, Opcode.LoadLocal, tmp);
            Context.CurrentMethod.EmitInstruction (foreachStmt.Iterator.Location, Opcode.IterMoveNext);
            Context.CurrentMethod.EmitInstruction (foreachStmt.Iterator.Location,
                Opcode.JumpIfFalse,
                breakLabel
            );
            Context.CurrentMethod.EmitInstruction (foreachStmt.Iterator.Location, Opcode.LoadLocal, tmp);
            Context.CurrentMethod.EmitInstruction (foreachStmt.Iterator.Location, Opcode.IterGetNext);
            Context.CurrentMethod.EmitInstruction (foreachStmt.Iterator.Location,
                Opcode.StoreLocal,
                Context.SymbolTable.AddSymbol (foreachStmt.Item)
            );

            foreachStmt.Body.Visit (this);

            Context.CurrentMethod.EmitInstruction (foreachStmt.Body.Location, Opcode.Jump, foreachLabel);
            Context.CurrentMethod.MarkLabelPosition (breakLabel);

            Context.SymbolTable.ExitScope ();

            Context.BreakLabels.Pop ();
            Context.ContinueLabels.Pop ();
        }

        public override void Accept (RaiseStatement raise)
        {
            raise.Value.Visit (this);
            Context.CurrentMethod.EmitInstruction (raise.Location, Opcode.Raise);
        }

        public override void Accept (ReturnStatement returnStmt)
        {
            returnStmt.VisitChildren (this);
            Context.CurrentMethod.EmitInstruction (returnStmt.Location, Opcode.Return);
        }

        public override void Accept (YieldStatement yieldStmt)
        {
            yieldStmt.VisitChildren (this);
            Context.CurrentMethod.Generator = true;
            Context.CurrentMethod.EmitInstruction (yieldStmt.Location, Opcode.Yield);
        }

        public override void Accept (BreakStatement brk)
        {
            Context.CurrentMethod.EmitInstruction (brk.Location,
                Opcode.Jump,
                Context.BreakLabels.Peek ()
            );
        }

        public override void Accept (ContinueStatement cont)
        {
            Context.CurrentMethod.EmitInstruction (cont.Location,
                Opcode.Jump,
                Context.ContinueLabels.Peek ()
            );
        }

        public override void Accept (SuperCallStatement super)
        {
            string[] subclass = super.Parent.Base [0].Split ('.');
            super.Arguments.Visit (this);

            Context.CurrentMethod.EmitInstruction (super.Location,
                Opcode.LoadGlobal,
                Context.CurrentModule.DefineConstant (new IodineName (subclass [0]))
            );

            for (int i = 1; i < subclass.Length; i++) {
                Context.CurrentMethod.EmitInstruction (super.Location,
                    Opcode.LoadAttribute,
                    Context.CurrentModule.DefineConstant (new IodineName (subclass [0]))
                );
            }

            Context.CurrentMethod.EmitInstruction (super.Location,
                Opcode.InvokeSuper,
                super.Arguments.Arguments.Count
            );

            for (int i = 1; i < super.Parent.Base.Count; i++) {
                string[] contract = super.Parent.Base [i].Split ('.');
                Context.CurrentMethod.EmitInstruction (super.Location,
                    Opcode.LoadGlobal,
                    Context.CurrentModule.DefineConstant (new IodineName (contract [0]))
                );

                for (int j = 1; j < contract.Length; j++) {
                    Context.CurrentMethod.EmitInstruction (super.Location,
                        Opcode.LoadAttribute,
                        Context.CurrentModule.DefineConstant (new IodineName (contract [0]))
                    );
                }
                Context.CurrentMethod.EmitInstruction (super.Location, Opcode.InvokeSuper, 0);
            }
        }

        public override void Accept (AssignStatement assignStmt)
        {
            if (assignStmt.Packed) {
                CompileAssignWithAutoUnpack (assignStmt);
            } else {
                for (int i = 0; i < assignStmt.Identifiers.Count; i++) {
                    assignStmt.Expressions [i].Visit (this);
                    string ident = assignStmt.Identifiers [i];
                    if (symbolTable.IsGlobal (ident)) {
                        Context.CurrentMethod.EmitInstruction (
                            Opcode.StoreGlobal,
                            Context.CurrentModule.DefineConstant (new IodineName (ident))
                        );
                    } else {
                        int localIndex = symbolTable.IsSymbolDefined (ident) ?
                            symbolTable.GetSymbolIndex (ident) :
                            symbolTable.AddSymbol (ident);
                        Context.CurrentMethod.EmitInstruction (
                            Opcode.StoreLocal,
                            localIndex
                        );
                    }
                }
            }
        }


        private void CompileAssignWithAutoUnpack (AssignStatement assignStmt)
        {
            int tmp = Context.CurrentMethod.CreateTemporary ();

            assignStmt.Expressions [0].Visit (this);

            Context.CurrentMethod.EmitInstruction (Opcode.StoreLocal, tmp);

            for (int i = 0; i < assignStmt.Identifiers.Count; i++) {

                string ident = assignStmt.Identifiers [i];

                Context.CurrentMethod.EmitInstruction (Opcode.LoadLocal, tmp);

                Context.CurrentMethod.EmitInstruction (
                    Opcode.LoadConst,
                    Context.CurrentModule.DefineConstant (new IodineInteger (i))
                );

                Context.CurrentMethod.EmitInstruction (Opcode.LoadIndex);

                if (symbolTable.IsGlobal (ident)) {
                    Context.CurrentMethod.EmitInstruction (
                        Opcode.StoreGlobal,
                        Context.CurrentModule.DefineConstant (new IodineName (ident))
                    );
                } else {
                    int localIndex = symbolTable.IsSymbolDefined (ident) ?
                        symbolTable.GetSymbolIndex (ident) :
                        symbolTable.AddSymbol (ident);
                    Context.CurrentMethod.EmitInstruction (
                        Opcode.StoreLocal,
                        localIndex
                    );
                }
            }
        }

        public override void Accept (Expression expr)
        {
            expr.VisitChildren (this);
            Context.CurrentMethod.EmitInstruction (expr.Location, Opcode.Pop);
        }

        #endregion

        #region Expressions

        public override void Accept (TernaryExpression ifExpr)
        {
            IodineLabel elseLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel endLabel = Context.CurrentMethod.CreateLabel ();
            ifExpr.Condition.Visit (this);
            Context.CurrentMethod.EmitInstruction (ifExpr.Expression.Location,
                Opcode.JumpIfFalse,
                elseLabel
            );
            ifExpr.Expression.Visit (this);
            Context.CurrentMethod.EmitInstruction (ifExpr.ElseExpression.Location,
                Opcode.Jump,
                endLabel
            );
            Context.CurrentMethod.MarkLabelPosition (elseLabel);
            ifExpr.ElseExpression.Visit (this);
            Context.CurrentMethod.MarkLabelPosition (endLabel);
        }

        public override void Accept (LambdaExpression lambda)
        {
            Context.SymbolTable.EnterScope ();

            MethodBuilder anonMethod = new MethodBuilder (Context.CurrentMethod,
                Context.CurrentModule,
                null,
                lambda.InstanceMethod, 
                lambda.Parameters.Count,
                lambda.Variadic,
                lambda.AcceptsKeywordArguments
            );

            for (int i = 0; i < lambda.Parameters.Count; i++) {
                anonMethod.Parameters [lambda.Parameters [i]] =
					Context.SymbolTable.AddSymbol (lambda.Parameters [i]);
            }

            CreateContext (anonMethod);

            lambda.VisitChildren (this);

            DestroyContext ();

            anonMethod.EmitInstruction (lambda.Location, Opcode.LoadNull);
            anonMethod.FinalizeLabels ();

            Context.CurrentMethod.EmitInstruction (lambda.Location,
                Opcode.LoadConst,
                Context.CurrentModule.DefineConstant (anonMethod)
            );

            Context.CurrentMethod.EmitInstruction (lambda.Location, Opcode.BuildClosure);

            Context.SymbolTable.ExitScope ();
        }

        public override void Accept (BinaryExpression binop)
        {
            if (binop.Operation == BinaryOperation.Assign) {
                binop.Right.Visit (this);
                if (binop.Left is NameExpression) {
                    NameExpression ident = (NameExpression)binop.Left;

                    if (!Context.SymbolTable.IsGlobal (ident.Value) || !Context.SymbolTable.IsSymbolDefined (ident.Value)) {

                        if (!Context.SymbolTable.IsSymbolDefined (ident.Value)) {
                            Context.SymbolTable.AddSymbol (ident.Value);
                        }
                        int sym = Context.SymbolTable.GetSymbolIndex (ident.Value);
                        Context.CurrentMethod.EmitInstruction (ident.Location, Opcode.StoreLocal, sym);
                        Context.CurrentMethod.EmitInstruction (ident.Location, Opcode.LoadLocal, sym);
                    } else {
                        int globalIndex = Context.CurrentModule.DefineConstant (new IodineName (ident.Value));
                        Context.CurrentMethod.EmitInstruction (ident.Location, Opcode.StoreGlobal, globalIndex);
                        Context.CurrentMethod.EmitInstruction (ident.Location, Opcode.LoadGlobal, globalIndex);
                    }
                } else if (binop.Left is MemberExpression) {
                    MemberExpression getattr = binop.Left as MemberExpression;
                    getattr.Target.Visit (this);
                    int attrIndex = Context.CurrentModule.DefineConstant (new IodineName (getattr.Field));
                    Context.CurrentMethod.EmitInstruction (getattr.Location, Opcode.StoreAttribute, attrIndex);
                    getattr.Target.Visit (this);
                    Context.CurrentMethod.EmitInstruction (getattr.Location, Opcode.LoadAttribute, attrIndex);
                } else if (binop.Left is IndexerExpression) {
                    IndexerExpression indexer = binop.Left as IndexerExpression;
                    indexer.Target.Visit (this);
                    indexer.Index.Visit (this);
                    Context.CurrentMethod.EmitInstruction (indexer.Location, Opcode.StoreIndex);
                    binop.Left.Visit (this);
                }
                return;
            } 

            switch (binop.Operation) {
            case BinaryOperation.InstanceOf:
                binop.Right.Visit (this);
                binop.Left.Visit (this);
                Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.InstanceOf);
                return;
            case BinaryOperation.NotInstanceOf:
                binop.Right.Visit (this);
                binop.Left.Visit (this);
                Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.InstanceOf);
                Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.UnaryOp, (int)UnaryOperation.BoolNot);
                return;
            case BinaryOperation.DynamicCast:
                binop.Right.Visit (this);
                binop.Left.Visit (this);
                Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.DynamicCast);
                return;
            case BinaryOperation.NullCoalescing:
                binop.Right.Visit (this);
                binop.Left.Visit (this);
                Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.DynamicCast);
                return;
            case BinaryOperation.Add:
            case BinaryOperation.Sub:
            case BinaryOperation.Mul:
            case BinaryOperation.Div:
            case BinaryOperation.Mod:
            case BinaryOperation.And:
            case BinaryOperation.Or:
            case BinaryOperation.Xor:
            case BinaryOperation.GreaterThan:
            case BinaryOperation.GreaterThanOrEqu:
            case BinaryOperation.LessThan:
            case BinaryOperation.LessThanOrEqu:
            case BinaryOperation.ClosedRange:
            case BinaryOperation.HalfRange:
            case BinaryOperation.LeftShift:
            case BinaryOperation.RightShift:
                binop.Left.Visit (this);
                binop.Right.Visit (this);
                Context.CurrentMethod.EmitInstruction (binop.Location,
                    Opcode.BinOp,
                    (int)binop.Operation
                );
                return;
            }

            IodineLabel shortCircuitTrueLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel shortCircuitFalseLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel endLabel = Context.CurrentMethod.CreateLabel ();
            binop.Left.Visit (this);

            /*
			 * Short circuit evaluation 
			 */
            switch (binop.Operation) {
            case BinaryOperation.BoolAnd:
                Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.Dup);
                Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.JumpIfFalse,
                    shortCircuitFalseLabel);
                break;
            case BinaryOperation.BoolOr:
                Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.Dup);
                Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.JumpIfTrue,
                    shortCircuitTrueLabel);
                break;
            }
            binop.Right.Visit (this);

            Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.BinOp, (int)binop.Operation);
            Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.Jump, endLabel);
            Context.CurrentMethod.MarkLabelPosition (shortCircuitTrueLabel);
            Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.Pop);
            Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.LoadTrue);
            Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.Jump, endLabel);
            Context.CurrentMethod.MarkLabelPosition (shortCircuitFalseLabel);
            Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.Pop);
            Context.CurrentMethod.EmitInstruction (binop.Location, Opcode.LoadFalse);
            Context.CurrentMethod.MarkLabelPosition (endLabel);
        }

        public override void Accept (UnaryExpression unaryop)
        {
            unaryop.VisitChildren (this);
            Context.CurrentMethod.EmitInstruction (unaryop.Location, Opcode.UnaryOp, (int)unaryop.Operation);
        }

        public override void Accept (CallExpression call)
        {
            call.Arguments.Visit (this);
            call.Target.Visit (this);
            if (call.Arguments.Packed) {
                Context.CurrentMethod.EmitInstruction (call.Target.Location, 
                    Opcode.InvokeVar, 
                    call.Arguments.Arguments.Count - 1
                );
            } else {
                Context.CurrentMethod.EmitInstruction (call.Target.Location,
                    Opcode.Invoke, 
                    call.Arguments.Arguments.Count
                );
            }

            if (Context.IsPatternExpression) {
                Context.CurrentMethod.EmitInstruction (call.Location,
                    Opcode.LoadLocal,
                    Context.PatternTemporary
                );
                Context.CurrentMethod.EmitInstruction (call.Location,
                    Opcode.BinOp,
                    (int)BinaryOperation.Equals
                );
            }
        }

        public override void Accept (ArgumentList arglist)
        {
            arglist.VisitChildren (this);
        }

        public override void Accept (KeywordArgumentList kwargs)
        {
            foreach (KeyValuePair<string, AstNode> kv in kwargs.Keywords) {
                string kw = kv.Key;
                AstNode val = kv.Value;
                Context.CurrentMethod.EmitInstruction (kwargs.Location,
                    Opcode.LoadConst,
                    Context.CurrentModule.DefineConstant (new IodineString (kw))
                );
                val.Visit (this);
                Context.CurrentMethod.EmitInstruction (kwargs.Location, Opcode.BuildTuple, 2);
            }
            Context.CurrentMethod.EmitInstruction (kwargs.Location, Opcode.BuildList, kwargs.Keywords.Count);
            Context.CurrentMethod.EmitInstruction (kwargs.Location,
                Opcode.LoadGlobal,
                Context.CurrentModule.DefineConstant (new IodineName ("Dict"))
            );
            Context.CurrentMethod.EmitInstruction (kwargs.Location, Opcode.Invoke, 1);
        }

        public override void Accept (MemberExpression getAttr)
        {
            if (Context.IsPatternExpression) {
                CreateContext ();

                getAttr.Target.Visit (this);

                DestroyContext ();

                Context.CurrentMethod.EmitInstruction (getAttr.Location,
                    Opcode.LoadAttribute,
                    Context.CurrentModule.DefineConstant (new IodineName (getAttr.Field))
                );
                Context.CurrentMethod.EmitInstruction (getAttr.Location,
                    Opcode.LoadLocal,
                    Context.PatternTemporary
                );
                Context.CurrentMethod.EmitInstruction (getAttr.Location,
                    Opcode.BinOp,
                    (int)BinaryOperation.Equals
                );
            } else {
                getAttr.Target.Visit (this);

                Context.CurrentMethod.EmitInstruction (getAttr.Location,
                    Opcode.LoadAttribute,
                    Context.CurrentModule.DefineConstant (new IodineName (getAttr.Field))
                );

            }
        }

        public override void Accept (MemberDefaultExpression getAttr)
        {
            getAttr.Target.Visit (this);
            Context.CurrentMethod.EmitInstruction (getAttr.Location,
                Opcode.LoadAttributeOrNull,
                Context.CurrentModule.DefineConstant (new IodineName (getAttr.Field))
            );

            if (Context.IsPatternExpression) {
                Context.CurrentMethod.EmitInstruction (getAttr.Location,
                    Opcode.LoadLocal,
                    Context.PatternTemporary
                );
                Context.CurrentMethod.EmitInstruction (getAttr.Location,
                    Opcode.BinOp,
                    (int)BinaryOperation.Equals
                );
            }
        }

        public override void Accept (IndexerExpression indexer)
        {
            indexer.Target.Visit (this);
            indexer.Index.Visit (this);
            Context.CurrentMethod.EmitInstruction (indexer.Location, Opcode.LoadIndex);

        }

        public override void Accept (SliceExpression slice)
        {
            slice.VisitChildren (this);

            Context.CurrentMethod.EmitInstruction (slice.Location, Opcode.Slice);

        }

        public override void Accept (GeneratorExpression genExpr)
        {

            MethodBuilder anonMethod = new MethodBuilder (Context.CurrentMethod,
                Context.CurrentModule,
                null,
                false,
                0,
                false,
                false
            );

            CreateContext (anonMethod);

            Context.SymbolTable.EnterScope ();

            IodineLabel foreachLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel breakLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel predicateSkip = Context.CurrentMethod.CreateLabel ();

            int tmp = Context.CurrentMethod.CreateTemporary (); 

            genExpr.Iterator.Visit (this);

            Context.CurrentMethod.EmitInstruction (genExpr.Iterator.Location, Opcode.GetIter);
            Context.CurrentMethod.EmitInstruction (genExpr.Iterator.Location, Opcode.Dup);
            Context.CurrentMethod.EmitInstruction (genExpr.Iterator.Location, Opcode.StoreLocal, tmp);
            Context.CurrentMethod.EmitInstruction (genExpr.Iterator.Location, Opcode.IterReset);
            Context.CurrentMethod.MarkLabelPosition (foreachLabel);
            Context.CurrentMethod.EmitInstruction (genExpr.Iterator.Location, Opcode.LoadLocal, tmp);
            Context.CurrentMethod.EmitInstruction (genExpr.Iterator.Location, Opcode.IterMoveNext);

            Context.CurrentMethod.EmitInstruction (genExpr.Iterator.Location,
                Opcode.JumpIfFalse,
                breakLabel
            );

            Context.CurrentMethod.EmitInstruction (genExpr.Iterator.Location, Opcode.LoadLocal, tmp);
            Context.CurrentMethod.EmitInstruction (genExpr.Iterator.Location, Opcode.IterGetNext);
            Context.CurrentMethod.EmitInstruction (genExpr.Iterator.Location,
                Opcode.StoreLocal,
                Context.SymbolTable.AddSymbol (genExpr.Identifier)
            );

            if (genExpr.Predicate != null) {
                genExpr.Predicate.Visit (this);
                Context.CurrentMethod.EmitInstruction (genExpr.Iterator.Location, Opcode.JumpIfFalse, predicateSkip);
            }

            genExpr.Expression.Visit (this);

            Context.CurrentMethod.EmitInstruction (genExpr.Expression.Location, Opcode.Yield);

            if (genExpr.Predicate != null) {
                Context.CurrentMethod.MarkLabelPosition (predicateSkip);
            }

            Context.CurrentMethod.EmitInstruction (genExpr.Expression.Location, Opcode.Jump, foreachLabel);
            Context.CurrentMethod.MarkLabelPosition (breakLabel);
            Context.CurrentMethod.EmitInstruction (genExpr.Location, Opcode.LoadNull);


            Context.SymbolTable.ExitScope ();

            anonMethod.FinalizeLabels ();

            DestroyContext ();

            Context.CurrentMethod.EmitInstruction (genExpr.Location,
                Opcode.LoadConst,
                Context.CurrentModule.DefineConstant (anonMethod)
            );

            Context.CurrentMethod.EmitInstruction (genExpr.Location, Opcode.BuildGenExpr);
        }

        public override void Accept (ListCompExpression list)
        {
            IodineLabel foreachLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel breakLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel predicateSkip = Context.CurrentMethod.CreateLabel ();
            int tmp = Context.CurrentMethod.CreateTemporary (); 
            int set = Context.CurrentMethod.CreateTemporary ();

            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.BuildList, 0);

            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.StoreLocal, set);

            Context.SymbolTable.EnterScope ();

            list.Iterator.Visit (this);

            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.GetIter);
            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.Dup);
            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.StoreLocal, tmp);
            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.IterReset);
            Context.CurrentMethod.MarkLabelPosition (foreachLabel);
            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.LoadLocal, tmp);
            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.IterMoveNext);
            Context.CurrentMethod.EmitInstruction (list.Iterator.Location,
                Opcode.JumpIfFalse,
                breakLabel
            );

            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.LoadLocal, tmp);
            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.IterGetNext);
            Context.CurrentMethod.EmitInstruction (list.Iterator.Location,
                Opcode.StoreLocal,
                Context.SymbolTable.AddSymbol (list.Identifier)
            );

            if (list.Predicate != null) {
                list.Predicate.Visit (this);
                Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.JumpIfFalse, predicateSkip);
            }

            list.Expression.Visit (this);

            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.LoadLocal, set);
            Context.CurrentMethod.EmitInstruction (list.Iterator.Location,
                Opcode.LoadAttribute,
                Context.CurrentModule.DefineConstant (new IodineName ("append"))
            );
            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.Invoke, 1);
            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.Pop);
            if (list.Predicate != null) {
                Context.CurrentMethod.MarkLabelPosition (predicateSkip);
            }
            Context.CurrentMethod.EmitInstruction (list.Expression.Location, Opcode.Jump, foreachLabel);
            Context.CurrentMethod.MarkLabelPosition (breakLabel);
            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.LoadLocal, set);

            Context.SymbolTable.ExitScope ();
        }

        public override void Accept (ListExpression list)
        {
            list.VisitChildren (this);
            Context.CurrentMethod.EmitInstruction (list.Location, Opcode.BuildList, list.Items.Count);
        }

        public override void Accept (TupleExpression tuple)
        {
            if (Context.IsPatternExpression) {
                IodineLabel startLabel = Context.CurrentMethod.CreateLabel ();
                IodineLabel endLabel = Context.CurrentMethod.CreateLabel ();
                int item = Context.CurrentMethod.CreateTemporary ();

                int prevTemporary = Context.PatternTemporary;

                CreatePatternContext (item);

                for (int i = 0; i < tuple.Items.Count; i++) {
                    if (tuple.Items [i] is NameExpression &&
                        ((NameExpression)tuple.Items [i]).Value == "_")
                        continue;
                    Context.CurrentMethod.EmitInstruction (tuple.Location, Opcode.LoadLocal, prevTemporary);
                    Context.CurrentMethod.EmitInstruction (tuple.Location,
                        Opcode.LoadConst,
                        Context.CurrentModule.DefineConstant (new IodineInteger (i))
                    );
                    Context.CurrentMethod.EmitInstruction (tuple.Location, Opcode.LoadIndex);
                    Context.CurrentMethod.EmitInstruction (tuple.Location, Opcode.StoreLocal, item);

                    tuple.Items [i].Visit (this);

                    Context.CurrentMethod.EmitInstruction (tuple.Location, Opcode.JumpIfFalse, endLabel);
                }

                DestroyContext ();

                Context.CurrentMethod.EmitInstruction (tuple.Location, Opcode.LoadTrue);
                Context.CurrentMethod.EmitInstruction (tuple.Location, Opcode.Jump, startLabel);

                Context.CurrentMethod.MarkLabelPosition (endLabel);
                Context.CurrentMethod.EmitInstruction (tuple.Location, Opcode.LoadFalse);

                Context.CurrentMethod.MarkLabelPosition (startLabel);
            } else {
                tuple.VisitChildren (this);
                Context.CurrentMethod.EmitInstruction (tuple.Location, Opcode.BuildTuple, tuple.Items.Count);
            }
        }

        public override void Accept (HashExpression hash)
        {
            hash.VisitChildren (this);
            Context.CurrentMethod.EmitInstruction (hash.Location, Opcode.BuildHash, hash.Items.Count / 2);
        }

        #endregion

        #region PatternExpression

        public override void Accept (MatchExpression match)
        {
            AstNode value = match.Expression;
            value.Visit (this);
            int temporary = Context.CurrentMethod.CreateTemporary ();
            Context.CurrentMethod.EmitInstruction (match.Location, Opcode.StoreLocal, temporary);

            IodineLabel nextLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel endLabel = Context.CurrentMethod.CreateLabel ();

            for (int i = 0; i < match.MatchCases.Count; i++) {
                if (i > 0) {
                    Context.CurrentMethod.MarkLabelPosition (nextLabel);
                    nextLabel = Context.CurrentMethod.CreateLabel ();
                }
                CaseExpression clause = match.MatchCases [i] as CaseExpression;

                CreatePatternContext (temporary);

                Context.SymbolTable.EnterScope ();

                clause.Pattern.Visit (this);

                DestroyContext ();

                Context.CurrentMethod.EmitInstruction (match.Location, Opcode.JumpIfFalse, nextLabel);

                if (clause.Condition != null) {
                    clause.Condition.Visit (this);
                    Context.CurrentMethod.EmitInstruction (match.Location, Opcode.JumpIfFalse, nextLabel);
                }

                clause.Value.Visit (this);

                if (clause.IsStatement) {
                    Context.CurrentMethod.EmitInstruction (match.Location, Opcode.LoadNull);
                }

                Context.SymbolTable.ExitScope ();

                Context.CurrentMethod.EmitInstruction (match.Location, Opcode.Jump, endLabel);
            }
            Context.CurrentMethod.MarkLabelPosition (endLabel);
        }

        public override void Accept (PatternExpression expression)
        {
            IodineLabel shortCircuitTrueLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel shortCircuitFalseLabel = Context.CurrentMethod.CreateLabel ();
            IodineLabel endLabel = Context.CurrentMethod.CreateLabel ();
            expression.Left.Visit (this);

            /*
			 * Short circuit evaluation 
			 */
            switch (expression.Operation) {
            case BinaryOperation.And:
                Context.CurrentMethod.EmitInstruction (expression.Location, Opcode.Dup);
                Context.CurrentMethod.EmitInstruction (expression.Location,
                    Opcode.JumpIfFalse,
                    shortCircuitFalseLabel
                );
                break;
            case BinaryOperation.Or:
                Context.CurrentMethod.EmitInstruction (expression.Location, Opcode.Dup);
                Context.CurrentMethod.EmitInstruction (expression.Location,
                    Opcode.JumpIfTrue,
                    shortCircuitTrueLabel
                );
                break;
            }
            expression.Right.Visit (this);

            Context.CurrentMethod.EmitInstruction (expression.Location, Opcode.BinOp, (int)expression.Operation);
            Context.CurrentMethod.EmitInstruction (expression.Location, Opcode.Jump, endLabel);
            Context.CurrentMethod.MarkLabelPosition (shortCircuitTrueLabel);
            Context.CurrentMethod.EmitInstruction (expression.Location, Opcode.Pop);
            Context.CurrentMethod.EmitInstruction (expression.Location, Opcode.LoadTrue);
            Context.CurrentMethod.EmitInstruction (expression.Location, Opcode.Jump, endLabel);
            Context.CurrentMethod.MarkLabelPosition (shortCircuitFalseLabel);
            Context.CurrentMethod.EmitInstruction (expression.Location, Opcode.Pop);
            Context.CurrentMethod.EmitInstruction (expression.Location, Opcode.LoadFalse);
            Context.CurrentMethod.MarkLabelPosition (endLabel);
        }

        #endregion

        #region Terminals

        public override void Accept (NameExpression ident)
        {
            if (Context.IsPatternExpression) {
                if (ident.Value == "_") {
                    Context.CurrentMethod.EmitInstruction (ident.Location, Opcode.LoadTrue);
                } else {
                    Context.CurrentMethod.EmitInstruction (ident.Location,
                        Opcode.LoadLocal,
                        Context.PatternTemporary
                    );
                    Context.CurrentMethod.EmitInstruction (ident.Location,
                        Opcode.StoreLocal,
                        Context.SymbolTable.AddSymbol (ident.Value)
                    );
                    Context.CurrentMethod.EmitInstruction (ident.Location, Opcode.LoadTrue);
                }
                return;
            }

            if (Context.SymbolTable.IsSymbolDefined (ident.Value)) {
                if (!Context.SymbolTable.IsGlobal (ident.Value)) {
                    int sym = Context.SymbolTable.GetSymbolIndex (ident.Value);
                    Context.CurrentMethod.EmitInstruction (ident.Location, Opcode.LoadLocal, sym);
                } else {
                    Context.CurrentMethod.EmitInstruction (ident.Location,
                        Opcode.LoadGlobal,
                        Context.CurrentModule.DefineConstant (new IodineName (ident.Value))
                    );
                }
            } else if (Context.IsInClass && ExistsInOuterClass (ident.Value)) {
                LoadAssociatedClass (ident.Value);
                Context.CurrentMethod.EmitInstruction (ident.Location,
                    Opcode.LoadAttribute,
                    Context.CurrentModule.DefineConstant (new IodineName (ident.Value))
                );
            } else {
                Context.CurrentMethod.EmitInstruction (ident.Location,
                    Opcode.LoadGlobal,
                    Context.CurrentModule.DefineConstant (new IodineName (ident.Value))
                );
            }

        }

        private bool ExistsInOuterClass (string name)
        {
            ClassBuilder current = Context.CurrentClass;

            while (current != null) {
                if (current.HasAttribute (name)) {
                    return true;
                }
                current = current.ParentClass;
            }

            return false;
        }

        /*
		 * Emits the instructions required for loading the class that contains
		 * the attribute 'item'
		 */
        private void LoadAssociatedClass (string item = null)
        {
            ClassBuilder current = Context.CurrentClass;
            List<string> names = new List<string> ();
            bool reachedClass = (item == null);

            while (current != null) {
                if (!reachedClass && current.HasAttribute (item)) {
                    reachedClass = true;
                }

                if (reachedClass) {
                    names.Add (current.Name);
                }

                current = current.ParentClass;
            }

            names.Reverse ();

            Context.CurrentMethod.EmitInstruction (Opcode.LoadGlobal,
                Context.CurrentModule.DefineConstant (new IodineName (names [0]))
            );

            for (int i = 1; i < names.Count; i++) {
                Context.CurrentMethod.EmitInstruction (Opcode.LoadAttribute,
                    Context.CurrentModule.DefineConstant (new IodineName (names [i]))
                );
            }
        }

        public override void Accept (IntegerExpression integer)
        {
            Context.CurrentMethod.EmitInstruction (integer.Location,
                Opcode.LoadConst, 
                Context.CurrentModule.DefineConstant (new IodineInteger (integer.Value))
            );

            if (Context.IsPatternExpression) {
                Context.CurrentMethod.EmitInstruction (integer.Location,
                    Opcode.LoadLocal,
                    Context.PatternTemporary
                );
                Context.CurrentMethod.EmitInstruction (integer.Location,
                    Opcode.BinOp,
                    (int)BinaryOperation.Equals
                );
            }
        }

        public override void Accept (FloatExpression num)
        {
            Context.CurrentMethod.EmitInstruction (num.Location,
                Opcode.LoadConst, 
                Context.CurrentModule.DefineConstant (new IodineFloat (num.Value))
            );

            if (Context.IsPatternExpression) {
                Context.CurrentMethod.EmitInstruction (num.Location,
                    Opcode.LoadLocal,
                    Context.PatternTemporary
                );
                Context.CurrentMethod.EmitInstruction (num.Location,
                    Opcode.BinOp,
                    (int)BinaryOperation.Equals
                );
            }
        }

        public override void Accept (StringExpression str)
        {
            str.VisitChildren (this); // A string can contain a list of sub expressions for string interpolation

            IodineObject constant = str.Binary ?
				(IodineObject)new IodineBytes (str.Value) :
				(IodineObject)new IodineString (str.Value);

            Context.CurrentMethod.EmitInstruction (str.Location,
                Opcode.LoadConst, 
                Context.CurrentModule.DefineConstant (constant)
            );

            if (str.SubExpressions.Count != 0) {
                Context.CurrentMethod.EmitInstruction (str.Location,
                    Opcode.LoadAttribute,
                    Context.CurrentModule.DefineConstant (new IodineName ("format"))
                );
                Context.CurrentMethod.EmitInstruction (str.Location, Opcode.Invoke, str.SubExpressions.Count);
            }

            if (Context.IsPatternExpression) {
                Context.CurrentMethod.EmitInstruction (str.Location,
                    Opcode.LoadLocal,
                    Context.PatternTemporary
                );
                Context.CurrentMethod.EmitInstruction (str.Location,
                    Opcode.BinOp,
                    (int)BinaryOperation.Equals
                );
            }
        }

        public override void Accept (SelfExpression self)
        {
            Context.CurrentMethod.EmitInstruction (self.Location, Opcode.LoadSelf);
        }

        public override void Accept (NullExpression nil)
        {
            Context.CurrentMethod.EmitInstruction (nil.Location, Opcode.LoadNull);

            if (Context.IsPatternExpression) {
                Context.CurrentMethod.EmitInstruction (nil.Location,
                    Opcode.LoadLocal,
                    Context.PatternTemporary
                );
                Context.CurrentMethod.EmitInstruction (nil.Location,
                    Opcode.BinOp,
                    (int)BinaryOperation.Equals
                );
            }
        }

        public override void Accept (TrueExpression ntrue)
        {
            Context.CurrentMethod.EmitInstruction (ntrue.Location, Opcode.LoadTrue);

            if (Context.IsPatternExpression) {
                Context.CurrentMethod.EmitInstruction (ntrue.Location,
                    Opcode.LoadLocal,
                    Context.PatternTemporary
                );
                Context.CurrentMethod.EmitInstruction (ntrue.Location,
                    Opcode.BinOp,
                    (int)BinaryOperation.Equals
                );
            }
        }

        public override void Accept (FalseExpression nfalse)
        {
            Context.CurrentMethod.EmitInstruction (nfalse.Location, Opcode.LoadFalse);

            if (Context.IsPatternExpression) {
                Context.CurrentMethod.EmitInstruction (nfalse.Location,
                    Opcode.LoadLocal,
                    Context.PatternTemporary
                );
                Context.CurrentMethod.EmitInstruction (nfalse.Location,
                    Opcode.BinOp,
                    (int)BinaryOperation.Equals
                );
            }
        }

        #endregion
    }
}

