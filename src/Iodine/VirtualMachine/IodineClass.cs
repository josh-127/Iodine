using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineClass : IodineObject
	{
		private IodineMethod constructor;
		private IList<IodineMethod> instanceMethods = new List<IodineMethod> ();

		public string Name
		{
			private set;
			get;
		}

		public IodineClass (string name, IodineMethod constructor)
		{
			this.constructor = constructor;
			this.Name = name;
		}

		public void AddInstanceMethod (IodineMethod method)
		{
			instanceMethods.Add (method);
		}

		public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			IodineObject obj = new IodineObject ();
			foreach (IodineMethod method in this.instanceMethods) {
				obj.SetAttribute (method.Name, method);
			}
			vm.InvokeMethod (constructor, obj, arguments);
			return obj;
		}
	}
}

