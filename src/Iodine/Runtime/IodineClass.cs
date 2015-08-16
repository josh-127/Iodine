using System;
using System.Collections.Generic;

namespace Iodine.Runtime
{
	public class IodineClass : IodineTypeDefinition
	{
		public IodineMethod Constructor {
			private set;
			get;
		}

		public IList<IodineMethod> InstanceMethods {
			private set;
			get;
		}

		public IodineClass (string name, IodineMethod constructor)
			: base (name)
		{
			this.Constructor = constructor;
			this.InstanceMethods = new List<IodineMethod> ();
		}

		public void AddInstanceMethod (IodineMethod method)
		{
			InstanceMethods.Add (method);
		}

		public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			IodineObject obj = new IodineObject (this);
			foreach (IodineMethod method in this.InstanceMethods) {
				obj.SetAttribute (method.Name, method);
			}
			vm.InvokeMethod (Constructor, obj, arguments);

			return obj;
		}

		public override void Inherit (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			IodineObject obj = new IodineObject (this);
			foreach (IodineMethod method in this.InstanceMethods) {
				if (!self.HasAttribute (method.Name))
					self.SetAttribute (method.Name, method);
				obj.SetAttribute (method.Name, method);
			}
			vm.InvokeMethod (Constructor, self, arguments);
			self.SetAttribute ("__super__", obj);
			self.Base = obj;
		}
	}
}

