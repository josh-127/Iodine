using System;
using System.Dynamic;

namespace Iodine
{
	public class IodineDynamicObject : DynamicObject
	{
		private IodineObject internalObject;
		private VirtualMachine internalVm;

		internal IodineDynamicObject (IodineObject obj, VirtualMachine vm)
		{
			this.internalObject = obj;
			this.internalVm = vm;
		}

		public override bool TryGetMember (GetMemberBinder binder, out object result)
		{
			if (internalObject.HasAttribute (binder.Name)) {
				IodineObject obj = internalObject.GetAttribute (binder.Name);
				if (!IodineTypeConverter.Instance.ConvertToPrimative (obj, out result)) {
					result = new IodineDynamicObject (obj, internalVm);
				}
				return true;
			}
			result = null;
			return true;
		}

		public override bool TrySetMember (SetMemberBinder binder, object value)
		{
			IodineObject val = null;
			if (!IodineTypeConverter.Instance.ConvertFromPrimative (value, out val)) {
				if (value is IodineObject) {
					val = (IodineObject)value;
				} else {
					return false;
				}
			}
			internalObject.SetAttribute (binder.Name, val);
			return true;
		}

		public override bool TryInvoke (InvokeBinder binder, object[] args, out object result)
		{
			IodineObject[] arguments = new IodineObject[args.Length];
			for (int i = 0; i < args.Length; i++) {
				IodineObject val = null;
				if (!IodineTypeConverter.Instance.ConvertFromPrimative (args [i], out val)) {
					if (args [i] is IodineObject) {
						val = (IodineObject)args [i];
					} else {
						result = null;
						return false;
					}
				}
				arguments [i] = val;
			}
			IodineObject returnVal = internalObject.Invoke (internalVm, arguments);
			if (!IodineTypeConverter.Instance.ConvertToPrimative (returnVal, out result)) {
				result = new IodineDynamicObject (returnVal, internalVm);
			}
			return true;
		}
	}
}

