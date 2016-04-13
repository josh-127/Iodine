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
	// TODO: This file needs to be reorganized, its a mess. 

	public class IodineLabel
	{
		public int _Position;
		public int _LabelID;

		public IodineLabel (int labelID)
		{
			_LabelID = labelID;
			_Position = 0;
		}
	}

	public class IodineBoundMethod : IodineObject
	{
		private static readonly IodineTypeDefinition InstanceTypeDef = new IodineTypeDefinition ("BoundMethod");

		public IodineMethod Method { private set; get; }

		public IodineObject Self { private set; get; }

		public IodineBoundMethod (IodineObject self, IodineMethod method)
			: base (InstanceTypeDef)
		{
			Method = method;
			Self = self;
		}

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
				StackFrame frame = new StackFrame (Method, vm.Top.Arguments, vm.Top, Self, Method.LocalCount);
				IodineObject initialValue = vm.InvokeMethod (Method, frame, Self, arguments);

				if (frame.Yielded) {
					return new IodineGenerator (frame, this, arguments, initialValue);
				}
				return initialValue;
			}
			return vm.InvokeMethod (Method, Self, arguments);
		}
	}

	// TODO: Abtract bytecode implementation away from IodineMethod
	public abstract class IodineMethod : IodineObject
	{
		private static readonly IodineTypeDefinition MethodTypeDef = new IodineTypeDefinition ("Method");

		public Instruction[] Body {
			get;
			internal set;
		}

		public string Name {
			get;
			protected set;
		}

		public int ParameterCount {
			get;
			protected set;
		}

		public int LocalCount {
			get;
			protected set;
		}

		public bool Variadic {
			get;
			protected set;
		}

		public bool AcceptsKeywordArgs {
			get;
			protected set;
		}

		public bool Generator {
			get;
			set;
		}

		public IodineModule Module {
			get;
			protected set;
		}

		public bool InstanceMethod {
			get;
			protected set;
		}

		public readonly Dictionary<string, int> Parameters = new Dictionary<string, int> ();

		public IodineMethod ()
			: base (MethodTypeDef)
		{
		}

		public override bool IsCallable ()
		{
			return true;
		}

		public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			if (Generator) {
				StackFrame frame = new StackFrame (this, vm.Top.Arguments, vm.Top, null, LocalCount);
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
			return string.Format ("<Function {0}>", Name);
		}
	}
}
