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
    public class IodineCompiler : AstVisitor
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
        private int _nextTemporary = 2048;

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

        public IodineModule Compile (string moduleName, string filePath)
        {
            ModuleBuilder moduleBuilder = new ModuleBuilder (moduleName, filePath);
            EmitContext context = new EmitContext (symbolTable, moduleBuilder, moduleBuilder.Initializer);

            context.SetCurrentModule (moduleBuilder);

            emitContexts.Push (context);

            root.Visit (this);

            moduleBuilder.Initializer.FinalizeMethod ();

            DestroyContext ();

            return moduleBuilder;
        }

        private void OptimizeObject (CodeBuilder code)
        {
            foreach (IBytecodeOptimization opt in Optimizations) {
                opt.PerformOptimization (code);
            }
        }

        private void CreateContext (bool isInClassBody = false)
        {
            emitContexts.Push (new EmitContext (Context.SymbolTable,
                Context.CurrentModule,
                Context.CurrentMethod,
                Context.IsInClass,
                isInClassBody
            ));
        }

        private void CreateContext (CodeBuilder methodBuilder)
        {
            emitContexts.Push (new EmitContext (Context.SymbolTable,
                Context.CurrentModule,
                methodBuilder,
                Context.IsInClass
            ));
        }

        private void CreatePatternContext (int temporary)
        {
            emitContexts.Push (new EmitContext (Context.SymbolTable,
                Context.CurrentModule,
                Context.CurrentMethod,
                Context.IsInClass,
                false,
                true,
                temporary
            ));
        }

        private void DestroyContext ()
        {
            emitContexts.Pop ();
        }

        private int CreateTemporary ()
        {
            return Context.CurrentModule.DefineConstant (
                new IodineName ("$tmp" + (_nextTemporary++).ToString ())
            );
        }

        private int CreateName (string name)
        {
            int i= Context.CurrentModule.DefineConstant (
                new IodineName (name)
            );
            return i;
        }

        public override void Accept (CompilationUnit ast)
        {
            ast.VisitChildren (this);
        }

        #region Declarations

        private void CompileClass (ClassDeclaration classDecl)
        {
            Context.SymbolTable.AddSymbol (classDecl.Name);

            CreateContext (true);

            foreach (AstNode member in classDecl.Members) {
                member.Visit (this);
            }

            DestroyContext ();

            foreach (AstNode contract in classDecl.Interfaces) {
                contract.Visit (this);
            }

            Context.CurrentMethod.EmitInstruction (Opcode.BuildTuple, classDecl.Interfaces.Count);

            if (classDecl.BaseClass != null) {
                classDecl.BaseClass.Visit (this);
            } else {
                Context.CurrentMethod.EmitInstruction (Opcode.LoadNull);
            }

            CompileMethod (classDecl.Constructor);

            Context.CurrentMethod.EmitInstruction (
                Opcode.LoadConst,
                Context.CurrentModule.DefineConstant (new IodineString (classDecl.Documentation))
            );

            Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName (classDecl.Name));

            Context.CurrentMethod.EmitInstruction (Opcode.BuildClass, classDecl.Members.Count);

        }

        private void CompileContract (ContractDeclaration contractDecl)
        {
            Context.SymbolTable.AddSymbol (contractDecl.Name);

            foreach (AstNode member in contractDecl.Members) {
                if (member is FunctionDeclaration) {
                    FunctionDeclaration funcDecl = member as FunctionDeclaration;
                    CompileMethod (funcDecl);
                }
            }

            Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName (contractDecl.Name));

            Context.CurrentMethod.EmitInstruction (Opcode.BuildContract, contractDecl.Members.Count);

        }
            
        private void CompileTrait (TraitDeclaration traitDecl)
        {
            Context.SymbolTable.AddSymbol (traitDecl.Name);

            foreach (AstNode member in traitDecl.Members) {
                if (member is FunctionDeclaration) {
                    FunctionDeclaration funcDecl = member as FunctionDeclaration;
                    CompileMethod (funcDecl);
                }
            }

            Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName (traitDecl.Name));

            Context.CurrentMethod.EmitInstruction (Opcode.BuildTrait, traitDecl.Members.Count);

        }

        private void CompileMixin (MixinDeclaration mixinDecl)
        {
            Context.SymbolTable.AddSymbol (mixinDecl.Name);

            foreach (AstNode member in mixinDecl.Members) {
                if (member is FunctionDeclaration) {
                    FunctionDeclaration funcDecl = member as FunctionDeclaration;
                    CompileMethod (funcDecl);
                    Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName (funcDecl.Name));
                }
            }

            Context.CurrentMethod.EmitInstruction (
                Opcode.LoadConst,
                Context.CurrentModule.DefineConstant (new IodineString (mixinDecl.Documentation))
            );

            Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName (mixinDecl.Name));

            Context.CurrentMethod.EmitInstruction (Opcode.BuildClass, mixinDecl.Members.Count);

        }

        private void CompileEnum (EnumDeclaration enumDecl)
        {
            Context.SymbolTable.AddSymbol (enumDecl.Name);

            foreach (KeyValuePair<string, int> key in enumDecl.Items) {
                Context.CurrentMethod.EmitInstruction (
                    Opcode.LoadConst,
                    CreateName (key.Key)
                );

                Context.CurrentMethod.EmitInstruction (
                    Opcode.LoadConst, 
                    Context.CurrentModule.DefineConstant (new IodineInteger (key.Value))
                );

            }

            Context.CurrentMethod.EmitInstruction (
                Opcode.LoadConst,
                CreateName (enumDecl.Name)
            );

            Context.CurrentMethod.EmitInstruction (Opcode.BuildEnum, enumDecl.Items.Count);
        }

        private void CompileMethod (Function funcDecl)
        {
            Context.SymbolTable.AddSymbol (funcDecl.Name);

            Context.SymbolTable.EnterScope ();

            CodeBuilder bytecode = new CodeBuilder ();

            CreateContext (bytecode);

            for (int i = 0; i < funcDecl.Parameters.Count; i++) {
                Context.SymbolTable.AddSymbol (funcDecl.Parameters [i].Name);

                if (funcDecl.Parameters [i].HasType) {
                    funcDecl.Parameters [i].Type.Visit (this);
                    Context.CurrentMethod.EmitInstruction (Opcode.CastLocal, CreateName (funcDecl.Parameters [i].Name));
                }
            }

            funcDecl.VisitChildren (this);

            DestroyContext ();

            bytecode.FinalizeMethod ();

            OptimizeObject (bytecode);

            MethodFlags flags = new MethodFlags ();

            if (funcDecl.AcceptsKeywordArgs) {
                flags |= MethodFlags.AcceptsKwargs;
            }

            if (funcDecl.Variadic) {
                flags |= MethodFlags.AcceptsVarArgs;
            }

            if (funcDecl.HasDefaultValues) {
                flags |= MethodFlags.HasDefaultParameters;

                int startingIndex = funcDecl.Parameters.FindIndex (p => p.HasDefaultValue);
                int defaultParamCount = 0;

                Context.CurrentMethod.EmitInstruction (
                    Opcode.LoadConst,
                    Context.CurrentModule.DefineConstant (new IodineInteger (startingIndex))
                );

                for (int i = 0; i < funcDecl.Parameters.Count; i++) {
                    if (funcDecl.Parameters [i].HasDefaultValue) {
                        funcDecl.Parameters [i].DefaultValue.Visit (this);
                        defaultParamCount++;
                    }
                }

                Context.CurrentMethod.EmitInstruction (Opcode.BuildTuple, defaultParamCount);
            }

            for (int i = 0; i < funcDecl.Parameters.Count; i++) {
                Context.CurrentMethod.EmitInstruction (
                    Opcode.LoadConst,
                    Context.CurrentModule.DefineConstant (new IodineString (funcDecl.Parameters [i].Name))
                );
            }

            Context.CurrentMethod.EmitInstruction (Opcode.BuildTuple, funcDecl.Parameters.Count);

            Context.CurrentMethod.EmitInstruction (
                Opcode.LoadConst,
                Context.CurrentModule.DefineConstant (bytecode)
            );

            Context.CurrentMethod.EmitInstruction (
                Opcode.LoadConst,
                Context.CurrentModule.DefineConstant (new IodineString (funcDecl.Documentation))
            );

            Context.CurrentMethod.EmitInstruction (
                Opcode.LoadConst,
                Context.CurrentModule.DefineConstant (new IodineString (funcDecl.Name))
            );

            Context.CurrentMethod.EmitInstruction (Opcode.BuildFunction, (int)flags);

            symbolTable.ExitScope ();
        }

        public override void Accept (ClassDeclaration classDecl)
        {
            CompileClass (classDecl);

            if (Context.IsInClassBody) {
                Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName (classDecl.Name));
            } else if (symbolTable.IsInGlobalScope) {
                Context.CurrentMethod.EmitInstruction (Opcode.StoreGlobal, CreateName (classDecl.Name));
            } else {
                Context.CurrentMethod.EmitInstruction (Opcode.StoreLocal, CreateName (classDecl.Name));
            }
        }

        public override void Accept (ContractDeclaration contractDecl)
        {
            CompileContract (contractDecl);

            if (Context.IsInClassBody) {
                Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName (contractDecl.Name));
            } else if (symbolTable.IsInGlobalScope) {
                Context.CurrentMethod.EmitInstruction (Opcode.StoreGlobal, CreateName (contractDecl.Name));
            } else {
                Context.CurrentMethod.EmitInstruction (Opcode.StoreLocal, CreateName (contractDecl.Name));
            }
        }

        public override void Accept (TraitDeclaration traitDecl)
        {
            CompileTrait (traitDecl);

            if (Context.IsInClassBody) {
                Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName (traitDecl.Name));
            } else if (symbolTable.IsInGlobalScope) {
                Context.CurrentMethod.EmitInstruction (Opcode.StoreGlobal, CreateName (traitDecl.Name));
            } else {
                Context.CurrentMethod.EmitInstruction (Opcode.StoreLocal, CreateName (traitDecl.Name));
            }
        }

        public override void Accept (MixinDeclaration mixinDecl)
        {
            CompileMixin (mixinDecl);

            if (Context.IsInClassBody) {
                Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName (mixinDecl.Name));
            } else if (symbolTable.IsInGlobalScope) {
                Context.CurrentMethod.EmitInstruction (Opcode.StoreGlobal, CreateName (mixinDecl.Name));
            } else {
                Context.CurrentMethod.EmitInstruction (Opcode.StoreLocal, CreateName (mixinDecl.Name));
            }
        }

        public override void Accept (EnumDeclaration enumDecl)
        {
            CompileEnum (enumDecl);

            if (Context.IsInClassBody) {
                Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName (enumDecl.Name));
            } else if (symbolTable.IsInGlobalScope) {
                Context.CurrentMethod.EmitInstruction (Opcode.StoreGlobal, CreateName (enumDecl.Name));
            } else {
                Context.CurrentMethod.EmitInstruction (Opcode.StoreLocal, CreateName (enumDecl.Name));
            }
        }

        public override void Accept (FunctionDeclaration funcDecl)
        {
            CompileMethod (funcDecl);

            if (Context.IsInClassBody) {
                Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName (funcDecl.Name));
            } else if (symbolTable.IsInGlobalScope) {
                Context.CurrentMethod.EmitInstruction (Opcode.StoreGlobal, CreateName (funcDecl.Name));
            } else {
                Context.CurrentMethod.EmitInstruction (funcDecl.Location, Opcode.BuildClosure);
                Context.CurrentMethod.EmitInstruction (Opcode.StoreLocal, CreateName (funcDecl.Name));
            }
        }

        public override void Accept (DecoratedFunction funcDecl)
        {
            CompileMethod (funcDecl.Function);

            if (!(Context.IsInClassBody || symbolTable.IsInGlobalScope)) {
                Context.CurrentMethod.EmitInstruction (Opcode.BuildClosure);
            }
            funcDecl.Decorator.Visit (this);
            Context.CurrentMethod.EmitInstruction (Opcode.Invoke, 1);

            if (Context.IsInClassBody) {
                Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName (funcDecl.Function.Name));
            } else if (symbolTable.IsInGlobalScope) {
                Context.CurrentMethod.EmitInstruction (Opcode.StoreGlobal, CreateName (funcDecl.Function.Name));
            } else {
                Context.CurrentMethod.EmitInstruction (Opcode.StoreLocal, CreateName (funcDecl.Function.Name));
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

                Context.CurrentModule.Initializer.EmitInstruction (
                    useStmt.Location,
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
                Context.CurrentMethod.EmitInstruction (
                    useStmt.Location,
                    Opcode.LoadGlobal,
                    CreateName ("require")
                );

                Context.CurrentMethod.EmitInstruction (useStmt.Location, Opcode.Invoke,
                    items.Length == 0 ? 1 : 2
                );

                Context.CurrentMethod.EmitInstruction (useStmt.Location, Opcode.Pop);
            }

        }

        public override void Accept (ExtendStatement extendStmt)
        {
            extendStmt.Class.Visit (this);

            foreach (AstNode member in extendStmt.Members) {
                if (member is FunctionDeclaration) {
                    FunctionDeclaration funcDecl = member as FunctionDeclaration;
                    CompileMethod (funcDecl);
                    Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName (funcDecl.Name));
                }
            }

            Context.CurrentMethod.EmitInstruction (Opcode.LoadConst, CreateName ("__anonymous__"));

            Context.CurrentMethod.EmitInstruction (Opcode.BuildMixin, extendStmt.Members.Count);

            Context.CurrentMethod.EmitInstruction (extendStmt.Location, Opcode.IncludeMixin);

            foreach (AstNode node in extendStmt.Mixins) {
                node.Visit (this);
                Context.CurrentMethod.EmitInstruction (
                    extendStmt.Location,
                    Opcode.IncludeMixin
                );
            }
        }

        public override void Accept (Statement stmt)
        {
            stmt.VisitChildren (this);
        }

        public override void Accept (GivenStatement switchStmt)
        {
            switchStmt.GivenValue.Visit (this);

            int temporary = CreateTemporary ();

            Context.CurrentMethod.EmitInstruction (
                switchStmt.GivenValue.Location,
                Opcode.StoreLocal,
                temporary
            );

            Label endSwitch = Context.CurrentMethod.CreateLabel ();

            foreach (WhenStatement caseStmt in switchStmt.WhenStatements) {
                Label nextLabel = Context.CurrentMethod.CreateLabel ();

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
            Label exceptLabel = Context.CurrentMethod.CreateLabel ();
            Label endLabel = Context.CurrentMethod.CreateLabel ();

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
                Context.SymbolTable.AddSymbol (tryExcept.ExceptionIdentifier);
                Context.CurrentMethod.EmitInstruction (tryExcept.ExceptBody.Location, Opcode.LoadException);
                Context.CurrentMethod.EmitInstruction (tryExcept.ExceptBody.Location,
                    Opcode.StoreLocal,
                    CreateName (tryExcept.ExceptionIdentifier)
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
            Label elseLabel = Context.CurrentMethod.CreateLabel ();
            Label endLabel = Context.CurrentMethod.CreateLabel ();
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
            Label whileLabel = Context.CurrentMethod.CreateLabel ();
            Label breakLabel = Context.CurrentMethod.CreateLabel ();

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
            Label doLabel = Context.CurrentMethod.CreateLabel ();
            Label breakLabel = Context.CurrentMethod.CreateLabel ();

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
            Label forLabel = Context.CurrentMethod.CreateLabel ();
            Label breakLabel = Context.CurrentMethod.CreateLabel ();
            Label skipAfterThought = Context.CurrentMethod.CreateLabel ();

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
            Label foreachLabel = Context.CurrentMethod.CreateLabel ();
            Label breakLabel = Context.CurrentMethod.CreateLabel ();
            int tmp = CreateTemporary (); 

            Context.BreakLabels.Push (breakLabel);
            Context.ContinueLabels.Push (foreachLabel);

            foreachStmt.Iterator.Visit (this);

            Context.SymbolTable.EnterScope ();

            Context.CurrentMethod.EmitInstruction (foreachStmt.Iterator.Location, Opcode.GetIter);
            Context.CurrentMethod.EmitInstruction (foreachStmt.Iterator.Location, Opcode.Dup);
            Context.CurrentMethod.EmitInstruction (
                foreachStmt.Iterator.Location,
                Opcode.StoreLocal,
                tmp
            );
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

            if (foreachStmt.Items.Count == 1) {
                Context.SymbolTable.AddSymbol (foreachStmt.Items [0]);

                Context.CurrentMethod.EmitInstruction (foreachStmt.Iterator.Location,
                    Opcode.StoreLocal,
                    CreateName (foreachStmt.Items [0])
                );
            } else {

                CompileForeachWithAutounpack (foreachStmt.Items);
            }

            foreachStmt.Body.Visit (this);

            Context.CurrentMethod.EmitInstruction (foreachStmt.Body.Location, Opcode.Jump, foreachLabel);
            Context.CurrentMethod.MarkLabelPosition (breakLabel);

            Context.SymbolTable.ExitScope ();

            Context.BreakLabels.Pop ();
            Context.ContinueLabels.Pop ();
        }

        private void CompileForeachWithAutounpack (List<string> identifiers)
        {
            int local = CreateTemporary ();

            Context.CurrentMethod.EmitInstruction (Opcode.StoreLocal, local);

            for (int i = 0; i < identifiers.Count; i++) {

                string ident = identifiers [i];

                Context.CurrentMethod.EmitInstruction (Opcode.LoadLocal, local);

                Context.CurrentMethod.EmitInstruction (
                    Opcode.LoadConst,
                    Context.CurrentModule.DefineConstant (new IodineInteger (i))
                );

                Context.CurrentMethod.EmitInstruction (Opcode.LoadIndex);

                if (!symbolTable.IsSymbolDefined (ident)) {
                    symbolTable.AddSymbol (ident);
                }

                Context.CurrentMethod.EmitInstruction (
                    Opcode.StoreLocal,
                    CreateName (ident)
                );
            }
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
            //Context.CurrentMethod.Generator = true;
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
            if (super.Parent.BaseClass != null) {
                super.VisitChildren (this);
                super.Parent.BaseClass.Visit (this);
                Context.CurrentMethod.EmitInstruction (Opcode.InvokeSuper, super.Arguments.Arguments.Count);
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
                    if (symbolTable.IsGlobal (ident) || assignStmt.Global) {
                        Context.CurrentMethod.EmitInstruction (
                            Opcode.StoreGlobal,
                            CreateName (ident)
                        );
                    } else {
                        if (!symbolTable.IsSymbolDefined (ident)) {
                            symbolTable.AddSymbol (ident);
                        }

                        Context.CurrentMethod.EmitInstruction (
                            Opcode.StoreLocal,
                            CreateName (ident)
                        );
                    }
                }
            }
        }

        private void CompileAssignWithAutoUnpack (AssignStatement assignStmt)
        {
            int tmp = CreateTemporary ();

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

                if (symbolTable.IsGlobal (ident) || assignStmt.Global) {
                    Context.CurrentMethod.EmitInstruction (
                        Opcode.StoreGlobal,
                        CreateName (ident)
                    );
                } else {
                    if (!symbolTable.IsSymbolDefined (ident)) {
                        symbolTable.AddSymbol (ident);
                    }
                    Context.CurrentMethod.EmitInstruction (
                        Opcode.StoreLocal,
                        CreateName (ident)
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

        public override void Accept (LambdaExpression lambda)
        {
            CompileMethod (lambda);

            if (!symbolTable.IsInGlobalScope) {
                Context.CurrentMethod.EmitInstruction (Opcode.BuildClosure);
            }
        }

        public override void Accept (BinaryExpression binop)
        {
            if (binop.Operation == BinaryOperation.Assign) {
                binop.Right.Visit (this);
                if (binop.Left is NameExpression) {
                    NameExpression ident = (NameExpression)binop.Left;
                    bool isGlobal = Context.SymbolTable.IsInGlobalScope || Context.SymbolTable.IsGlobal (ident.Value);
                    if (!isGlobal) {

                        if (!Context.SymbolTable.IsSymbolDefined (ident.Value)) {
                            Context.SymbolTable.AddSymbol (ident.Value);
                        }
                        int sym = CreateName (ident.Value);
                        Context.CurrentMethod.EmitInstruction (ident.Location, Opcode.StoreLocal, sym);
                        Context.CurrentMethod.EmitInstruction (ident.Location, Opcode.LoadLocal, sym);
                    } else {
                        int globalIndex = CreateName (ident.Value);
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

            Label shortCircuitTrueLabel = Context.CurrentMethod.CreateLabel ();
            Label shortCircuitFalseLabel = Context.CurrentMethod.CreateLabel ();
            Label endLabel = Context.CurrentMethod.CreateLabel ();
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
                Context.CurrentMethod.EmitInstruction (
                    call.Target.Location, 
                    Opcode.InvokeVar, 
                    call.Arguments.Arguments.Count - 1
                );
            } else {
                Context.CurrentMethod.EmitInstruction (
                    call.Target.Location,
                    Opcode.Invoke, 
                    call.Arguments.Arguments.Count
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
                Context.CurrentMethod.EmitInstruction (
                    kwargs.Location,
                    Opcode.LoadConst,
                    Context.CurrentModule.DefineConstant (new IodineString (kw))
                );
                val.Visit (this);
                Context.CurrentMethod.EmitInstruction (kwargs.Location, Opcode.BuildTuple, 2);
            }
            Context.CurrentMethod.EmitInstruction (kwargs.Location, Opcode.BuildList, kwargs.Keywords.Count);
            Context.CurrentMethod.EmitInstruction (kwargs.Location,
                Opcode.LoadGlobal,
                CreateName ("Dict")
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
                CreateName (getAttr.Field)
            );

            if (Context.IsPatternExpression) {
                Context.CurrentMethod.EmitInstruction (
                    getAttr.Location,
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

        public override void Accept (TernaryExpression ternaryExpr)
        {
            Label elseLabel = Context.CurrentMethod.CreateLabel ();
            Label endLabel = Context.CurrentMethod.CreateLabel ();
            ternaryExpr.Condition.Visit (this);
            Context.CurrentMethod.EmitInstruction (ternaryExpr.Expression.Location, Opcode.JumpIfFalse, elseLabel);
            ternaryExpr.Expression.Visit (this);
            Context.CurrentMethod.EmitInstruction (ternaryExpr.ElseExpression != null
                ? ternaryExpr.ElseExpression.Location
                : ternaryExpr.Location,
                Opcode.Jump,
                endLabel
            );
            Context.CurrentMethod.MarkLabelPosition (elseLabel);
            if (ternaryExpr.ElseExpression != null) {
                ternaryExpr.ElseExpression.Visit (this);
            }
            Context.CurrentMethod.MarkLabelPosition (endLabel);
        }

        public override void Accept (GeneratorExpression genExpr)
        {
            CodeBuilder anonMethod = new CodeBuilder ();

            CreateContext (anonMethod);

            Context.SymbolTable.EnterScope ();

            Label foreachLabel = Context.CurrentMethod.CreateLabel ();
            Label breakLabel = Context.CurrentMethod.CreateLabel ();
            Label predicateSkip = Context.CurrentMethod.CreateLabel ();

            int tmp = CreateTemporary (); 

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
            Context.SymbolTable.AddSymbol (genExpr.Identifier);
            Context.CurrentMethod.EmitInstruction (genExpr.Iterator.Location,
                Opcode.StoreLocal,
                CreateName (genExpr.Identifier)
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

            anonMethod.FinalizeMethod ();

            DestroyContext ();

            Context.CurrentMethod.EmitInstruction (
                genExpr.Location,
                Opcode.LoadConst,
                Context.CurrentModule.DefineConstant (anonMethod)
            );

            Context.CurrentMethod.EmitInstruction (genExpr.Location, Opcode.BuildGenExpr);
        }

        public override void Accept (ListCompExpression list)
        {
            Label foreachLabel = Context.CurrentMethod.CreateLabel ();
            Label breakLabel = Context.CurrentMethod.CreateLabel ();
            Label predicateSkip = Context.CurrentMethod.CreateLabel ();

            int tmp = CreateTemporary (); 
            int set = CreateTemporary ();

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
            Context.CurrentMethod.EmitInstruction (
                list.Iterator.Location,
                Opcode.JumpIfFalse,
                breakLabel
            );

            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.LoadLocal, tmp);
            Context.CurrentMethod.EmitInstruction (list.Iterator.Location, Opcode.IterGetNext);

            Context.SymbolTable.AddSymbol (list.Identifier);

            Context.CurrentMethod.EmitInstruction (
                list.Iterator.Location,
                Opcode.StoreLocal,
                CreateName (list.Identifier)
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
            CreateContext (Context.IsInClassBody);

            tuple.VisitChildren (this);

            DestroyContext ();


            if (Context.IsPatternExpression) {
                Context.CurrentMethod.EmitInstruction (Opcode.LoadLocal, Context.PatternTemporary);
                Context.CurrentMethod.EmitInstruction (tuple.Location, Opcode.MatchPattern, tuple.Items.Count);

            } else {
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
            int temporary = CreateTemporary ();
            Context.CurrentMethod.EmitInstruction (match.Location, Opcode.StoreLocal, temporary);

            Label nextLabel = Context.CurrentMethod.CreateLabel ();
            Label endLabel = Context.CurrentMethod.CreateLabel ();

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
            Label shortCircuitTrueLabel = Context.CurrentMethod.CreateLabel ();
            Label shortCircuitFalseLabel = Context.CurrentMethod.CreateLabel ();
            Label endLabel = Context.CurrentMethod.CreateLabel ();
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
 

        public override void Accept (PatternExtractExpression extractExpression)
        {
            Label notInstance = Context.CurrentMethod.CreateLabel ();
            Label isInstance = Context.CurrentMethod.CreateLabel ();

            CreateContext (Context.IsInClassBody);

            extractExpression.Target.Visit (this);

            DestroyContext ();

            Context.CurrentMethod.EmitInstruction (extractExpression.Target.Location,
                Opcode.LoadLocal,
                Context.PatternTemporary
            );

            Context.CurrentMethod.EmitInstruction (
                Opcode.InstanceOf
            );
                
            Context.CurrentMethod.EmitInstruction (Opcode.JumpIfFalse, notInstance);

            Context.CurrentMethod.EmitInstruction (extractExpression.Target.Location,
                Opcode.LoadLocal,
                Context.PatternTemporary
            );

            Context.CurrentMethod.EmitInstruction (
                Opcode.Unwrap,
                extractExpression.Captures.Count
            );

            Context.CurrentMethod.EmitInstruction (Opcode.JumpIfFalse, notInstance);

            if (extractExpression.Captures.Count > 1) {
                Context.CurrentMethod.EmitInstruction (
                    Opcode.Unpack,
                    extractExpression.Captures.Count
                );
            }

            foreach (string capture in extractExpression.Captures) {
                Context.CurrentMethod.EmitInstruction (
                    Opcode.StoreLocal,
                    Context.CurrentModule.DefineConstant (new IodineName (capture))
                );

                symbolTable.AddSymbol (capture);
            }

            Context.CurrentMethod.EmitInstruction (Opcode.LoadTrue);
            Context.CurrentMethod.EmitInstruction (Opcode.Jump, isInstance);
            Context.CurrentMethod.MarkLabelPosition (notInstance);
            Context.CurrentMethod.EmitInstruction (Opcode.LoadFalse);
            Context.CurrentMethod.MarkLabelPosition (isInstance);
        }
        #endregion

        #region Terminals

        public override void Accept (NameExpression ident)
        {
            if (Context.IsPatternExpression) {
                if (ident.Value == "_") {
                    Context.CurrentMethod.EmitInstruction (ident.Location, Opcode.LoadTrue);
                } else {

                    Context.CurrentMethod.EmitInstruction (
                        ident.Location,
                        Opcode.LoadGlobal,
                        CreateName (ident.Value)
                    );
                    Context.CurrentMethod.EmitInstruction (ident.Location,
                        Opcode.LoadLocal,
                        Context.PatternTemporary
                    );

                    Context.CurrentMethod.EmitInstruction (
                        ident.Location,
                        Opcode.InstanceOf
                    );
                }
                return;
            
            }
        
            if (Context.SymbolTable.IsSymbolDefined (ident.Value)) {
                if (!Context.SymbolTable.IsGlobal (ident.Value)) {
                    Context.CurrentMethod.EmitInstruction (
                        ident.Location,
                        Opcode.LoadLocal, 
                        CreateName (ident.Value)
                    );
                } else {
                    Context.CurrentMethod.EmitInstruction (
                        ident.Location,
                        Opcode.LoadGlobal,
                        CreateName (ident.Value)
                    );
                }
            } else if (Context.IsInClass && ExistsInOuterClass (ident.Value)) {
                LoadAssociatedClass (ident.Value);
                Context.CurrentMethod.EmitInstruction (
                    ident.Location,
                    Opcode.LoadAttribute,
                    CreateName (ident.Value)
                );
            } else {
                Context.CurrentMethod.EmitInstruction (
                    ident.Location,
                    Opcode.LoadGlobal,
                    CreateName (ident.Value)
                );
            }

        }

        private bool ExistsInOuterClass (string name)
        {

            return false;
        }

        /*
         * Emits the instructions required for loading the class that contains
         * the attribute 'item'
         */
        private void LoadAssociatedClass (string item = null)
        {
        }

        public override void Accept (IntegerExpression integer)
        {
            Context.CurrentMethod.EmitInstruction (integer.Location,
                Opcode.LoadConst, 
                Context.CurrentModule.DefineConstant (new IodineInteger (integer.Value))
            );

            if (Context.IsPatternExpression) {
                Context.CurrentMethod.EmitInstruction (
                    integer.Location,
                    Opcode.LoadLocal,
                    Context.PatternTemporary
                );
                Context.CurrentMethod.EmitInstruction (
                    integer.Location,
                    Opcode.BinOp,
                    (int)BinaryOperation.Equals
                );
            }
        }

        public override void Accept (BigIntegerExpression integer)
        {
            Context.CurrentMethod.EmitInstruction (integer.Location,
                Opcode.LoadConst,
                Context.CurrentModule.DefineConstant (new IodineBigInt (integer.Value))
            );

            if (Context.IsPatternExpression) {
                Context.CurrentMethod.EmitInstruction (
                    integer.Location,
                    Opcode.LoadLocal,
                    Context.PatternTemporary
                );
                Context.CurrentMethod.EmitInstruction (
                    integer.Location,
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

