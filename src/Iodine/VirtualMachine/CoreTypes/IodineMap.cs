using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineMap : IodineObject
	{
		private static readonly IodineTypeDefinition MapTypeDef = new IodineTypeDefinition ("HashMap"); 

		private int iterIndex = 0;

		public Dictionary <int, IodineObject> Dict
		{
			private set;
			get;
		}

		private List <IodineObject> keys = new List<IodineObject>();

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
			this.keys.Add (key);
		}

		private IodineObject getSize (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			return new IodineInteger (((IodineList)self).Objects.Count);
		}

		public override int GetHashCode ()
		{
			return Dict.GetHashCode ();
		}

		public override IodineObject IterGetNext (VirtualMachine vm)
		{
			return this.keys[this.iterIndex - 1];
		}

		public override bool IterMoveNext (VirtualMachine vm)
		{
			if (this.iterIndex >= this.Dict.Keys.Count)
				return false;
			this.iterIndex++;
			return true;
		}

		public override void IterReset (VirtualMachine vm)
		{
			this.iterIndex = 0;
		}

		private IodineObject contains (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			return new IodineBool (Dict.ContainsKey (args[0].GetHashCode ()));
		}
	}
}

