using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineStack
	{
		private Stack<StackFrame> frames = new Stack<StackFrame>( );
		private StackFrame top = null;

		public IodineMethod CurrentMethod
		{
			get
			{
				return top.Method;
			}
		}

		public IodineModule CurrentModule
		{
			get
			{
				return top.Module;
			}
		}

		public IodineObject Self
		{
			get
			{
				return this.top.Self;
			}
		}

		public StackFrame Top
		{
			get
			{
				return top;
			}
		}

		public int Frames
		{
			private set;
			get;
		}

		public int InstructionPointer
		{
			get
			{
				return top.InstructionPointer;
			}
			set
			{
				top.InstructionPointer = value;
			}
		}

		public void NewFrame (StackFrame frame)
		{
			Frames++;
			top = frame;
			this.frames.Push (frame);
		}

		public void NewFrame (IodineMethod method, IodineObject self, int localCount)
		{
			Frames++;
			top = new StackFrame (method, top, self, localCount);
			this.frames.Push (top);
		}

		public void EndFrame ()
		{
			Frames--;
			this.frames.Pop ();
			if (this.frames.Count != 0) {
				top = this.frames.Peek ();
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
		}

		public IodineObject Pop ()
		{
			return top.Pop ();
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
		public int LocalCount
		{
			private set;
			get;
		}

		public bool AbortExecution
		{
			set;
			get;
		}

		public IodineMethod Method
		{
			private set;
			get;
		}

		public IodineModule Module
		{
			private set;
			get;
		}

		public IodineObject Self
		{
			private set;
			get;
		}

		public int InstructionPointer
		{
			get;
			set;
		}

		public StackFrame Parent
		{
			private set;
			get;
		}

		private Stack<IodineObject> stack = new Stack<IodineObject> ();
		private IodineObject[] locals;

		public StackFrame (IodineMethod method, StackFrame parent, IodineObject self, int localCount)
		{
			this.LocalCount = localCount;
			this.locals = new IodineObject[localCount];
			this.Method = method;
			this.Module = method.Module;
			this.Self = self;
			this.Parent = parent;
		}

		public void StoreLocal (int index, IodineObject obj)
		{
			this.locals[index] = obj;
		}

		public IodineObject LoadLocal (int index)
		{
			return this.locals[index];
		}

		public void Push (IodineObject obj)
		{
			this.stack.Push (obj);
		}

		public IodineObject Pop ()
		{
			return this.stack.Pop ();
		}

		public StackFrame Duplicate (StackFrame top)
		{
			StackFrame newStackFrame = new StackFrame (this.Method, top, this.Self, this.LocalCount);
			for (int i = 0; i < LocalCount; i++) {
				newStackFrame.StoreLocal (i, locals[i]);
			}
			return newStackFrame;
		}
	}
}

