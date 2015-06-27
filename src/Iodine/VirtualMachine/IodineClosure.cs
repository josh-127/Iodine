using System;

namespace Iodine
{
	public class IodineClosure : IodineObject
	{
		private StackFrame frame;
		private IodineMethod target;

		public IodineClosure (StackFrame frame, IodineMethod target)
		{
			this.frame = frame;
			this.target = target;
		}

		public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			return vm.InvokeMethod (target, frame.Duplicate (), frame.Self, arguments);
		}
	}
}

