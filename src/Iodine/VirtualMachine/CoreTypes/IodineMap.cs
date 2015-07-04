using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineMap : IodineObject
	{
		public static readonly IodineTypeDefinition TypeDefinition = new MapTypeDef ();

		class MapTypeDef : IodineTypeDefinition
		{
			public MapTypeDef () 
				: base ("HashMap")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				if (args.Length >= 1) {
					IodineList inputList = args[0] as IodineList;
					IodineMap ret = new IodineMap ();
					if (inputList != null) {
						foreach (IodineObject item in inputList.Objects) {
							IodineTuple kv = item as IodineTuple;
							if (kv != null) {
								ret.Dict.Add (kv.Objects[0].GetHashCode (), kv.Objects[1]);
							}
						}
					} 
					return ret;
				}
				return new IodineMap ();
			}
		}

		private int iterIndex = 0;

		public Dictionary <int, IodineObject> Dict {
			private set;
			get;
		}

		public Dictionary <int, IodineObject> Keys {
			private set;
			get;
		}

		public IodineMap ()
			: base (TypeDefinition)
		{
			Dict = new Dictionary<int, IodineObject> ();
			Keys = new Dictionary<int, IodineObject> ();
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
			this.Keys[key.GetHashCode ()] = key;
		}

		public override int GetHashCode ()
		{
			return Dict.GetHashCode ();
		}

		public override IodineObject IterGetNext (VirtualMachine vm)
		{
			IodineObject[] Keys = new IodineObject[this.Keys.Count];
			this.Keys.Values.CopyTo (Keys, 0);
			return Keys[this.iterIndex - 1];
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
			this.Keys[key.GetHashCode ()] = key;
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
			this.Keys.Clear ();
			return null;
		}

		private IodineObject set (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			if (arguments.Length >= 2) {
				IodineObject key = arguments[0];
				IodineObject val = arguments[1];
				this.Dict[key.GetHashCode ()] = val;
				this.Keys[key.GetHashCode ()] = key;
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
				this.Keys.Remove (hash);
				this.Dict.Remove (hash);
				return null;
			}
			vm.RaiseException (new IodineArgumentException (2));
			return null;
		}
	}
}

