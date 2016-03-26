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
using Iodine.Compiler.Ast;
using Iodine.Runtime;

namespace Iodine.Compiler
{
	/// <summary>
	/// Responsible for compiling all code within a compilation unit
	/// </summary>
	internal class ModuleCompiler : IodineAstVisitor
	{
		private FunctionCompiler functionCompiler;

		private EmitContext context;

		public ModuleCompiler (EmitContext context)
		{
			this.context = context;
			functionCompiler = new FunctionCompiler (context.CurrentModule.Initializer, context);
		}

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
				context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.LoadConst,
					context.CurrentModule.DefineConstant (new IodineString (import))
				);
				context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.BuildTuple, 0);
				context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.LoadGlobal,
					context.CurrentModule.DefineConstant (new IodineName ("require"))
				);
				context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.Invoke, 2);
				context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.Pop);
			} else {
				IodineObject[] items = new IodineObject [useStmt.Imports.Count];

				context.CurrentModule.Initializer.EmitInstruction (useStmt.Location,
					Opcode.LoadConst,
					context.CurrentModule.DefineConstant (new IodineString (import))
				);

				if (items.Length > 0) {
					for (int i = 0; i < items.Length; i++) {
						items [i] = new IodineString (useStmt.Imports [i]);
						context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.LoadConst,
							context.CurrentModule.DefineConstant (new IodineString (useStmt.Imports [i])));
					}
					context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.BuildTuple, items.Length);
				}
				context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.LoadGlobal,
					context.CurrentModule.DefineConstant (new IodineName ("require"))
				);

				context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.Invoke,
					items.Length == 0 ? 1 : 2
				);

				context.CurrentModule.Initializer.EmitInstruction (useStmt.Location, Opcode.Pop);
			}
			
		}

		public override void Accept (ClassDeclaration classDecl)
		{
			context.CurrentModule.SetAttribute (classDecl.Name, CompileClass (classDecl));
		}

		public override void Accept (EnumDeclaration enumDecl)
		{
			context.CurrentModule.SetAttribute (enumDecl.Name, CompileEnum (enumDecl));
		}

		public override void Accept (InterfaceDeclaration contractDecl)
		{
			IodineInterface contract = new IodineInterface (contractDecl.Name);
			foreach (AstNode node in contractDecl.Members) {
				FunctionDeclaration decl = node as FunctionDeclaration;
				contract.AddMethod (new MethodBuilder (context.CurrentModule,
					decl.Name,
					decl.InstanceMethod,
					decl.Parameters.Count,
					0,
					decl.Variadic,
					decl.AcceptsKeywordArgs
				));
			}
			context.CurrentModule.SetAttribute (contractDecl.Name, contract);
		}
		
		public override void Accept (FunctionDeclaration funcDecl)
		{
			context.CurrentModule.AddMethod (CompileMethod (funcDecl));
		}

		public IodineClass CompileClass (ClassDeclaration classDecl)
		{
			MethodBuilder constructor = CompileMethod (classDecl.Constructor);
			if (classDecl.Constructor.Body.Statements.Count == 0) {
				if (classDecl.Base.Count > 0) {
					foreach (string subclass in classDecl.Base) {
						string[] contract = subclass.Split ('.');
						constructor.EmitInstruction (classDecl.Location,
							Opcode.LoadGlobal,
							context.CurrentModule.DefineConstant (new IodineName (contract [0]))
						);
						for (int j = 1; j < contract.Length; j++) {
							constructor.EmitInstruction (classDecl.Location,
								Opcode.LoadAttribute,
								context.CurrentModule.DefineConstant (new IodineName (contract [0]))
							);
						}
						constructor.EmitInstruction (classDecl.Location, Opcode.InvokeSuper, 0);
					}
				}
			}

			MethodBuilder initializer = new MethodBuilder (context.CurrentModule,
				"__init__",
				false,
				0,
				0,
				false,
				false
			);
			
			IodineClass clazz = new IodineClass (classDecl.Name, initializer, constructor);

			FunctionCompiler compiler = new FunctionCompiler (initializer, context);

			foreach (AstNode member in classDecl.Members) {
				if (member is FunctionDeclaration) {
					FunctionDeclaration func = member as FunctionDeclaration;
					if (func.InstanceMethod) {
						clazz.AddInstanceMethod (CompileMethod (func));
					} else {
						clazz.SetAttribute (func.Name, CompileMethod (func));
					}
				} else if (member is ClassDeclaration) {
					ClassDeclaration subclass = member as ClassDeclaration;
					clazz.SetAttribute (subclass.Name, CompileClass (subclass));
				} else if (member is EnumDeclaration) {
					EnumDeclaration enumeration = member as EnumDeclaration;
					clazz.SetAttribute (enumeration.Name, CompileEnum (enumeration));
				} else if (member is BinaryExpression) {
					BinaryExpression expr = member as BinaryExpression;
					NameExpression name = expr.Left as NameExpression;
					expr.Right.Visit (compiler);
					initializer.EmitInstruction (classDecl.Location,
						Opcode.LoadGlobal,
						context.CurrentModule.DefineConstant (new IodineName (classDecl.Name))
					);

					initializer.EmitInstruction (classDecl.Location,
						Opcode.StoreAttribute,
						context.CurrentModule.DefineConstant (new IodineName (name.Value))
					);
				} else {
					member.Visit (compiler);
				}
			}

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

		public override void Accept (VariableDeclaration varDecl)
		{
			varDecl.VisitChildren (this);
		}


		private MethodBuilder CompileMethod (FunctionDeclaration funcDecl)
		{
			context.SymbolTable.NextScope ();

			MethodBuilder methodBuilder = new MethodBuilder (context.CurrentModule,
				funcDecl.Name,
				funcDecl.InstanceMethod,
				funcDecl.Parameters.Count,
				context.SymbolTable.CurrentScope.SymbolCount,
				funcDecl.Variadic,
				funcDecl.AcceptsKeywordArgs
			);

			FunctionCompiler compiler = new FunctionCompiler (methodBuilder, context);
			
			for (int i = 0; i < funcDecl.Parameters.Count; i++) {
				methodBuilder.Parameters [funcDecl.Parameters [i]] = context.SymbolTable.GetSymbol
					(funcDecl.Parameters [i]).Index;
			}

			funcDecl.VisitChildren (compiler);

			methodBuilder.EmitInstruction (funcDecl.Location, Opcode.LoadNull);


			methodBuilder.FinalizeLabels ();

			context.SymbolTable.LeaveScope ();

			return methodBuilder;
		}

		public void Accept (AstNode ast)
		{
			ast.VisitChildren (this);
		}

		public override void Accept (CompilationUnit ast)
		{
			ast.VisitChildren (this);
		}

		public override void Accept (Expression expr)
		{
			expr.VisitChildren (this);
		}

		public override void Accept (Statement stmt)
		{
			stmt.Visit (functionCompiler);
		}

		public override void Accept (StatementList stmtList)
		{
			stmtList.VisitChildren (this);
		}

		#region Expressions

		public override void Accept (BinaryExpression binop)
		{
			binop.Visit (functionCompiler);
		}

		public override void Accept (UnaryExpression unaryop)
		{
			unaryop.Visit (functionCompiler);
		}

		public override void Accept (NameExpression ident)
		{
			ident.Visit (functionCompiler);
		}

		public override void Accept (CallExpression call)
		{
			call.Visit (functionCompiler);
		}

		public override void Accept (ArgumentList arglist)
		{
			arglist.Visit (functionCompiler);
		}

		public override void Accept (KeywordArgumentList kwargs)
		{
			kwargs.Visit (functionCompiler);
		}

		public override void Accept (GetExpression getAttr)
		{
			getAttr.Visit (functionCompiler);
		}

		public override void Accept (IntegerExpression integer)
		{
			integer.Visit (functionCompiler);
		}

		public override void Accept (FloatExpression num)
		{
			num.Visit (functionCompiler);
		}

		public override void Accept (StringExpression str)
		{
			str.Visit (functionCompiler);
		}

		public override void Accept (TupleExpression tuple)
		{
			tuple.Visit (functionCompiler);
		}

		public override void Accept (ContinueStatement cont)
		{
			cont.Visit (functionCompiler);
		}

		public override void Accept (MatchExpression match)
		{
			match.VisitChildren (functionCompiler);
		}

		public override void Accept (CaseExpression caseExpr)
		{
			caseExpr.VisitChildren (functionCompiler);
		}

		public override void Accept (TernaryExpression ifExpr)
		{
			ifExpr.Visit (functionCompiler);
		}

		public override void Accept (IndexerExpression indexer)
		{
			indexer.Visit (functionCompiler);
		}

		public override void Accept (ListExpression list)
		{
			list.Visit (functionCompiler);
		}

		public override void Accept (HashExpression hash)
		{
			hash.Visit (functionCompiler);
		}

		public override void Accept (SelfStatement self)
		{
			self.Visit (functionCompiler);
		}

		public override void Accept (TrueExpression ntrue)
		{
			ntrue.Visit (functionCompiler);
		}

		public override void Accept (FalseExpression nfalse)
		{
			nfalse.Visit (functionCompiler);
		}

		public override void Accept (NullExpression nil)
		{
			nil.Visit (functionCompiler);
		}

		public override void Accept (LambdaExpression lambda)
		{
			lambda.Visit (functionCompiler);
		}
		public override void Accept (ListCompExpression list)
		{
			list.Visit (functionCompiler);
		}

		#endregion
	}
}

