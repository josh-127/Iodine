// /**
//   * Copyright (c) 2015, GruntTheDivine All rights reserved.
//
//   * Redistribution and use in source and binary forms, with or without modification,
//   * are permitted provided that the following conditions are met:
//   * 
//   *  * Redistributions of source code must retain the above copyright notice, this list
//   *    of conditions and the following disclaimer.
//   * 
//   *  * Redistributions in binary form must reproduce the above copyright notice, this
//   *    list of conditions and the following disclaimer in the documentation and/or
//   *    other materials provided with the distribution.
//
//   * Neither the name of the copyright holder nor the names of its contributors may be
//   * used to endorse or promote products derived from this software without specific
//   * prior written permission.
//   * 
//   * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
//   * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
//   * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
//   * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
//   * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
//   * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
//   * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
//   * CONTRACT ,STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
//   * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
//   * DAMAGE.
// /**
using System;

namespace Iodine.Runtime
{
    /// <summary>
    /// An IodineBound method represents an IodineMethod that has been "bound" to an
    /// object. When invoked, this will provide the "bound" method with the self 
    /// reference needed to access any instance types
    /// </summary>
    public class IodineBoundMethod : IodineObject
    {
        private static readonly IodineTypeDefinition InstanceTypeDef = new IodineTypeDefinition ("BoundMethod");

        /// <summary>
        /// The method that has been "bound" to a new a self reference
        /// </summary>
        public readonly IodineMethod Method;

        /// <summary>
        /// The self reference which will be provided to the "bound" method
        /// </summary>
        public IodineObject Self {
            private set;
            get;
        }

        public IodineBoundMethod (IodineObject self, IodineMethod method)
            : base (InstanceTypeDef)
        {
            Method = method;
            SetAttribute ("__doc__", method.Attributes ["__doc__"]);
            Self = self;
        }

        /// <summary>
        /// Rebinds this method with a new self reference
        /// </summary>
        /// <param name="newSelf">New self reference</param>
        public void Bind (IodineObject newSelf)
        {
            Self = newSelf;
        }

        public override bool IsCallable ()
        {
            return true;
        }

        public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
        {
            if (Method.Generator) {
                StackFrame frame = new StackFrame (Method, vm.Top.Arguments, vm.Top, Self);
                IodineObject initialValue = vm.InvokeMethod (Method, frame, Self, arguments);

                if (frame.Yielded) {
                    return new IodineGenerator (frame, this, arguments, initialValue);
                }
                return initialValue;
            }
            return vm.InvokeMethod (Method, Self, arguments);
        }

        public override string ToString ()
        {
            return string.Format ("<Bound {0}>", Method.Name);
        }
    }
}

