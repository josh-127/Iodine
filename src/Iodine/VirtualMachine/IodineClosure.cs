using System;

namespace Iodine
{
	public class IodineClosure : IodineObject
	{
		private static readonly IodineTypeDefinition ClosureTypeDef = new IodineTypeDefinition ("Closure"); 
		private StackFrame frame;
		private IodineMethod target;

		public IodineClosure (StackFrame frame, IodineMethod target)
			: base (ClosureTypeDef)
		{
			this.frame = frame;
			this.target = target;
		}

		public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			return vm.InvokeMethod (target, frame.Duplicate (vm.Stack.Top), frame.Self, arguments);
		}
	}
}

