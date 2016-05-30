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
using System.Reflection;
using System.Collections.Generic;
using Iodine.Compiler;
using Iodine.Compiler.Ast;
using Iodine.Util;

namespace Iodine.Runtime
{
    public abstract class IodineModule : IodineObject
    {
        private static readonly IodineTypeDefinition ModuleTypeDef = new IodineTypeDefinition ("Module");

        public readonly string Name;

        public bool ExistsInGlobalNamespace {
            protected set;
            get;
        }

        /*
		 * Anonymous modules will use the global dictionary for storage rather than the modules own
         * dictionary. This is *kind of* a hack, but this functionality is *sort of* necessary for 
         * Iodine REPLs to work properly
         */
        public bool IsAnonymous {
            set;
            get;
        }

        internal virtual IList<IodineObject> ConstantPool {
            get {
                return this.constantPool;
            }
        }
            
        private CodeObject initializer;

        private List<IodineObject> constantPool = new List<IodineObject> ();

        public CodeObject Initializer {
            protected set {
                initializer = value;
                SetAttribute ("__init__", value);
            }
            get {
                return initializer; 
            }
        }

        public IodineModule (string name)
            : base (ModuleTypeDef)
        {
            Name = name;

            SetAttribute ("__doc__", IodineString.Empty);
            Attributes ["__name__"] = new IodineString (name);
        }

        public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
        {
            vm.NewFrame (new StackFrame (this, null, new IodineObject[] { }, null, null, Attributes));
            IodineObject retObj = vm.EvalCode (Initializer);
            vm.EndFrame ();
            return retObj;
        }

        public override string ToString ()
        {
            return string.Format ("<Module {0}>", Name);
        }
    }
}