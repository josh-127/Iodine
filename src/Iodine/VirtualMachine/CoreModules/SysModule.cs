using System;
using System.IO;
using System.Reflection;

namespace Iodine
{
	public class SysModule : IodineModule
	{
		public SysModule ()
			: base ("sys")
		{
			this.SetAttribute ("executable", new IodineString (Assembly.GetExecutingAssembly ().Location));

			this.SetAttribute ("path", new IodineList (IodineModule.SearchPaths));
			this.SetAttribute ("exit", new InternalMethodCallback (exit, this));
		}

		private IodineObject exit (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineInteger code = args[0] as IodineInteger;

			if (code == null) {
				vm.RaiseException (new IodineTypeException ("Int"));
				return null;
			}

			Environment.Exit ((int)code.Value);
			return null;
		}
	}
}

