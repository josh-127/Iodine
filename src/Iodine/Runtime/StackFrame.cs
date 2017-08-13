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
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Iodine.Util;
using Iodine.Compiler;

namespace Iodine.Runtime
{
    /// <summary>
    /// Represents a single frame (Activation record) for an Iodine method
    /// </summary>
    public class StackFrame
    {
        /// <summary>
        /// Iodine Method which this stack frame is for
        /// </summary>
        public readonly IodineMethod Method;

        /// <summary>
        /// Self reference ('this' pointer)
        /// </summary>
        public readonly IodineObject Self;

        /// <summary>
        /// The arguments passed to the function
        /// </summary>
        public readonly IodineObject[] Arguments;

        /// <summary>
        /// These are objects which were used with the 'with' statement
        /// </summary>
        public readonly LinkedStack<IodineObject> DisposableObjects = new LinkedStack<IodineObject> ();

        /// <summary>
        /// Exception handlers
        /// </summary>
        public readonly LinkedStack<IodineExceptionHandler> ExceptionHandlers = new LinkedStack<IodineExceptionHandler> ();

        /// <summary>
        /// The stack frame of the parent function
        /// </summary>
        public readonly StackFrame Parent;

        /// <summary>
        /// This flag controls whether or not the VM should break from execution
        /// </summary>
        public volatile bool AbortExecution = false;

        /// <summary>
        /// Gets or sets a bool that indicates whether this function is to yield to the caller
        /// </summary>
        /// <value><c>true</c> if yielded; otherwise, <c>false</c>.</value>
        public bool Yielded { set; get; }

        /// <summary>
        /// Gets or sets the source for this stack frame location.
        /// </summary>
        /// <value>The location.</value>
        public SourceLocation Location { set; get; }

        /// <summary>
        /// Gets or sets the instruction pointer.
        /// </summary>
        /// <value>The instruction pointer.</value>
        public int InstructionPointer { set; get; }

        /// <summary>
        /// Gets the module.
        /// </summary>
        /// <value>The module.</value>
        public IodineModule Module {
            get;
            set;
        }

        public AttributeDictionary Locals {
            get {
                return locals;
            }
        }

        private LinkedStack<IodineObject> stack = new LinkedStack<IodineObject> ();
        private AttributeDictionary locals;
        private AttributeDictionary arguments = new AttributeDictionary ();
        private AttributeDictionary parentLocals = null;

        public StackFrame (
            IodineModule module,
            IodineMethod method,
            IodineObject[] arguments,
            StackFrame parent,
            IodineObject self)
        {
            locals = new AttributeDictionary ();
            parentLocals = locals;
            Method = method;
            Module = module;
            Self = self;
            Arguments = arguments;
            Parent = parent;
        }

        public StackFrame (
            IodineModule module,
            IodineMethod method,
            IodineObject[] arguments,
            StackFrame parent,
            IodineObject self,
            AttributeDictionary locals) : this (module, method, arguments, parent, self)
        {
            this.locals = locals;
        }

        private StackFrame (
            IodineModule module,
            IodineMethod method,
            IodineObject[] arguments,
            StackFrame parent,
            IodineObject self,
            AttributeDictionary locals,
            AttributeDictionary parentLocals) : this (module, method, arguments, parent, self)
        {
            this.parentLocals = parentLocals;
            this.locals = locals;
        }

        #if DOTNET_45
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        #endif
        internal void StoreLocalExplicit (string index, IodineObject obj)
        {
            arguments [index] = obj;
            locals [index] = obj;
        }

        #if DOTNET_45
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        #endif
        internal void StoreLocal (string index, IodineObject obj)
        {
            if (parentLocals.ContainsKey (index)) {
                parentLocals [index] = obj;
            }
            locals [index] = obj;
        }

        #if DOTNET_45
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        #endif
        internal IodineObject LoadLocal (string index)
        {
            return locals [index];
        }

        #if DOTNET_45
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        #endif
        internal void Push (IodineObject obj)
        {
            stack.Push (obj ?? IodineNull.Instance);
        }

        #if DOTNET_45
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        #endif
        internal IodineObject Pop ()
        {
            return stack.Pop ();
        }

        #if DOTNET_45
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        #endif
        internal StackFrame Duplicate (StackFrame top)
        {
            var oldLocals = new AttributeDictionary ();

            foreach (KeyValuePair<string, IodineObject> kv in locals) {
                oldLocals.Add (kv.Key, kv.Value);
            }

            return new StackFrame (Module, Method, Arguments, top, Self, oldLocals, locals);
        }
    }
}