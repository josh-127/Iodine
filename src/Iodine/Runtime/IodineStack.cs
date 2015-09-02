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

namespace Iodine.Runtime
{
	public class IodineStack
	{
		private LinkedStack<StackFrame> frames = new LinkedStack<StackFrame> ();
		private StackFrame top = null;

		public IodineObject Last {
			private set;
			get;
		}

		public StackFrame Top {
			get {
				return top;
			}
		}

		public int Frames {
			private set;
			get;
		}

		public void NewFrame (StackFrame frame)
		{
			Frames++;
			top = frame;
			frames.Push (frame);
		}

		public void NewFrame (IodineMethod method, IodineObject self, int localCount)
		{
			Frames++;
			top = new StackFrame (method, top, self, localCount);
			frames.Push (top);
		}

		public void EndFrame ()
		{
			Frames--;
			frames.Pop ();
			if (frames.Count != 0) {
				top = frames.Peek ();
			} else {
				top = null;
			}
		}

		public void StoreLocal (int index, IodineObject obj)
		{
			top.StoreLocal (index, obj);
		}

		public IodineObject LoadLocal (int index)
		{
			return top.LoadLocal (index);
		}

		public void Push (IodineObject obj)
		{
			top.Push (obj);
			Last = obj;
		}

		public IodineObject Pop ()
		{
			return top.Pop ();
		}

		public string Trace ()
		{
			StringBuilder accum = new StringBuilder ();
			StackFrame top = this.top;
			while (top != null) {
				if (top is NativeStackFrame) {
					NativeStackFrame frame = top as NativeStackFrame;

					accum.AppendFormat (" at {0} <internal method>\n", frame.NativeMethod.Callback.Method.Name);
				} else {
					accum.AppendFormat (" at {0} (Module: {1}, Line: {2})\n", top.Method.Name, top.Module.Name,
						top.Location.Line + 1);
				}
				top = top.Parent;
			}

			return accum.ToString ();
		}

		public void Unwind (int frames)
		{
			for (int i = 0; i < frames; i++) {
				StackFrame frame = this.frames.Pop ();
				frame.AbortExecution = true;
			}
			Frames -= frames;
			this.top = this.frames.Peek ();
		}
	}

	public class StackFrame
	{
		public readonly LinkedStack<IodineObject> DisposableObjects = new LinkedStack<IodineObject> ();

		public int LocalCount {
			private set;
			get;
		}

		public bool AbortExecution {
			set;
			get;
		}

		public bool Yielded {
			set;
			get;
		}

		public IodineMethod Method {
			private set;
			get;
		}

		public IodineModule Module {
			get {
				return Method.Module;
			}
		}

		public IodineObject Self {
			private set;
			get;
		}

		public Location Location {
			set;
			get;
		}

		public int InstructionPointer {
			get;
			set;
		}

		public StackFrame Parent {
			private set;
			get;
		}

		private LinkedStack<IodineObject> stack = new LinkedStack<IodineObject> ();
		private IodineObject[] locals;
		private IodineObject[] parentLocals = null;

		public StackFrame (IodineMethod method, StackFrame parent, IodineObject self, int localCount)
		{
			LocalCount = localCount;
			locals = new IodineObject[localCount];
			parentLocals = locals;
			Method = method;
			Self = self;
			Parent = parent;
		}

		public StackFrame (IodineMethod method, StackFrame parent, IodineObject self, int localCount,
		                   IodineObject[] locals) : this (method, parent, self, localCount)
		{
			parentLocals = locals;
			this.locals = new IodineObject[localCount];
			for (int i = 0; i < localCount; i++) {
				this.locals [i] = locals [i]; 
			}
		}

		public void StoreLocal (int index, IodineObject obj)
		{
			if (parentLocals [index] != null) {
				parentLocals [index] = obj;
			}
			locals [index] = obj;
		}

		public IodineObject LoadLocal (int index)
		{
			return locals [index];
		}

		public void Push (IodineObject obj)
		{
			if (obj != null)
				stack.Push (obj);
			else
				stack.Push (IodineNull.Instance);
		}

		public IodineObject Pop ()
		{
			return stack.Pop ();
		}

		public StackFrame Duplicate (StackFrame top, int localCount)
		{
			if (localCount > LocalCount) {
				IodineObject[] oldLocals = locals;
				locals = new IodineObject[localCount];
				Array.Copy (oldLocals, locals, oldLocals.Length);
			}
			return new StackFrame (Method, top, Self, Math.Max (LocalCount, localCount), locals);
		}
	}

	public class NativeStackFrame : StackFrame
	{
		public InternalMethodCallback NativeMethod {
			private set;
			get;
		}

		public NativeStackFrame (InternalMethodCallback method, StackFrame parent)
			: base (null, parent, null, 0)
		{
			this.NativeMethod = method;
		}
	}
}