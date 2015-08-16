using System;

namespace Iodine.Runtime
{
	public class IodineExceptionHandler
	{
		public int Frame {
			private set;
			get;
		}

		public int InstructionPointer {
			private set;
			get;
		}

		public IodineExceptionHandler (int frame, int ip)
		{
			this.Frame = frame;
			this.InstructionPointer = ip;
		}
	}
}

