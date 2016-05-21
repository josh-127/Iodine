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
using System.Collections.Generic;
using Iodine.Compiler;

namespace Iodine.Runtime
{
    /// <summary>
    /// Abstract class representing an IodineMethod containing Iodine bytecode. This 
    /// is the only class that is directly invokable by the virtual machine
    /// </summary>
    public abstract class IodineMethod : IodineObject
    {
        private static readonly IodineTypeDefinition MethodTypeDef = new IodineTypeDefinition ("Method");

        /// <summary>
        /// Gets the bytecode which defines this method
        /// </summary>
        /// <value>The body.</value>
        public Instruction[] Body {
            get;
            internal set;
        }

        private string name;

        /// <summary>
        /// The name of the method
        /// </summary>
        /// <value>The name.</value>
        public string Name {
            get {
                return name;
            }
            protected set {
                name = value;
                SetAttribute ("__name__", new IodineString (value));
            }
        }

        /// <summary>
        /// How many parameters the method can receive
        /// </summary>
        /// <value>The parameter count.</value>
        public int ParameterCount {
            get;
            protected set;
        }

        /// <summary>
        /// Does this method accept variable arguments?
        /// </summary>
        /// <value><c>true</c> if variadic; otherwise, <c>false</c>.</value>
        public bool Variadic {
            get;
            protected set;
        }

        /// <summary>
        /// Does this method accept keyword arguments?
        /// </summary>
        /// <value><c>true</c> if accepts keyword arguments; otherwise, <c>false</c>.</value>
        public bool AcceptsKeywordArgs {
            get;
            protected set;
        }

        /// <summary>
        /// Does this method have a chance of yielding to the caller?
        /// </summary>
        /// <value><c>true</c> if generator; otherwise, <c>false</c>.</value>
        public bool Generator {
            get;
            set;
        }

        /// <summary>
        /// Module in which this method was defined in
        /// </summary>
        /// <value>The module.</value>
        public IodineModule Module {
            get;
            protected set;
        }

        /// <summary>
        /// Is this an instance method
        /// </summary>
        /// <value><c>true</c> if instance method; otherwise, <c>false</c>.</value>
        public bool InstanceMethod {
            get;
            protected set;
        }

        /// <summary>
        /// Maps each parameter to a local variable index, used by the virtual machine
        /// </summary>
        public readonly Dictionary<string, int> Parameters = new Dictionary<string, int> ();

        protected IodineMethod ()
            : base (MethodTypeDef)
        {
            SetAttribute ("__doc__", IodineString.Empty);
            SetAttribute ("__invoke__", new BuiltinMethodCallback (invoke, this));
        }

        /// <summary>
        /// A small wrapper around IodineObject.Invoke
        /// </summary>
        /// <param name="vm">Vm.</param>
        /// <param name="self">Self.</param>
        /// <param name="args">Arguments.</param>
        IodineObject invoke (VirtualMachine vm, IodineObject self, IodineObject [] args)
        {
            return Invoke (vm, args);
        }

        public override bool IsCallable ()
        {
            return true;
        }

        /// <summary>
        /// Invoke the specified vm and arguments.
        /// </summary>
        /// <param name="vm">Vm.</param>
        /// <param name="arguments">Arguments.</param>
        public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
        {
            if (Generator) {
                /*
                 * If this method happens to be a generator method (Which just means it has
                 * a yield statement in it), we will attempt to invoke it in the VM and check
                 * if the method yielded or not. If the method did yield, we must return a 
                 * generator so the caller can iterate over any other items which this method
                 * may yield. If the method did not yield, we just return the original value
                 * returned
                 */
                StackFrame frame = new StackFrame (this, vm.Top.Arguments, vm.Top, null);
                IodineObject initialValue = vm.InvokeMethod (this, frame, null, arguments);

                if (frame.Yielded) {
                    return new IodineGenerator (frame, this, arguments, initialValue);
                }
                return initialValue;
            }
            return vm.InvokeMethod (this, null, arguments);
        }

        public override string ToString ()
        {
            return string.Format ("<Function {0}>", name);
        }
    }
}
