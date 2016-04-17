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
using Iodine.Compiler;

namespace Iodine.Runtime
{
    /// <summary>
    /// Represents a single frame (Activation record) for an Iodine method
    /// </summary>
    public class StackFrame
    {
        public readonly IodineMethod Method;
        public readonly IodineObject Self;
        public readonly IodineObject[] Arguments;
        public readonly LinkedStack<IodineObject> DisposableObjects = new LinkedStack<IodineObject> ();
        public readonly LinkedStack<IodineExceptionHandler> ExceptionHandlers = new LinkedStack<IodineExceptionHandler> ();

        public volatile bool AbortExecution = false;

        public bool Yielded { set; get; }

        public SourceLocation Location { set; get; }
        public SourceLocation CurrentLocation { set; get; }

        public int InstructionPointer { set; get; }

        public StackFrame Parent { private set; get; }

        public IodineModule Module {
            get { return Method.Module; }
        }

        private LinkedStack<IodineObject> stack = new LinkedStack<IodineObject> ();
        private Dictionary<int, IodineObject> locals;
        private Dictionary<int, IodineObject> parentLocals = null;

        public StackFrame (IodineMethod method,
            IodineObject[] arguments,
            StackFrame parent,
            IodineObject self)
        {
            locals = new Dictionary<int, IodineObject> ();
            parentLocals = locals;
            Method = method;
            Self = self;
            Arguments = arguments;
            Parent = parent;
        }

        public StackFrame (IodineMethod method,
            IodineObject[] arguments,
            StackFrame parent,
            IodineObject self,
            Dictionary<int, IodineObject> locals) : this (method, arguments, parent, self)
        {
            parentLocals = locals;
            this.locals = new Dictionary<int, IodineObject> ();

            foreach (int key in locals.Keys) {
                this.locals.Add (key, locals [key]);
            }
        }

        #if DOTNET_45
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		#endif
        internal void StoreLocal (int index, IodineObject obj)
        {
            if (parentLocals.ContainsKey (index)) {
                parentLocals [index] = obj;
            }
            locals [index] = obj;
        }

        #if DOTNET_45
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		#endif
        internal IodineObject LoadLocal (int index)
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
            Dictionary<int, IodineObject> oldLocals = new Dictionary<int, IodineObject> ();

            foreach (int key in locals.Keys) {
                oldLocals [key] = locals [key];
            }

            return new StackFrame (Method, Arguments, top, Self, locals);
        }
    }
}