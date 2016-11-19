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
using System.Text;
using System.Collections.Generic;
using Iodine.Compiler.Ast;
using System.Numerics;

namespace Iodine.Compiler
{
    public sealed class Parser
    {
        private ErrorSink errorLog;
        private IodineContext context;
        private List<Token> tokens = new List<Token> ();

        private int position = 0;

        private Token Current {
            get {
                return PeekToken ();
            }
        }

        private bool EndOfStream {
            get {
                return tokens.Count <= position;
            }
        }

        private SourceLocation Location {
            get {
                if (PeekToken () != null)
                    return PeekToken ().Location;
                else if (tokens.Count == 0) {
                    return new SourceLocation (0, 0, "");
                }
                return PeekToken (-1).Location;

            }
        }
            
        private Parser (IodineContext context, IEnumerable<Token> tokens)
        {
            errorLog = context.ErrorLog;

            this.context = context;
            this.tokens.AddRange (tokens);
        }

        public static Parser CreateParser (IodineContext context, SourceUnit source)
        {
            Tokenizer tokenizer = new Tokenizer (
                context.ErrorLog,
                source.GetReader ()
            );
            return new Parser (context, tokenizer.Scan ());
        }

        public CompilationUnit Parse ()
        {
            try {
                CompilationUnit root = new CompilationUnit (Location);
                while (!EndOfStream) {
                    root.Add (ParseStatement ());
                }

                return root;
            } catch (EndOfFileException) {
                throw new SyntaxException (errorLog);
            } finally {
                if (errorLog.ErrorCount > 0) {
                    throw new SyntaxException (errorLog);
                }
            }
        }

        #region Declarations

        /*
         * class <name> [extends <baseclass> [implements <interfaces>, ...]] {
         * 
         * }
         * 
         * OR
         * 
         * class <name> (parameters, ...) [extends <baseclass> [implements <interfaces>, ...]]
         */
        private AstNode ParseClass ()
        {
            string doc = Expect (TokenClass.Keyword, "class").Documentation;

            string name = Expect (TokenClass.Identifier).Value;

            ClassDeclaration clazz = new ClassDeclaration (Location, name, doc);

            if (Match (TokenClass.OpenParan)) {
                bool isInstanceMethod;
                bool isVariadic;
                bool hasKeywordArgs;
                bool hasDefaultVals;

                List<NamedParameter> parameters = ParseFuncParameters (
                    out isInstanceMethod,
                    out isVariadic,
                    out hasKeywordArgs,
                    out hasDefaultVals
                );

                if (isInstanceMethod) {
                    errorLog.Add (Errors.RecordCantHaveSelf, Location);
                }

                if (isVariadic) {
                    errorLog.Add (Errors.RecordCantHaveVargs, Location);
                }

                if (hasKeywordArgs) {
                    errorLog.Add (Errors.RecordCantHaveKwargs, Location);
                }

                clazz = new ClassDeclaration (Location, name, doc, parameters);
            }

            if (Accept (TokenClass.Keyword, "extends")) {
                clazz.BaseClass = ParseExpression ();
            }

            if (Accept (TokenClass.Keyword, "implements")) {
                do {
                    clazz.Interfaces.Add (ParseExpression ());
                } while (Accept (TokenClass.Comma));
            }

            if (Accept (TokenClass.Keyword, "use")) {
                do {
                    clazz.Mixins.Add (ParseExpression ());
                } while (Accept (TokenClass.Comma));
            }

            if (Accept (TokenClass.OpenBrace)) {
                while (!Match (TokenClass.CloseBrace)) {
                    if (Match (TokenClass.Keyword, "func") || Match (TokenClass.Operator,
                        "@")) {
                        AstNode node = ParseFunction (false, clazz);
                        if (node is FunctionDeclaration) {
                            FunctionDeclaration func = node as FunctionDeclaration;
                            if (func == null) {
                                clazz.Add (node);
                            } else if (func.Name == name) {
                                clazz.Constructor = func;
                            } else {
                                clazz.Add (func);
                            }
                        } else {
                            StatementList list = node as StatementList;
                            clazz.Add (list.Statements [0]);
                            clazz.Add (list.Statements [1]);
                        }
                    } else {
                        clazz.Add (ParseStatement ());
                    }
                }

                Expect (TokenClass.CloseBrace);

            }
            return clazz;
        }

        /*
         * enum <name> {
         *  <item> [= <constant>],
         *  ...
         * }
         * 
         */
        private AstNode ParseEnum ()
        {
            string doc = Expect (TokenClass.Keyword, "enum").Documentation;
            string name = Expect (TokenClass.Identifier).Value;
            EnumDeclaration decl = new EnumDeclaration (Location, name, doc);

            Expect (TokenClass.OpenBrace);

            int defaultVal = -1;

            while (!Match (TokenClass.CloseBrace)) {
                string ident = Expect (TokenClass.Identifier).Value;
                if (Accept (TokenClass.Operator, "=")) {
                    string val = Expect (TokenClass.IntLiteral).Value;
                    int numVal = 0;
                    if (val != "") {
                        numVal = Int32.Parse (val);
                    }
                    decl.Items [ident] = numVal;
                } else {
                    decl.Items [ident] = defaultVal--;
                }

                if (!Accept (TokenClass.Comma)) {
                    break;
                }
            }

            Expect (TokenClass.CloseBrace);

            return decl;
        }

        /*
         * interface <name> {
         *     ...
         * }
         */
        private AstNode ParseContract ()
        {
            string doc = Expect (TokenClass.Keyword, "contract").Value;
            string name = Expect (TokenClass.Identifier).Value;

            ContractDeclaration contract = new ContractDeclaration (Location, name, doc);

            Expect (TokenClass.OpenBrace);

            while (!Match (TokenClass.CloseBrace)) {
                if (Match (TokenClass.Keyword, "func")) {
                    FunctionDeclaration func = ParseFunction (true) as FunctionDeclaration;
                    contract.AddMember (func);
                } else {
                    errorLog.Add (Errors.IllegalInterfaceDeclaration, Location);
                }
                while (Accept (TokenClass.SemiColon))
                    ;
            }

            Expect (TokenClass.CloseBrace);

            return contract;
        }

        /*
         * trait <name> {
         *     ...
         * }
         */
        private AstNode ParseTrait ()
        {
            string doc = Expect (TokenClass.Keyword, "trait").Documentation;
            string name = Expect (TokenClass.Identifier).Value;

            TraitDeclaration trait = new TraitDeclaration (Location, name, doc);

            Expect (TokenClass.OpenBrace);

            while (!Match (TokenClass.CloseBrace)) {
                if (Match (TokenClass.Keyword, "func")) {
                    FunctionDeclaration func = ParseFunction (true) as FunctionDeclaration;
                    trait.AddMember (func);
                } else {
                    errorLog.Add (Errors.IllegalInterfaceDeclaration, Location);
                }
                while (Accept (TokenClass.SemiColon))
                    ;
            }

            Expect (TokenClass.CloseBrace);

            return trait;
        }
            
        /*
         * mixin <name> {
         *     ...
         * }
         */
        private AstNode ParseMixin ()
        {
            string doc = Expect (TokenClass.Keyword, "mixin").Documentation;
            string name = Expect (TokenClass.Identifier).Value;

            MixinDeclaration mixin = new MixinDeclaration (Location, name, doc);

            Expect (TokenClass.OpenBrace);

            while (!Match (TokenClass.CloseBrace)) {
                if (Match (TokenClass.Keyword, "func")) {
                    FunctionDeclaration func = ParseFunction () as FunctionDeclaration;
                    mixin.AddMember (func);
                } else {
                    errorLog.Add (Errors.IllegalInterfaceDeclaration, Location);
                }
                while (Accept (TokenClass.SemiColon))
                    ;
            }

            Expect (TokenClass.CloseBrace);

            return mixin;
        }

        private string ParseClassName ()
        {
            StringBuilder ret = new StringBuilder ();
            do {
                string attr = Expect (TokenClass.Identifier).Value;
                ret.Append (attr);
                if (Match (TokenClass.MemberAccess)) {
                    ret.Append ('.');
                }
            } while (Accept (TokenClass.MemberAccess));
            return ret.ToString ();
        }


        private AstNode ParseFunction (bool prototype = false, ClassDeclaration cdecl = null)
        {
            string doc = Current.Documentation;

            if (Accept (TokenClass.Operator, "@")) {
                AstNode decorator = ParseExpression (); 
                FunctionDeclaration originalFunc = ParseFunction (prototype, cdecl) as FunctionDeclaration;
                return new DecoratedFunction (decorator.Location, decorator, originalFunc);
            }

            Expect (TokenClass.Keyword, "func");

            bool isInstanceMethod;
            bool isVariadic;
            bool hasKeywordArgs;
            bool hasDefaultVals;

            Token ident = Expect (TokenClass.Identifier);

            List<NamedParameter> parameters = ParseFuncParameters (
              out isInstanceMethod,
              out isVariadic,
              out hasKeywordArgs,
              out hasDefaultVals
            );

            FunctionDeclaration decl = new FunctionDeclaration (Location, ident != null ?
                ident.Value : "",
                isInstanceMethod,
                isVariadic,
                hasKeywordArgs,
                hasDefaultVals,
                parameters,
                doc
            );

            if (!prototype) {

                if (Accept (TokenClass.Operator, "=>")) {
                    decl.AddStatement (new ReturnStatement (Location, ParseExpression ()));
                } else {
                    Expect (TokenClass.OpenBrace);
                    StatementList scope = new StatementList (Location);

                    if (Match (TokenClass.Keyword, "super")) {
                        scope.AddStatement (ParseSuperCall (cdecl));
                    } else if (cdecl != null && cdecl.Name == decl.Name) {
                        /*
                         * If this is infact a constructor and no super call is provided, we must implicitly call super ()
                         */
                        scope.AddStatement (new SuperCallStatement (decl.Location, cdecl, new ArgumentList (decl.Location)));
                    }

                    while (!Match (TokenClass.CloseBrace)) {
                        scope.AddStatement (ParseStatement ());
                    }

                    decl.AddStatement (scope);
                    Expect (TokenClass.CloseBrace);
                }
            }
            return decl;
        }

        private List<NamedParameter> ParseFuncParameters (
            out bool isInstanceMethod,
            out bool isVariadic,
            out bool hasKeywordArgs,
            out bool hasDefaultValues)
        {
            isVariadic = false;
            hasKeywordArgs = false;
            isInstanceMethod = false;
            hasDefaultValues = false;
            List<NamedParameter> ret = new List<NamedParameter> ();
            Expect (TokenClass.OpenParan);
            if (Accept (TokenClass.Keyword, "self")) {
                isInstanceMethod = true;
                if (!Accept (TokenClass.Comma)) {
                    Expect (TokenClass.CloseParan);
                    return ret;
                }
            }

            while (!Match (TokenClass.CloseParan)) {

                if (!hasKeywordArgs && Accept (TokenClass.Operator, "**")) {
                    hasKeywordArgs = true;
                    Token ident = Expect (TokenClass.Identifier);
                    ret.Add (new NamedParameter (ident.Value));
                } else if (hasKeywordArgs) {
                    errorLog.Add (Errors.ArgumentAfterKeywordArgs, Location);
                } else if (!isVariadic && Accept (TokenClass.Operator, "*")) {
                    isVariadic = true;
                    Token ident = Expect (TokenClass.Identifier);
                    ret.Add (new NamedParameter (ident.Value));
                } else if (isVariadic) {
                    errorLog.Add (Errors.ArgumentAfterVariadicArgs, Location);
                } else {
                    Token param = Expect (TokenClass.Identifier);

                    AstNode type = null;
                    AstNode value = null;

                    if (Accept (TokenClass.Colon)) {
                        type = ParseExpression ();
                    }

                    if (Accept (TokenClass.Operator, "=")) {
                        value = ParseExpression ();
                        hasDefaultValues = true;
                    } else if (hasDefaultValues) {
                        //TODO: create error
                    }

                    ret.Add (new NamedParameter (param.Value, type, value));
                }

                if (!Accept (TokenClass.Comma)) {
                    break;
                }
            }
            Expect (TokenClass.CloseParan);
            return ret;
        }

        #endregion

        #region Statements

        /*
         * use <module> |
         * use <class> from <module>
         */
        private UseStatement ParseUse ()
        {
            Expect (TokenClass.Keyword, "use");
            bool relative = Accept (TokenClass.MemberAccess);
            string ident = "";

            if (!Match (TokenClass.Operator, "*")) {
                ident = ParseModuleName ();
            }

            if (Match (TokenClass.Keyword, "from") || Match (TokenClass.Comma) ||
                Match (TokenClass.Operator, "*")) {
                List<string> items = new List<string> ();
                bool wildcard = false;
                if (!Accept (TokenClass.Operator, "*")) {
                    items.Add (ident);
                    Accept (TokenClass.Comma);
                    while (!Match (TokenClass.Keyword, "from")) {
                        Token item = Expect (TokenClass.Identifier);
                        items.Add (item.Value);
                        if (!Accept (TokenClass.Comma)) {
                            break;
                        }
                    }
                } else {
                    wildcard = true;
                }
                Expect (TokenClass.Keyword, "from");

                relative = Accept (TokenClass.MemberAccess);
                string module = ParseModuleName ();
                return new UseStatement (Location, module, items, wildcard, relative);
            }
            return new UseStatement (Location, ident, relative);
        }

        private string ParseModuleName ()
        {
            Token initIdent = Expect (TokenClass.Identifier);

            if (Match (TokenClass.MemberAccess)) {
                StringBuilder accum = new StringBuilder ();
                accum.Append (initIdent.Value);
                while (Accept (TokenClass.MemberAccess)) {
                    Token ident = Expect (TokenClass.Identifier);
                    accum.Append (Path.DirectorySeparatorChar);
                    accum.Append (ident.Value);
                }
                return accum.ToString ();

            }
            return initIdent.Value;
        }

        private AstNode ParseStatement ()
        {
            try {
                return DoParseStatement ();
            } catch  (SyntaxException) {
                Synchronize ();
                return null;
            }
        }

        private AstNode DoParseStatement ()
        {
            if (Match (TokenClass.Keyword)) {
                switch (Current.Value) {
                case "class":
                    return ParseClass ();
                case "enum":
                    return ParseEnum ();
                case "contract":
                    return ParseContract ();
                case "trait":
                    return ParseTrait ();
                case "mixin":
                    return ParseMixin ();
                case "extend":
                    return ParseExtend ();
                case "func":
                    return ParseFunction ();
                case "if":
                    return ParseIf ();
                case "given":
                    return ParseGiven ();
                case "for":
                    return ParseFor ();
                case "foreach":
                    return ParseForeach ();
                case "with":
                    return ParseWith ();
                case "while":
                    return ParseWhile ();
                case "do":
                    return ParseDoWhile ();
                case "use":
                    return ParseUse ();
                case "return":
                    return ParseReturn ();
                case "raise":
                    return ParseRaise ();
                case "yield":
                    return ParseYield ();
                case "try":
                    return ParseTryExcept ();
                case "global":
                    return ParseAssignStatement ();
                case "break":
                    Accept (TokenClass.Keyword);
                    return new BreakStatement (Location);
                case "continue":
                    Accept (TokenClass.Keyword);
                    return new ContinueStatement (Location);
                case "super":
                    errorLog.Add (Errors.SuperCalledAfter, Location);
                    return ParseSuperCall (new ClassDeclaration (Location, "", null));
                }
            }

            if (Match (TokenClass.OpenBrace)) {
                return ParseBlock ();
            } else if (Accept (TokenClass.SemiColon)) {
                return new Statement (Location);
            } else if (Match (TokenClass.Operator, "@")) {
                return ParseFunction ();
            } else if (PeekToken (1) != null && PeekToken (1).Class == TokenClass.Comma) {
                return ParseAssignStatement ();
            } else {
                AstNode node = ParseExpression ();
                if (node == null) {
                    MakeError ();
                }
                return new Expression (Location, node);
            }
        }

        private AstNode ParseBlock ()
        {
            CodeBlock ret = new CodeBlock (Location);
            Expect (TokenClass.OpenBrace);

            while (!Match (TokenClass.CloseBrace)) {
                ret.Add (ParseStatement ());
            }

            Expect (TokenClass.CloseBrace);
            return ret;
        }

        /*
         * extend <class> [use <mixin>, ...] { 
         *      ...
         * }
         */
        private AstNode ParseExtend ()
        {
            Expect (TokenClass.Keyword, "extend");
            AstNode clazz = ParseExpression ();

            ExtendStatement statement = new ExtendStatement (clazz.Location, clazz, "");

            if (Accept (TokenClass.Keyword, "use")) {
                do {
                    statement.Mixins.Add (ParseExpression ());
                } while (Accept (TokenClass.Comma));
            }

            if (Accept (TokenClass.OpenBrace)) {
                while (!Match (TokenClass.CloseBrace) & !EndOfStream) {
                    statement.AddMember (ParseFunction ());
                }
                Expect (TokenClass.CloseBrace);
            }

            return statement;
        }

        /*
         * try {
         * 
         * } except [(<identifier> as <type>)] {
         * 
         * }
         */
        private AstNode ParseTryExcept ()
        {
            string exceptionVariable = null;
            Expect (TokenClass.Keyword, "try");
            AstNode tryBody = ParseStatement ();
            ArgumentList typeList = new ArgumentList (Location);
            Expect (TokenClass.Keyword, "except");
            if (Accept (TokenClass.OpenParan)) {
                Token ident = Expect (TokenClass.Identifier);
                if (Accept (TokenClass.Operator, "as")) {
                    typeList = ParseTypeList ();
                }
                Expect (TokenClass.CloseParan);
                exceptionVariable = ident.Value;
            }
            AstNode exceptBody = ParseStatement ();
            return new TryExceptStatement (Location, exceptionVariable, tryBody, exceptBody, typeList);
        }

        private ArgumentList ParseTypeList ()
        {
            ArgumentList argList = new ArgumentList (Location);
            while (!Match (TokenClass.CloseParan)) {
                argList.AddArgument (ParseExpression ());
                if (!Accept (TokenClass.Comma)) {
                    break;
                }
            }
            return argList;
        }

        private VariableDeclaration ParseVariableDeclaration ()
        {
            bool global = false;

            if (Accept (TokenClass.Keyword, "global")) {
                global = true;
            } else {
                Expect (TokenClass.Keyword, "local");
            }

            Token ident = Expect (TokenClass.Identifier);
            AstNode value = null;
            if (Accept (TokenClass.Operator, "=")) {
                value = new BinaryExpression (Location,
                    BinaryOperation.Assign,
                    new NameExpression (ident.Location, ident.Value),
                    ParseExpression ()
                );
            }
            return new VariableDeclaration (Location, global, ident.Value, value);
        }

        /*
         * given <condition> {
         *     when <expression>
         *         <statement>
         * }
         */
        private AstNode ParseGiven ()
        {
            SourceLocation location = Location;
            Expect (TokenClass.Keyword, "given");
            Expect (TokenClass.OpenParan);
            AstNode value = ParseExpression ();
            Expect (TokenClass.CloseParan);
            Expect (TokenClass.OpenBrace);
            AstNode defaultBlock = new CompilationUnit (Location);
            List<WhenStatement> whenStatements = new List<WhenStatement> ();
            while (!EndOfStream && !Match (TokenClass.CloseBrace)) {
                whenStatements.Add (ParseWhen ());
                if (Accept (TokenClass.Keyword, "default")) {
                    defaultBlock = ParseStatement (); 
                }
            }
            Expect (TokenClass.CloseBrace);
            return new GivenStatement (location, value, whenStatements, defaultBlock);
        }

        /*
         * given <condition> {
         *     when <expression>
         *         <statement>
         * }
         */
        private WhenStatement ParseWhen ()
        {
            SourceLocation location = Location;
            Expect (TokenClass.Keyword, "when");
            AstNode value = ParseExpression ();
            AstNode body = ParseStatement ();

            return new WhenStatement (location, value, body);
        }

        /*
         * if (<expression> 
         *     <statement>
         * [
         * else
         *     <statement>
         * ]
         */
        private AstNode ParseIf ()
        {
            SourceLocation location = Location;
            Expect (TokenClass.Keyword, "if");
            Expect (TokenClass.OpenParan);
            AstNode predicate = ParseExpression ();
            Expect (TokenClass.CloseParan);
            AstNode body = ParseStatement ();
            AstNode elseBody = null;
            if (Accept (TokenClass.Keyword, "else")) {
                elseBody = ParseStatement ();
            }
            return new IfStatement (location, predicate, body, elseBody);
        }

        /*
         * for (<initializer>; <condition>; <afterthought>)
         *     <statement>
         * --- OR ---
         * for (<identifier> in <expression) 
         *     <statement>
         */
        private AstNode ParseFor ()
        {
            SourceLocation location = Location;

            if (Match (3, TokenClass.Keyword, "in") || Match (3, TokenClass.Comma)) {
                return ParseForeach ();
            }

            context.Warn (
                WarningType.SyntaxWarning,
                "C style for loop is deprecated infavor of for each loop."
            );

            Expect (TokenClass.Keyword, "for");
            Expect (TokenClass.OpenParan);
            AstNode initializer = new Expression (Location, ParseExpression ());
            Expect (TokenClass.SemiColon);
            AstNode condition = ParseExpression ();
            Expect (TokenClass.SemiColon);
            AstNode afterThought = new Expression (Location, ParseExpression ());
            Expect (TokenClass.CloseParan);
            AstNode body = ParseStatement ();

            return new ForStatement (location, initializer, condition, afterThought, body);
        }

        /*
         * NOTE: Usage of this foreach keyword is deprecated infavor of doing 
         * for (<identifier> in <expression>
         * foreach (<identifier> in <expression>)
         *     <statement>
         */
        private AstNode ParseForeach ()
        {
            if (Match (TokenClass.Keyword, "for")) {
                Expect (TokenClass.Keyword, "for");
            } else {
                Expect (TokenClass.Keyword, "foreach");
                context.Warn (
                    WarningType.SyntaxWarning,
                    "'foreach' keyword is deprecated! Use 'for' instead."
                );
            }
            Expect (TokenClass.OpenParan);
            bool anotherValue = false;
            List<string> identifiers = new List<string> ();
            do {
                Token identifier = Expect (TokenClass.Identifier);
                anotherValue = Accept (TokenClass.Comma);
                identifiers.Add (identifier.Value);
            } while (anotherValue);

            Expect (TokenClass.Keyword, "in");
            AstNode expr = ParseExpression ();
            Expect (TokenClass.CloseParan);
            AstNode body = ParseStatement ();
            return new ForeachStatement (Location, identifiers, expr, body);
        }

        /*
         * do 
         *     <statement>
         * while (<expression>)
         */
        private AstNode ParseDoWhile ()
        {
            SourceLocation location = Location;
            Expect (TokenClass.Keyword, "do");
            AstNode body = ParseStatement ();
            Expect (TokenClass.Keyword, "while");
            Expect (TokenClass.OpenParan);
            AstNode condition = ParseExpression ();
            Expect (TokenClass.CloseParan);
            return new DoStatement (location, condition, body);
        }

        /*
         * while (<expression>) 
         *     <statement>
         */
        private AstNode ParseWhile ()
        {
            SourceLocation location = Location;
            Expect (TokenClass.Keyword, "while");
            Expect (TokenClass.OpenParan);
            AstNode condition = ParseExpression ();
            Expect (TokenClass.CloseParan);
            AstNode body = ParseStatement ();
            return new WhileStatement (location, condition, body);
        }

        /*
         * with (<expression) 
         *      <statement>
         */
        private AstNode ParseWith ()
        {
            SourceLocation location = Location;
            Expect (TokenClass.Keyword, "with");
            Expect (TokenClass.OpenParan);
            AstNode value = ParseExpression ();
            Expect (TokenClass.CloseParan);
            AstNode body = ParseStatement ();
            return new WithStatement (location, value, body);
        }

        /*
         * raise <expression>;
         */
        private AstNode ParseRaise ()
        {
            Expect (TokenClass.Keyword, "raise");
            return new RaiseStatement (Location, ParseExpression ());
        }

        private AstNode ParseReturn ()
        {
            Expect (TokenClass.Keyword, "return");

            if (Accept (TokenClass.SemiColon)) {
                return new ReturnStatement (Location, new CodeBlock (Location));
            } else {

                AstNode ret = new ReturnStatement (Location, ParseExpression ());

                if (Accept (TokenClass.Keyword, "when")) {
                    return new IfStatement (Location, ParseExpression (), ret);
                }

                return ret;
            }
        }

        private AstNode ParseYield ()
        {
            Expect (TokenClass.Keyword, "yield");
            return new YieldStatement (Location, ParseExpression ());
        }


        private AstNode ParseAssignStatement ()
        {
            List<string> identifiers = new List<string> ();

            bool isGlobal = false;

            if (Accept (TokenClass.Keyword, "global")) {
                isGlobal = true;
            } else {
                Accept (TokenClass.Keyword, "local");
            }

            SourceLocation location = Location;

            while (!Match (TokenClass.Operator, "=") && !EndOfStream) {
                Token ident = Expect (TokenClass.Identifier);
                identifiers.Add (ident.Value);

                if (!Match (TokenClass.Operator, "=")) {
                    Expect (TokenClass.Comma);
                }
            }

            Expect (TokenClass.Operator, "=");

            bool isPacked = false;

            List<AstNode> expressions = new List<AstNode> ();

            do {
                expressions.Add (ParseExpression ());
            } while (Accept (TokenClass.Comma));

            if (identifiers.Count > 1 && expressions.Count == 1) {
                isPacked = true;
            }

            return new AssignStatement (location, isGlobal, identifiers, expressions, isPacked);
        }

        #endregion

        #region Expressions

        private AstNode ParseExpression ()
        {
            return ParseGeneratorExpression ();
        }

        private AstNode ParseGeneratorExpression ()
        {
            AstNode expr = ParseAssign ();

            bool isGenExpr = Match (TokenClass.Keyword, "for")
                && PeekToken (1) != null
                && PeekToken (1).Class != TokenClass.OpenParan;
            
            if (isGenExpr) {
                Expect (TokenClass.Keyword, "for");
                string ident = Expect (TokenClass.Identifier).Value;
                Expect (TokenClass.Keyword, "in");
                AstNode iterator = ParseExpression ();
                AstNode predicate = null;
                if (Accept (TokenClass.Keyword, "if")) {
                    predicate = ParseExpression ();
                }
                return new GeneratorExpression (expr.Location, expr, ident, iterator, predicate);
            }
            return expr;
        }

        private AstNode ParseAssign ()
        {
            AstNode expr = ParseTernaryIfElse ();
            while (Match (TokenClass.Operator)) {
                switch (Current.Value) {
                case "=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Assign,
                        expr, ParseTernaryIfElse ());
                    continue;
                case "+=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Assign, expr,
                        new BinaryExpression (Location,
                            BinaryOperation.Add, expr,
                            ParseTernaryIfElse ()));
                    continue;
                case "-=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Assign, expr,
                        new BinaryExpression (Location,
                            BinaryOperation.Sub,
                            expr,
                            ParseTernaryIfElse ()));
                    continue;
                case "*=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Assign, expr,
                        new BinaryExpression (Location,
                            BinaryOperation.Mul,
                            expr,
                            ParseTernaryIfElse ()));
                    continue;
                case "/=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Assign, expr,
                        new BinaryExpression (Location,
                            BinaryOperation.Div,
                            expr,
                            ParseTernaryIfElse ()));
                    continue;
                case "%=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Assign, expr,
                        new BinaryExpression (Location,
                            BinaryOperation.Mod,
                            expr,
                            ParseTernaryIfElse ()));
                    continue;
                case "^=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Assign, expr,
                        new BinaryExpression (Location,
                            BinaryOperation.Xor,
                            expr, 
                            ParseTernaryIfElse ()));
                    continue;
                case "&=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Assign, expr,
                        new BinaryExpression (Location,
                            BinaryOperation.And,
                            expr,
                            ParseTernaryIfElse ()));
                    continue;
                case "|=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Assign, expr,
                        new BinaryExpression (Location,
                            BinaryOperation.Or,
                            expr,
                            ParseTernaryIfElse ()));
                    continue;
                case "<<=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Assign, expr,
                        new BinaryExpression (Location,
                            BinaryOperation.LeftShift,
                            expr,
                            ParseTernaryIfElse ()));
                    continue;
                case ">>=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Assign, expr,
                        new BinaryExpression (Location,
                            BinaryOperation.RightShift,
                            expr,
                            ParseTernaryIfElse ()));
                    continue;
                default:
                    break;
                }
                break;
            }
            return expr;
        }

        private AstNode ParseTernaryIfElse ()
        {
            AstNode expr = ParseRange ();

            int backup = position;

            if (Accept (TokenClass.Keyword, "when")) {
                AstNode condition = ParseExpression ();
                if (Accept (TokenClass.Keyword, "else")) {
                    AstNode altValue = ParseTernaryIfElse ();
                    expr = new TernaryExpression (expr.Location, condition, expr, altValue);
                } else {
                    position = backup;
                }
            }
            return expr;
        }

        private AstNode ParseRange ()
        {
            AstNode expr = ParseBoolOr ();
            while (Match (TokenClass.Operator)) {
                switch (Current.Value) {
                case "...":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (
                        Location,
                        BinaryOperation.ClosedRange,
                        expr,
                        ParseBoolOr ()
                    );
                    continue;
                case "..":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (
                        Location,
                        BinaryOperation.HalfRange,
                        expr,
                        ParseBoolOr ()
                    );
                    continue;
                default:
                    break;
                }
                break;
            }
            return expr;
        }

        private AstNode ParseBoolOr ()
        {
            AstNode expr = ParseBoolAnd ();
            while (Match (TokenClass.Operator)) {
                switch (Current.Value) {
                case "||":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.BoolOr, expr,
                        ParseBoolAnd ());
                    continue;
                case "??":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.NullCoalescing, expr,
                        ParseBoolAnd ());
                    continue;
                default:
                    break;
                }
                break;
            }
            return expr;
        }

        private AstNode ParseBoolAnd ()
        {
            AstNode expr = ParseOr ();
            while (Accept (TokenClass.Operator, "&&")) {
                expr = new BinaryExpression (Location, BinaryOperation.BoolAnd, expr, ParseOr ());
            }
            return expr;
        }

        private AstNode ParseOr ()
        {
            AstNode expr = ParseXor ();
            while (Accept (TokenClass.Operator, "|")) {
                expr = new BinaryExpression (Location, BinaryOperation.Or, expr, ParseXor ());
            }
            return expr;
        }

        private AstNode ParseXor ()
        {
            AstNode expr = ParseAnd ();
            while (Accept (TokenClass.Operator, "^")) {
                expr = new BinaryExpression (Location, BinaryOperation.Xor, expr, ParseAnd ());
            }
            return expr;
        }

        private AstNode ParseAnd ()
        {
            AstNode expr = ParseEquals ();
            while (Accept (TokenClass.Operator, "&")) {
                expr = new BinaryExpression (Location, BinaryOperation.And, expr,
                    ParseEquals ());
            }
            return expr;
        }

        private AstNode ParseEquals ()
        {
            AstNode expr = ParseRelationalOp ();
            while (Match (TokenClass.Operator)) {
                switch (Current.Value) {
                case "==":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Equals, expr,
                        ParseRelationalOp ());
                    continue;
                case "!=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.NotEquals, expr,
                        ParseRelationalOp ());
                    continue;
                default:
                    break;
                }
                break;
            }
            return expr;
        }

        private AstNode ParseRelationalOp ()
        {
            AstNode expr = ParseBitshift ();
            while (Match (TokenClass.Operator)) {
                switch (Current.Value) {
                case ">":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location,
                        BinaryOperation.GreaterThan,
                        expr,
                        ParseBitshift ()
                    );
                    continue;
                case "<":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location,
                        BinaryOperation.LessThan,
                        expr,
                        ParseBitshift ()
                    );
                    continue;
                case ">=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location,
                        BinaryOperation.GreaterThanOrEqu,
                        expr,
                        ParseBitshift ()
                    );
                    continue;
                case "<=":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location,
                        BinaryOperation.LessThanOrEqu,
                        expr,
                        ParseBitshift ()
                    );
                    continue;
                case "is":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location,
                        BinaryOperation.InstanceOf,
                        expr,
                        ParseBitshift ()
                    );
                    continue;
                case "isnot":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location,
                        BinaryOperation.NotInstanceOf,
                        expr,
                        ParseBitshift ()
                    );
                    continue;
                case "as":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location,
                        BinaryOperation.DynamicCast,
                        expr,
                        ParseBitshift ()
                    );
                    continue;
                default:
                    break;
                }
                break;
            }
            return expr;
        }

        private AstNode ParseBitshift ()
        {
            AstNode expr = ParseAdditive ();
            while (Match (TokenClass.Operator)) {
                switch (Current.Value) {
                case "<<":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.LeftShift, expr,
                        ParseAdditive ());
                    continue;
                case ">>":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.RightShift, expr,
                        ParseAdditive ());
                    continue;
                default:
                    break;
                }
                break;
            }
            return expr;
        }

        private AstNode ParseAdditive ()
        {
            AstNode expr = ParseMultiplicative ();
            while (Match (TokenClass.Operator)) {
                switch (Current.Value) {
                case "+":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Add, expr,
                        ParseMultiplicative ());
                    continue;
                case "-":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Sub, expr,
                        ParseMultiplicative ());
                    continue;
                default:
                    break;
                }
                break;
            }
            return expr;
        }

        private AstNode ParseMultiplicative ()
        {
            AstNode expr = ParseUnary ();
            while (Match (TokenClass.Operator)) {
                switch (Current.Value) {
                case "**":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Pow, expr,
                        ParseUnary ());
                    continue;
                case "*":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Mul, expr,
                        ParseUnary ());
                    continue;
                case "/":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Div, expr,
                        ParseUnary ());
                    continue;
                case "%":
                    Accept (TokenClass.Operator);
                    expr = new BinaryExpression (Location, BinaryOperation.Mod, expr,
                        ParseUnary ());
                    continue;
                default:
                    break;
                }
                break;
            }
            return expr;
        }

        private AstNode ParseUnary ()
        {
            if (Match (TokenClass.Operator)) {
                switch (Current.Value) {
                case "-":
                    Accept (TokenClass.Operator);
                    return new UnaryExpression (Location, UnaryOperation.Negate, ParseUnary ());
                case "~":
                    Accept (TokenClass.Operator);
                    return new UnaryExpression (Location, UnaryOperation.Not, ParseUnary ());
                case "!":
                    Accept (TokenClass.Operator);
                    return new UnaryExpression (Location, UnaryOperation.BoolNot, ParseUnary ());

                }
            }
            return ParseCallSubscriptAccess ();
        }

        private AstNode ParseCallSubscriptAccess ()
        {
            return ParseCallSubscriptAccess (ParseMatchExpression ());
        }

        private AstNode ParseCallSubscriptAccess (AstNode lvalue)
        {
            if (Current != null) {
                switch (Current.Class) {
                case TokenClass.OpenParan:
                    return ParseCallSubscriptAccess (
                        new CallExpression (Location, lvalue, ParseArgumentList ())
                    );
                case TokenClass.OpenBracket:
                    return ParseCallSubscriptAccess (ParseIndexerExpression (lvalue));
                case TokenClass.MemberAccess:
                    return ParseCallSubscriptAccess (ParseGetExpression (lvalue));
                case TokenClass.MemberDefaultAccess:
                    return ParseCallSubscriptAccess (ParseGetOrNullExpression (lvalue));
                }
            }

            return lvalue;
        }

        private AstNode ParseIndexerExpression (AstNode lvalue)
        {
            Expect (TokenClass.OpenBracket);

            if (Accept (TokenClass.Colon)) {
                return ParseSlice (lvalue, null);
            }

            AstNode index = ParseExpression ();

            if (Accept (TokenClass.Colon)) {
                return ParseSlice (lvalue, index);
            }

            Expect (TokenClass.CloseBracket);
            return new IndexerExpression (Location, lvalue, index);
        }

        private AstNode ParseGetExpression (AstNode lvalue)
        {
            Expect (TokenClass.MemberAccess);
            Token ident = Expect (TokenClass.Identifier);
            return new MemberExpression (Location, lvalue, ident.Value);
        }

        private AstNode ParseGetOrNullExpression (AstNode lvalue)
        {
            Expect (TokenClass.MemberDefaultAccess);
            Token ident = Expect (TokenClass.Identifier);
            return new MemberDefaultExpression (Location, lvalue, ident.Value);
        }

        private AstNode ParseMatchExpression ()
        {
            if (Accept (TokenClass.Keyword, "match")) {
                MatchExpression expr = new MatchExpression (Location, ParseExpression ());
                Expect (TokenClass.OpenBrace);
                while (Accept (TokenClass.Keyword, "case")) {
                    AstNode condition = null;
                    AstNode pattern = ParsePattern ();
                    if (Accept (TokenClass.Keyword, "when")) {
                        condition = ParseExpression ();
                    }
                    AstNode value = null;

                    if (Accept (TokenClass.Operator, "=>")) {
                        value = ParseExpression ();
                        expr.AddCase (new CaseExpression (
                            pattern.Location,
                            pattern,
                            condition,
                            value,
                            false
                        ));
                    } else {
                        value = ParseStatement ();
                        expr.AddCase (new CaseExpression (
                            pattern.Location,
                            pattern, 
                            condition,
                            value,
                            true
                        ));
                    }
                }
                Expect (TokenClass.CloseBrace);
                return expr;
            }

            return ParseLambdaExpression ();
        }

        private AstNode ParsePattern ()
        {
            return ParsePatternOr ();
        }

        private AstNode ParsePatternOr ()
        {
            AstNode expr = ParsePatternAnd ();
            while (Match (TokenClass.Operator, "|")) {
                Accept (TokenClass.Operator);
                expr = new PatternExpression (Location,
                    BinaryOperation.Or,
                    expr,
                    ParsePatternAnd ()
                );
            }
            return expr;
        }

        private AstNode ParsePatternAnd ()
        {
            AstNode expr = ParsePatternExtractor ();
            while (Match (TokenClass.Operator, "&")) {
                Accept (TokenClass.Operator);
                expr = new PatternExpression (Location,
                    BinaryOperation.And,
                    expr,
                    ParsePatternExtractor ()
                );
            }
            return expr;
        }

        private AstNode ParsePatternExtractor ()
        {
            AstNode ret = ParsePatternTerm ();

            if (Accept (TokenClass.OpenParan)) {
                ret = new PatternExtractExpression (Location, ret);

                while (!Match (TokenClass.CloseParan)) {
                    Token capture = Expect (TokenClass.Identifier);

                    ((PatternExtractExpression)ret).Captures.Add (capture.Value);

                    if (!Match (TokenClass.CloseParan)) {
                        Expect (TokenClass.Comma);
                    }
                }

                Expect (TokenClass.CloseParan);
            }

            return ret;
        }

        private AstNode ParsePatternTerm ()
        {
            return ParseLiteral ();
        }

        private AstNode ParseLambdaExpression ()
        {
            if (Accept (TokenClass.Keyword, "lambda")) {
                bool isInstanceMethod;
                bool isVariadic;
                bool acceptsKwargs;
                bool hasDefaultVals;

                List<NamedParameter> parameters = ParseFuncParameters (
                    out isInstanceMethod,
                    out isVariadic,
                    out acceptsKwargs,
                    out hasDefaultVals
                );

                LambdaExpression decl = new LambdaExpression (
                    Location, 
                    isInstanceMethod, 
                    isVariadic, 
                    acceptsKwargs,
                    hasDefaultVals,
                    parameters
                );

                if (Accept (TokenClass.Operator, "=>")) {
                    decl.AddStatement (new ReturnStatement (Location, ParseExpression ()));
                } else {
                    decl.AddStatement (ParseStatement ());
                }

                return decl;
            }

            return ParseLiteral ();
        }
            
        private AstNode ParseLiteral ()
        {
            if (Current == null) {
                errorLog.Add (Errors.UnexpectedEndOfFile, Location);
                throw new EndOfFileException ();
            }

            switch (Current.Class) {
            case TokenClass.OpenBracket:
                return ParseListLiteral ();
            case TokenClass.OpenBrace:
                return ParseHashLiteral ();
            case TokenClass.OpenParan:
                ReadToken ();
                AstNode expr = ParseExpression ();
                if (Accept (TokenClass.Comma)) {
                    return ParseTupleLiteral (expr);
                }
                Expect (TokenClass.CloseParan);
                return expr;
            default:
                return ParseTerminal ();
            }
        }

        private AstNode ParseListLiteral ()
        {
            Expect (TokenClass.OpenBracket);
            ListExpression ret = new ListExpression (Location);
            while (!Match (TokenClass.CloseBracket)) {
                AstNode expr = ParseAssign ();
                if (Accept (TokenClass.Keyword, "for")) {
                    string ident = Expect (TokenClass.Identifier).Value;
                    Expect (TokenClass.Keyword, "in");
                    AstNode iterator = ParseExpression ();
                    AstNode predicate = null;
                    if (Accept (TokenClass.Keyword, "if")) {
                        predicate = ParseExpression ();
                    }
                    Expect (TokenClass.CloseBracket);
                    return new ListCompExpression (expr.Location, expr, ident, iterator, predicate);
                }
                ret.AddItem (expr);
                if (!Accept (TokenClass.Comma)) {
                    break;
                }
            }
            Expect (TokenClass.CloseBracket);
            return ret;
        }

        private AstNode ParseHashLiteral ()
        {
            Expect (TokenClass.OpenBrace);
            HashExpression ret = new HashExpression (Location);
            while (!Match (TokenClass.CloseBrace)) {
                AstNode key = ParseExpression ();
                Expect (TokenClass.Colon);
                AstNode value = ParseExpression ();
                ret.AddItem (key, value);
                if (!Accept (TokenClass.Comma)) {
                    break;
                }
            }
            Expect (TokenClass.CloseBrace);
            return ret;
        }

        private AstNode ParseTupleLiteral (AstNode firstVal)
        {
            TupleExpression tuple = new TupleExpression (Location);
            tuple.AddItem (firstVal);
            while (!Match (TokenClass.CloseParan)) {
                tuple.AddItem (ParseExpression ());
                if (!Accept (TokenClass.Comma)) {
                    break;
                }
            }
            Expect (TokenClass.CloseParan);
            return tuple;
        }

        private AstNode ParseTerminal ()
        {
            switch (Current.Class) {
            case TokenClass.Identifier:
                return new NameExpression (Location, ReadToken ().Value);
            case TokenClass.IntLiteral:
                long lval64;
                if (!long.TryParse (Current.Value, out lval64)) {
                    errorLog.Add (Errors.IntegerOverBounds, Current.Location);
                }
                ReadToken ();
                return new IntegerExpression (Location, lval64);
            case TokenClass.BigIntLiteral:
                BigInteger bintVal;
                if (!BigInteger.TryParse (Current.Value, out bintVal)) {
                    errorLog.Add (Errors.IntegerOverBounds, Current.Location);
                }
                ReadToken ();
                return new BigIntegerExpression (Location, bintVal);
            case TokenClass.FloatLiteral:
                return new FloatExpression (Location, double.Parse (
                    ReadToken ().Value));
            case TokenClass.InterpolatedStringLiteral:
                AstNode val = ParseString (Location, ReadToken ().Value);
                if (val == null) {
                    MakeError ();
                    return new StringExpression (Location, "");
                }
                return val;
            case TokenClass.StringLiteral:
                return new StringExpression (Location, ReadToken ().Value);
            case TokenClass.BinaryStringLiteral:
                return new StringExpression (Location, ReadToken ().Value, true);
            case TokenClass.Keyword:
                switch (Current.Value) {
                case "self":
                    ReadToken ();
                    return new SelfExpression (Location);
                case "true":
                    ReadToken ();
                    return new TrueExpression (Location);
                case "false":
                    ReadToken ();
                    return new FalseExpression (Location);
                case "null":
                    ReadToken ();
                    return new NullExpression (Location);
                }
                break;
            }
        
            MakeError ();
            return null;
        }
            
        private AstNode ParseSlice (AstNode lvalue, AstNode start)
        {
            if (Accept (TokenClass.CloseBracket)) {
                return new SliceExpression (lvalue.Location, lvalue, start, null, null);
            }

            AstNode end = null;
            AstNode step = null;

            if (Accept (TokenClass.Colon)) {
                step = ParseExpression ();
            } else {
                end = ParseExpression ();

                if (Accept (TokenClass.Colon)) {
                    step = ParseExpression ();
                }
            }

            Expect (TokenClass.CloseBracket);

            return new SliceExpression (lvalue.Location, lvalue, start, end, step);
        }
            
        private SuperCallStatement ParseSuperCall (ClassDeclaration parent)
        {
            SourceLocation location = Location;
            Expect (TokenClass.Keyword, "super");
            ArgumentList argumentList = ParseArgumentList ();
            while (Accept (TokenClass.SemiColon))
                ;
            return new SuperCallStatement (location, parent, argumentList);
        }

        private ArgumentList ParseArgumentList ()
        {
            ArgumentList argList = new ArgumentList (Location);
            Expect (TokenClass.OpenParan);
            KeywordArgumentList kwargs = null;
            while (!Match (TokenClass.CloseParan)) {
                if (Accept (TokenClass.Operator, "*")) {
                    argList.Packed = true;
                    argList.AddArgument (ParseExpression ());
                    break;
                }
                AstNode arg = ParseExpression ();
                if (Accept (TokenClass.Colon)) {
                    if (kwargs == null) {
                        kwargs = new KeywordArgumentList (arg.Location);
                    }
                    NameExpression ident = arg as NameExpression;
                    AstNode val = ParseExpression ();
                    if (ident == null) {
                        errorLog.Add (Errors.ExpectedIdentifier, Location);
                    } else {
                        kwargs.Add (ident.Value, val);
                    }
                } else
                    argList.AddArgument (arg);
                if (!Accept (TokenClass.Comma)) {
                    break;
                }

            }
            if (kwargs != null) {
                argList.AddArgument (kwargs);
            }
            Expect (TokenClass.CloseParan);
            return argList;

        }

        private AstNode ParseString (SourceLocation loc, string str)
        {
            /*
             * This might be a *bit* hacky, but, basically Iodine string interpolation
             * is *basically* just syntactic sugar for Str.format (...)
             */
            int pos = 0;
            string accum = "";
            List<string> subExpressions = new List<string> ();
            while (pos < str.Length) {
                if (str [pos] == '#' && str.Length != pos + 1 && str [pos + 1] == '{') {
                    string substr = str.Substring (pos + 2);
                    if (substr.IndexOf ('}') == -1)
                        return null;
                    substr = substr.Substring (0, substr.IndexOf ('}'));
                    pos += substr.Length + 3;
                    subExpressions.Add (substr);
                    accum += "{}";

                } else {
                    accum += str [pos++];
                }
            }

            StringExpression ret = new StringExpression (loc, accum);

            foreach (string name in subExpressions) {
                Tokenizer tokenizer = new Tokenizer (
                    errorLog,
                    SourceUnit.CreateFromSource (name).GetReader ()
                );

                Parser parser = new Parser (context, tokenizer.Scan ());
                var expression = parser.ParseExpression ();
                ret.AddSubExpression (expression);
            }
            return ret;
        }

        #endregion

        #region Token Manipulation functions

        public void Synchronize ()
        {
            while (Current != null) {
                Token tok = ReadToken ();
                switch (tok.Class) {
                case TokenClass.CloseBracket:
                case TokenClass.SemiColon:
                    return;
                }
            }
        }

        public void AddToken (Token token)
        {
            tokens.Add (token);
        }

        public bool Match (TokenClass clazz)
        {
            return PeekToken () != null && PeekToken ().Class == clazz;
        }

        public bool Match (TokenClass clazz1, TokenClass clazz2)
        {
            return PeekToken () != null &&
                PeekToken ().Class == clazz1 &&
                PeekToken (1) != null &&
                PeekToken (1).Class == clazz2;
        }

        public bool Match (TokenClass clazz, string val)
        {
            return PeekToken () != null &&
                PeekToken ().Class == clazz &&
                PeekToken ().Value == val;
        }

        public bool Match (int lookahead, TokenClass clazz)
        {
            return PeekToken (lookahead) != null &&PeekToken (lookahead).Class == clazz;
        }

        public bool Match (int lookahead, TokenClass clazz, string val)
        {
            return PeekToken (lookahead) != null &&
                PeekToken (lookahead).Class == clazz &&
                PeekToken (lookahead).Value == val;
        }

        public bool Accept (TokenClass clazz)
        {
            if (PeekToken () != null && PeekToken ().Class == clazz) {
                ReadToken ();
                return true;
            }
            return false;
        }

        public bool Accept (TokenClass clazz, ref Token token)
        {
            if (PeekToken () != null && PeekToken ().Class == clazz) {
                token = ReadToken ();
                return true;
            }
            return false;
        }

        public bool Accept (TokenClass clazz, string val)
        {
            if (PeekToken () != null && PeekToken ().Class == clazz && PeekToken ().Value == val) {
                ReadToken ();
                return true;
            }
            return false;
        }

        public Token Expect (TokenClass clazz)
        {
            Token ret = null;

            if (Accept (clazz, ref ret)) {
                return ret;
            }

            Token offender = ReadToken ();

            if (offender != null) {
                errorLog.Add (Errors.UnexpectedToken, offender.Location, offender.Value);
                throw new SyntaxException (errorLog);
            }

            errorLog.Add (Errors.UnexpectedEndOfFile, Location);
            throw new EndOfFileException ();
        }

        public Token Expect (TokenClass clazz, string val)
        {
            Token ret = PeekToken ();

            if (Accept (clazz, val)) {
                return ret;
            }

            Token offender = ReadToken ();

            if (offender != null) {
                errorLog.Add (Errors.UnexpectedToken, offender.Location, offender.Value);
                throw new SyntaxException (errorLog);
            }

            errorLog.Add (Errors.UnexpectedEndOfFile, Location);
            throw new EndOfFileException ();
        }

        public void MakeError ()
        {
            if (PeekToken () == null) {
                errorLog.Add (Errors.UnexpectedEndOfFile, Location);
                throw new EndOfFileException ();
            }

            errorLog.Add (
                Errors.UnexpectedToken,
                PeekToken ().Location,
                ReadToken ().Value
            );

            throw new SyntaxException (errorLog);
        }

        private Token PeekToken ()
        {
            return PeekToken (0);
        }

        public Token PeekToken (int n)
        {
            if (position + n < tokens.Count) {
                return tokens [position + n];
            }
            return null;
        }

        public Token ReadToken ()
        {
            if (position >= tokens.Count) {
                return null;
            }
            return tokens [position++];
        }
        #endregion
    }
}

