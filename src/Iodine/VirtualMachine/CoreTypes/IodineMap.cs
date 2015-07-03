using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineMap : IodineObject
	{
		private static readonly IodineTypeDefinition MapTypeDef = new IodineTypeDefinition ("HashMap"); 

		private int iterIndex = 0;

		public Dictionary <int, IodineObject> Dict {
			private set;
			get;
		}

		private List <IodineObject> keys = new List<IodineObject>();

		public IodineMap ()
			: base (MapTypeDef)
		{
			Dict = new Dictionary<int, IodineObject> ();
			this.SetAttribute ("contains", new InternalMethodCallback (contains, this));
			this.SetAttribute ("getSize", new InternalMethodCallback (getSize, this));
			this.SetAttribute ("clear", new InternalMethodCallback (clear, this));
			this.SetAttribute ("set", new InternalMethodCallback (set, this));
			this.SetAttribute ("remove", new InternalMethodCallback (remove, this));
		}

		public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
		{
			int hash = key.GetHashCode ();
			if (!Dict.ContainsKey (hash)) {
				vm.RaiseException (new IodineKeyNotFound ());
				return null;
			}
			return Dict[key.GetHashCode ()];
		}

		public override void SetIndex (VirtualMachine vm, IodineObject key, IodineObject value)
		{
			this.Dict[key.GetHashCode ()] = value;
			this.keys.Add (key);
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

		public void Set (IodineObject key, IodineObject val)
		{
			this.Dict[key.GetHashCode ()] = val;
			this.keys.Add (key);
		}

		public IodineObject Get (IodineObject key)
		{
			return this.Dict[key.GetHashCode ()];
		}

		private IodineObject contains (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			return new IodineBool (Dict.ContainsKey (args[0].GetHashCode ()));
		}

		private IodineObject getSize (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			return new IodineInteger (this.Dict.Count);
		}

		private IodineObject clear (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			this.Dict.Clear ();
			this.keys.Clear ();
			return null;
		}

		private IodineObject set (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			if (arguments.Length >= 2) {
				IodineObject key = arguments[0];
				IodineObject val = arguments[1];
				this.Dict[key.GetHashCode ()] = val;
				this.keys.Add (key);
				return null;
			}
			vm.RaiseException (new IodineArgumentException (2));
			return null;
		}

		private IodineObject remove (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			if (arguments.Length >= 1) {
				IodineObject key = arguments[0];
				int hash = key.GetHashCode ();
				if (!Dict.ContainsKey (hash)) {
					vm.RaiseException (new IodineKeyNotFound ());
					return null;
				}
				this.Dict.Remove (hash);
				return null;
			}
			vm.RaiseException (new IodineArgumentException (2));
			return null;
		}
	}
}

