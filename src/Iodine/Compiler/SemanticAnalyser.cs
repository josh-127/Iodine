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
using Iodine.Compiler.Ast;

namespace Iodine.Compiler
{
    internal class SemanticAnalyser : AstVisitor
    {
        private ErrorSink errorLog;
        private SymbolTable symbolTable = new SymbolTable ();

        public SemanticAnalyser (ErrorSink errorLog)
        {
            this.errorLog = errorLog;
        }

        public SymbolTable Analyse (CompilationUnit ast)
        {
            ast.VisitChildren (this);
            return symbolTable;
        }

        public override void Accept (ClassDeclaration declaration)
        {
            symbolTable.AddSymbol (declaration.Name);
        }

        public override void Accept (EnumDeclaration enumDecl)
        {
            symbolTable.AddSymbol (enumDecl.Name);
        }

        public override void Accept (ContractDeclaration idecl)
        {
            symbolTable.AddSymbol (idecl.Name);
        }

        public override void Accept (FunctionDeclaration decl)
        {
            symbolTable.AddSymbol (decl.Name);

            symbolTable.EnterScope ();

            foreach (NamedParameter param in decl.Parameters) {
                symbolTable.AddSymbol (param.Name);
            }

            decl.VisitChildren (this);

            symbolTable.ExitScope ();

        }

        public override void Accept (CodeBlock scope)
        {
            symbolTable.EnterScope ();
            scope.VisitChildren (this);
            symbolTable.ExitScope ();
        }

        public override void Accept (StatementList stmtList)
        {
            stmtList.VisitChildren (this);
        }

        public override void Accept (IfStatement ifstmt)
        {
            ifstmt.VisitChildren (this);
        }

        public override void Accept (ForStatement forStmt)
        {
            forStmt.VisitChildren (this);
        }

        public override void Accept (ForeachStatement stmt)
        {
            stmt.VisitChildren (this);
        }

        public override void Accept (WhileStatement stmt)
        {
            stmt.VisitChildren (this);
        }

        public override void Accept (DoStatement doStmt)
        {
            doStmt.VisitChildren (this);
        }

        public override void Accept (GivenStatement givenStmt)
        {
            givenStmt.VisitChildren (this);
        }

        public override void Accept (SuperCallStatement super)
        {
            super.VisitChildren (this);
        }

        public override void Accept (ReturnStatement returnStmt)
        {
            returnStmt.VisitChildren (this);
        }

        public override void Accept (Expression expr)
        {
            expr.VisitChildren (this);
        }

        public override void Accept (CallExpression callExpr)
        {
            callExpr.VisitChildren (this);
        }

        public override void Accept (ArgumentList arglist)
        {
            arglist.VisitChildren (this);
        }

        public override void Accept (IndexerExpression indexer)
        {
            indexer.VisitChildren (this);
        }

        public override void Accept (MemberExpression getAttr)
        {
            getAttr.VisitChildren (this);
        }

        public override void Accept (MemberDefaultExpression getAttr)
        {
            getAttr.VisitChildren (this);
        }

        public override void Accept (TernaryExpression ifExpr)
        {
            ifExpr.VisitChildren (this);
        }

        public override void Accept (BinaryExpression expression)
        {
            if (expression.Operation == BinaryOperation.Assign &&
                expression.Left is NameExpression) {
                NameExpression name = expression.Left as NameExpression;
                if (!symbolTable.IsSymbolDefined (name.Value)) {
                    symbolTable.AddSymbol (name.Value);
                }
            }
            expression.VisitChildren (this);
        }

        public override void Accept (ListCompExpression list)
        {
            list.VisitChildren (this);
        }

        public override void Accept (TupleExpression tuple)
        {
            tuple.VisitChildren (this);
        }

        public override void Accept (HashExpression hash)
        {
            hash.VisitChildren (this);
        }

        public override void Accept (ListExpression list)
        {
            list.VisitChildren (this);
        }

        public override void Accept (PatternExpression expression)
        {
            expression.VisitChildren (this);
        }

        public override void Accept (TryExceptStatement tryCatch)
        {
            tryCatch.VisitChildren (this);
        }

        public override void Accept (CaseExpression caseExpr)
        {
            caseExpr.VisitChildren (this);
        }

        public override void Accept (WithStatement with)
        {
            with.VisitChildren (this);
        }

        public override void Accept (PatternExtractExpression patternExtract)
        {
            patternExtract.Target.Visit (this);

            foreach (string capture in patternExtract.Captures) {
                symbolTable.AddSymbol (capture);
            }
        }

        public override void Accept (CompilationUnit ast)
        {
            ast.VisitChildren (this);
        }

        public override void Accept (DecoratedFunction funcDecl)
        {
            funcDecl.VisitChildren (this);
        }

        public override void Accept (ExtendStatement exten)
        {
            exten.VisitChildren (this);
        }

        public override void Accept (WhenStatement caseStmt)
        {
            caseStmt.VisitChildren (this);
        }

        public override void Accept (LambdaExpression lambda)
        {
            lambda.VisitChildren (this);
        }
    }
}

