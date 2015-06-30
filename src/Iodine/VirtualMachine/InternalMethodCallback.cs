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
		public IodineMethodCallback Callback
		{
			private set;
			get;
		}

		public InternalMethodCallback (IodineMethodCallback callback, IodineObject self) 
			: base (InternalMethodTypeDef)
		{
			this.self = self;
			this.Callback = callback;
		}

		public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			//vm.Stack.NewFrame (new NativeStackFrame (this, vm.Stack.Top));
			try
			{
				IodineObject obj = Callback.Invoke (vm, self, arguments);
				//vm.Stack.EndFrame ();
				return obj;
			}
			catch (UnhandledIodineExceptionException e)
			{
				throw e;
			}
			catch (Exception ex)
			{
				vm.RaiseException (new IodineInternalErrorException (ex));
			}
			return null;
			
		}
	}
}

