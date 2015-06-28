using System;

namespace Iodine
{
	public delegate IodineObject IodineMethodCallback (VirtualMachine vm, IodineObject self,
		IodineObject[] arguments);

	public class InternalMethodCallback : IodineObject
	{
		private static readonly IodineTypeDefinition InternalMethodTypeDef =
			new IodineTypeDefinition ("InternalMethod"); 
		
		private IodineObject self;
		private IodineMethodCallback callback;

		public InternalMethodCallback (IodineMethodCallback callback, IodineObject self) 
			: base (InternalMethodTypeDef)
		{
			this.self = self;
			this.callback = callback;
		}

		public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			//vm.Stack.NewFrame (new NativeStackFrame (this, vm.Stack.Top));
			IodineObject ret = callback.Invoke (vm, self, arguments);
			//vm.Stack.EndFrame ();
			return ret;
		}
	}
}

