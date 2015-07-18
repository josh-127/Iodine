using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineInterface : IodineTypeDefinition
	{
		public IodineMethod Constructor {
			private set;
			get;
		}

		public IList<IodineMethod> RequiredMethods {
			private set;
			get;
		}

		public IodineInterface (string name)
			: base (name)
		{
			this.RequiredMethods = new List<IodineMethod> ();
		}

		public void AddMethod (IodineMethod method)
		{
			RequiredMethods.Add (method);
		}

		public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			return null;
		}

		public override void Inherit (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			foreach (IodineMethod method in RequiredMethods) {
				if (!self.HasAttribute (method.Name)) {
					vm.RaiseException (new IodineNotSupportedException ());
					return;
				}
			}
			self.Interfaces.Add (this);
		}
	}
}

