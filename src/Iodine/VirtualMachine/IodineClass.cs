using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineClass : IodineTypeDefinition
	{
		private IodineMethod constructor;
		private IList<IodineMethod> instanceMethods = new List<IodineMethod> ();

		public IodineClass (string name, IodineMethod constructor)
			: base (name)
		{
			this.constructor = constructor;
		}

		public void AddInstanceMethod (IodineMethod method)
		{
			instanceMethods.Add (method);
		}

		public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			IodineObject obj = new IodineObject (this);
			foreach (IodineMethod method in this.instanceMethods) {
				obj.SetAttribute (method.Name, method);
			}
			vm.InvokeMethod (constructor, obj, arguments);

			return obj;
		}
	}
}

