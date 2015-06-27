using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineMap : IodineObject
	{
		private static readonly IodineTypeDefinition MapTypeDef = new IodineTypeDefinition ("HashMap"); 

		public Dictionary <int, IodineObject> Dict
		{
			private set;
			get;
		}

		public IodineMap ()
			: base (MapTypeDef)
		{
			Dict = new Dictionary<int, IodineObject> ();
			this.SetAttribute ("contains", new InternalMethodCallback (contains, this));
		}

		public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
		{
			return Dict[key.GetHashCode ()];
		}

		public override void SetIndex (VirtualMachine vm, IodineObject key, IodineObject value)
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

		private IodineObject contains (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineBool (Dict.ContainsKey (args[0].GetHashCode ()));
		}
	}
}

