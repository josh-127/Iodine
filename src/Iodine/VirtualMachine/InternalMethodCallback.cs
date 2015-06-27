using System;

namespace Iodine
{
	public delegate IodineObject IodineMethodCallback (VirtualMachine vm, IodineObject self,
		IodineObject[] arguments);

	public class InternalMethodCallback : IodineObject
	{
		private IodineObject self;
		private IodineMethodCallback callback;

		public InternalMethodCallback (IodineMethodCallback callback, IodineObject self)
		{
			this.self = self;
			this.callback = callback;
		}

		public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			return callback.Invoke (vm, self, arguments);
		}
	}
}

