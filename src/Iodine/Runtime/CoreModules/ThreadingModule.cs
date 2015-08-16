using System;
using System.Threading;
using System.Security.Cryptography;

namespace Iodine
{
	[IodineBuiltinModule ("threading")]
	public class ThreadingModule : IodineModule
	{
		class IodineThread : IodineObject
		{
			public static readonly IodineTypeDefinition ThreadTypeDef = new IodineTypeDefinition ("Thread");

			public Thread Value {
				private set;
				get;
			}

			public IodineThread (Thread t)
				: base (ThreadTypeDef)
			{
				this.Value = t;
				this.SetAttribute ("start", new InternalMethodCallback (start, this));
			}

			private IodineObject start (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				Value.Start ();
				return null;
			}
		}

		public ThreadingModule ()
			: base ("threading")
		{
			this.SetAttribute ("Thread", new InternalMethodCallback (thread, this));
			this.SetAttribute ("sleep", new InternalMethodCallback (sleep, this));
		}

		private IodineObject thread (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineObject func = args [0];

			Thread t = new Thread (() => {
				VirtualMachine newVm = new VirtualMachine (vm.Globals);
				func.Invoke (newVm, new IodineObject[] { });
			});
			return new IodineThread (t);
		}

		private IodineObject sleep (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}
			IodineInteger time = args [0] as IodineInteger;
			System.Threading.Thread.Sleep ((int)time.Value);
			return null;
		}
	}
}

