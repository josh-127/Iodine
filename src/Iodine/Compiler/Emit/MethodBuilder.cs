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
using System.Collections.Generic;
using Iodine.Runtime;
using Iodine.Util;

namespace Iodine.Compiler 
{
    public class MethodBuilder : IodineMethod
    {
        private static int nextLabelID = 0;
        private int nextTemporary = 2048;
        private MethodBuilder parent;
        private Dictionary<int, Label> labelReferences = new Dictionary<int, Label> ();
        protected List<Instruction> instructions = new List<Instruction> ();

        public MethodBuilder (IodineModule module,
            string name,
            bool isInstance,
            int parameterCount,
            bool isVariadic,
            bool acceptsKwargs) : base ()
        {
            Name = name;
            ParameterCount = parameterCount;
            Module = module;
            InstanceMethod = isInstance;
            Variadic = isVariadic;
            AcceptsKeywordArgs = acceptsKwargs;
            SetAttribute ("__module__", module);
        }

        public MethodBuilder (MethodBuilder parent,
            IodineModule module,
            string name,
            bool isInstance,
            int parameterCount,
            bool isVariadic,
            bool acceptsKwargs) : this (module, name, isInstance, parameterCount, isVariadic, acceptsKwargs)
        {
            this.parent = parent;
        }

        public void EmitInstruction (Opcode opcode)
        {
            instructions.Add (new Instruction (null, opcode));
        }

        public void EmitInstruction (Opcode opcode, int arg)
        {
            instructions.Add (new Instruction (null, opcode, arg));
        }

        public void EmitInstruction (Opcode opcode, Label label)
        {
            labelReferences [instructions.Count] = label;
            instructions.Add (new Instruction (null, opcode, 0));
        }

        public void EmitInstruction (SourceLocation loc, Opcode opcode)
        {
            instructions.Add (new Instruction (loc, opcode));
        }

        public void EmitInstruction (SourceLocation loc, Opcode opcode, int arg)
        {
            instructions.Add (new Instruction (loc, opcode, arg));
        }

        public void EmitInstruction (SourceLocation loc, Opcode opcode, Label label)
        {
            labelReferences [instructions.Count] = label;
            instructions.Add (new Instruction (loc, opcode, 0));
        }

        public int CreateTemporary ()
        {
            if (parent != null) {
                parent.CreateTemporary ();
            }
            return nextTemporary++;
        }

        public Label CreateLabel ()
        {
            return new Label (nextLabelID++);
        }

        public void MarkLabelPosition (Label label)
        {
            label._Position = instructions.Count;
        }

        public void FinalizeLabels ()
        {
            foreach (int position in labelReferences.Keys) {
                instructions [position] = new Instruction (instructions [position].Location,
                    instructions [position].OperationCode,
                    labelReferences [position]._Position
                );
            }
            Body = instructions.ToArray ();
        }
    }
}

