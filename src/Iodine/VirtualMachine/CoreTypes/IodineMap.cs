using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineMap : IodineObject
	{
		public Dictionary <int, IodineObject> Dict
		{
			private set;
			get;
		}

		public IodineMap ()
		{
			Dict = new Dictionary<int, IodineObject> ();
		}

		public override IodineObject GetIndex (IodineObject key)
		{
			return Dict[key.GetHashCode ()];
		}

		public override void SetIndex (IodineObject key, IodineObject value)
		{
			this.Dict[key.GetHashCode ()] = value;
		}

		private IodineObject getSize (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			return new IodineInteger (((IodineList)self).Objects.Count);
		}

		public override int GetHashCode ()
		{
			return Dict.GetHashCode ();
		}
	}
}

