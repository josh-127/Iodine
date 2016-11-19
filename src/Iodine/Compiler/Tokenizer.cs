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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Numerics;

namespace Iodine.Compiler
{
    /// <summary>
    /// Iodine lexer class, tokenizes our source into a list of Token objects represented as a TokenStream object.
    /// </summary>
    public sealed class Tokenizer
    {
        const string OperatorChars = "+-*/=<>~!&^|%@?.";

        private SourceReader source;

        private string lastDocStr = null;
        private ErrorSink errorLog;

        public Tokenizer (ErrorSink errorLog, SourceReader sourceReader)
        {
            this.errorLog = errorLog;
            this.source = sourceReader;
        }

        public IEnumerable<Token> Scan ()
        {
            List<Token> tokens = new List<Token> ();

            source.SkipWhitespace ();

            while (source.See ()) {
                Token nextToken = NextToken ();

                if (nextToken != null) {
                    tokens.Add (nextToken);

                    lastDocStr = null;
                }

                source.SkipWhitespace ();
            }

            if (errorLog.ErrorCount > 0) {
                throw new SyntaxException (errorLog);
            }
            return tokens;
        }

        private Token NextToken ()
        {
            char ch = source.Peek ();

            if (source.Peeks (2) == "0x") {
                return ReadHexNumber ();
            }

            if (char.IsDigit (ch)) {
                return ReadNumber ();
            }

            if (char.IsLetter (ch) || ch == '_') {
                return ReadIdentifier ();
            }

            if (source.Peeks (3) == "/**") {
                ReadDocComment ();
                return null;
            }

            if (source.Peeks (2) == "/*") {
                ReadLineComment ();
                return null;
            }

            switch (ch) {
            case '\"':
            case '\'':
                return ReadStringLiteral ();
            case '#':
                source.SkipLine ();
                return null;
            }

            var punctuators = new Dictionary<char, TokenClass> {
                { '{', TokenClass.OpenBrace },
                { '}', TokenClass.CloseBrace },
                { '(', TokenClass.OpenParan },
                { ')', TokenClass.CloseParan },
                { '[', TokenClass.OpenBracket },
                { ']', TokenClass.CloseBracket },
                { ';', TokenClass.SemiColon },
                { ':', TokenClass.Colon },
                { ',', TokenClass.Comma }
            };

            if (punctuators.ContainsKey (source.Peek ())) {
                char punctuator = source.Read ();

                return new Token (
                    punctuators [punctuator],
                    punctuator.ToString (),
                    lastDocStr,
                    source.Location
                );
            }

            if (OperatorChars.Contains (source.Peek ())) {
                return ReadOperator ();
            }

            if (char.IsLetter (ch)) {
                return ReadIdentifier ();
            }

            errorLog.Add (Errors.UnexpectedToken, source.Location, source.Read ());

            return null;
        }

        private Token ReadNumber ()
        {
            StringBuilder accum = new StringBuilder ();

            while (source.See () && char.IsDigit (source.Peek ())) {
                accum.Append (source.Read ());
            }

            if (source.Peek () == '.') {
                return ReadFloat (accum);
            }

            bool isBigInt = source.Peek () == 'L';

            string val = accum.ToString ();
            bool fitsInInteger;
           
            if (isBigInt) {
                source.Skip ();
                BigInteger valBig;
                fitsInInteger = BigInteger.TryParse ("0" + val, out valBig);
            } else {
                long val64;
                fitsInInteger = long.TryParse (val, out val64);
            }

            if (!fitsInInteger) {
                errorLog.Add (Errors.IntegerOverBounds, source.Location);
            }

            TokenClass tokenClass = isBigInt ?
                TokenClass.BigIntLiteral :
                TokenClass.IntLiteral;

            if (string.IsNullOrEmpty (val)) {
                errorLog.Add (Errors.IllegalSyntax, source.Location);
            }

            return new Token (tokenClass, accum.ToString (), lastDocStr, source.Location);
        }

        private Token ReadHexNumber ()
        {
            StringBuilder accum = new StringBuilder ();

            source.Skip (2);

            while (source.See () && IsHexNumber (source.Peek ())) {
                accum.Append (source.Read ());
            }

            string val = accum.ToString ();

            bool isBigInt = source.Peek () == 'L';

            bool fitsInInteger = false;

            if (isBigInt) {
                source.Skip ();
                BigInteger valBig;
                fitsInInteger = BigInteger.TryParse (
                    "0" + val,
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out valBig
                );

                val = valBig.ToString ();
            } else {
                long val64;
                fitsInInteger = long.TryParse (
                    val,
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out val64
                );

                val = val64.ToString ();
            }

            if (!fitsInInteger) {
                errorLog.Add (Errors.IntegerOverBounds, source.Location);
            }

            TokenClass tokenClass = isBigInt ?
                TokenClass.BigIntLiteral :
                TokenClass.IntLiteral;

            if (string.IsNullOrEmpty (val)) {
                errorLog.Add (Errors.IllegalSyntax, source.Location);
            }

            return new Token (tokenClass, val, lastDocStr, source.Location);
        }

        private static bool IsHexNumber (char c)
        {
            return "ABCDEFabcdef0123456789".Contains (c.ToString ());
        }

        private Token ReadFloat (StringBuilder buffer)
        {
            SourceLocation location = source.Location;

            source.Skip (); // .
            buffer.Append (".");
            char ch = source.Peek ();
            do {
                buffer.Append (source.Read ());
                ch = source.Peek ();
            } while (char.IsDigit (ch));

            return new Token (TokenClass.FloatLiteral, buffer.ToString (), lastDocStr, location);
        }

        private Token ReadStringLiteral (bool binary = false)
        {
            SourceLocation location = source.Location;

            StringBuilder accum = new StringBuilder ();

            char delimiter = source.Read ();

            char ch = source.Peek ();

            while (source.See () && ch != delimiter) {
                if (ch == '\\') {
                    source.Skip ();
                    accum.Append (ParseEscapeCode ());
                } else {
                    accum.Append (source.Read ());
                }
                ch = source.Peek ();
            }

            if (!source.See ()) {
                errorLog.Add (Errors.UnterminatedStringLiteral, location);
            }

            source.Skip ();

            if (binary) {
                return new Token (TokenClass.BinaryStringLiteral,
                    accum.ToString (),
                    lastDocStr,
                    location
                );
            }

            return new Token (ch == '"' ? 
                TokenClass.InterpolatedStringLiteral :
                TokenClass.StringLiteral,
                accum.ToString (),
                lastDocStr,
                location
            );
        }

        private char ParseEscapeCode ()
        {
            switch (source.Read ()) {
            case '\'':
                return '\'';
            case '"':
                return '"';
            case 'n':
                return '\n';
            case 'b':
                return '\b';
            case 'r':
                return '\r';
            case 't':
                return '\t';
            case 'f':
                return '\f';
            case '\\':
                return '\\';
            }

            errorLog.Add (Errors.UnrecognizedEscapeSequence, source.Location);

            return char.MinValue;
        }

        private Token ReadIdentifier ()
        {
            StringBuilder accum = new StringBuilder ();

            char ch = source.Peek ();

            while (char.IsLetterOrDigit (ch) || ch == '_') {
                accum.Append (source.Read ());
                ch = source.Peek ();
            }

            string identValue = accum.ToString ();

            string[] keywords = new string[] {
                "if", "else", "while", "do", "for", "func",
                "class", "use", "self", "foreach", "in",
                "true", "false", "null", "lambda", "try",
                "except", "break", "from", "continue", "super",
                "enum", "raise", "contract", "trait", "mixin", 
                "given", "case", "yield", "default", "return", 
                "match", "when", "var", "with", "global",
                "extend", "extends", "implements"
            };

            string[] operators = new string[] { "in", "is", "isnot", "as" };

            if (keywords.Contains (identValue)) {
                return new Token (TokenClass.Keyword, accum.ToString (), lastDocStr, source.Location);
            }

            if (operators.Contains (identValue)) {
                return new Token (TokenClass.Operator, accum.ToString (), lastDocStr, source.Location);
            }

            return new Token (TokenClass.Identifier, accum.ToString (), lastDocStr, source.Location);
        }

        private Token ReadOperator ()
        {
            var operators = new Dictionary<string, TokenClass> {
                { ">>", TokenClass.Operator },
                { "<<", TokenClass.Operator },
                { "&&", TokenClass.Operator },
                { "||", TokenClass.Operator },
                { "==", TokenClass.Operator },
                { "!=", TokenClass.Operator },
                { "=>", TokenClass.Operator },
                { "<=", TokenClass.Operator },
                { ">=", TokenClass.Operator },
                { "+=", TokenClass.Operator },
                { "-=", TokenClass.Operator },
                { "*=", TokenClass.Operator },
                { "/=", TokenClass.Operator },
                { "%=", TokenClass.Operator },
                { "^=", TokenClass.Operator },
                { "&=", TokenClass.Operator },
                { "|=", TokenClass.Operator },
                { "??", TokenClass.Operator },
                { "**", TokenClass.Operator },
                { "..", TokenClass.Operator },
                { "...", TokenClass.Operator },
                { "<<=", TokenClass.Operator },
                { ">>=", TokenClass.Operator },
                {".", TokenClass.MemberAccess },
                { ".?", TokenClass.MemberDefaultAccess },
            };

            string opStr;

            if (operators.ContainsKey (source.Peeks (3))) {
                opStr = source.Reads (3);

                return new Token (
                    operators [opStr],
                    opStr, lastDocStr,
                    source.Location
                );
            }

            if (operators.ContainsKey (source.Peeks (2))) {
                opStr = source.Reads (2);

                return new Token (
                    operators [opStr],
                    opStr, lastDocStr,
                    source.Location
                );
            }


            opStr = source.Reads (1);


            if (operators.ContainsKey (opStr)) {
                return new Token (
                    operators [opStr],
                    opStr, lastDocStr,
                    source.Location
                );
            }

            return new Token (
                TokenClass.Operator,
                opStr, lastDocStr,
                source.Location
            );
        }

        private void ReadLineComment ()
        {
            while (source.See ()) {
                if (source.Peeks (2) == "*/") {
                    source.Skip (2);
                    return;
                }
                source.Skip ();
            }

            errorLog.Add (Errors.UnexpectedEndOfFile, source.Location);
        }

        private void ReadDocComment ()
        {
            StringBuilder accum = new StringBuilder ();

            source.Skip (3);

            while (source.See ()) {
                if (source.Peeks (2) == "*/") {

                    source.Skip (2);

                    string doc = accum.ToString ();
                    var lines = doc.Split ('\n')
                        .Select (p => p.Trim ())
                        .Where (p => p.StartsWith ("*"))
                        .Select (p => p.Substring (p.IndexOf ('*') + 1).Trim ());
                    lastDocStr = String.Join ("\n", lines);
                    return;
                }

                accum.Append (source.Read ());
            }

            errorLog.Add (Errors.UnexpectedEndOfFile, source.Location);
        }
    }
}

