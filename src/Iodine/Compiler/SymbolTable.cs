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
using System.Collections.Generic;

namespace Iodine.Compiler
{
    /// <summary>
    /// Symbol table.
    /// </summary>
    public class SymbolTable
    {
        class Scope
        {
            private Dictionary<string, int> symbols = new Dictionary<string, int> ();

            public int GetSymbol (string name)
            {
                return symbols [name];
            }

            public bool FindSymbol (string name)
            {
                return symbols.ContainsKey (name);
            }

            public void AddSymbol (string name, int index)
            {
                symbols [name] = index;
            }
        }

        private Scope globalScope = new Scope ();
        private Stack<Scope> scopes = new Stack<Scope> ();
        private int nextIndex = 0;

        public bool IsInGlobalScope {
            get {
                return scopes.Peek () == globalScope;
            }
        }

        public SymbolTable ()
        {
            scopes.Push (globalScope);
        }

        public void EnterScope ()
        {
            scopes.Push (new Scope ());
        }

        public void ExitScope ()
        {
            scopes.Pop ();

            if (scopes.Count == 1) {
                nextIndex = 0;
            }
        }

        public bool IsGlobal (string name)
        {
            foreach (Scope scope in scopes) {
                if (scope.FindSymbol (name) && scope != globalScope) {
                    return false;
                }
            }
            return true;
        }

        public int GetSymbolIndex (string name)
        {
            foreach (Scope scope in scopes) {
                if (scope.FindSymbol (name)) {
                    return scope.GetSymbol (name);
                }
            }
            return -1;
        }

        public bool IsSymbolDefined (string name)
        {
            foreach (Scope scope in scopes) {
                if (scope.FindSymbol (name)) {
                    return true;
                }
            }
            return false;
        }

        public int AddSymbol (string name)
        {
            scopes.Peek ().AddSymbol (name, nextIndex);
            nextIndex++;
            return nextIndex - 1;
        }
    }
}

