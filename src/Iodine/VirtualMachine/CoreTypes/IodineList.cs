using System;
using System.Text;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineList : IodineObject
	{
		public static readonly IodineTypeDefinition TypeDefinition = new ListTypeDef ();

		class ListTypeDef : IodineTypeDefinition
		{
			public ListTypeDef () 
				: base ("List")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				return new IodineList (args);
			}
		}

		private int iterIndex = 0;
		public List<IodineObject> Objects {
			private set;
			get;
		}


		public IodineList (IodineObject[] items)
			: base (TypeDefinition)
		{
			this.Objects = new List<IodineObject> ();
			this.Objects.AddRange (items);
			this.SetAttribute ("getSize", new InternalMethodCallback (getSize, this));
			this.SetAttribute ("add", new InternalMethodCallback (add, this));
			this.SetAttribute ("addRange", new InternalMethodCallback (add, this));
			this.SetAttribute ("remove", new InternalMethodCallback (remove, this));
			this.SetAttribute ("removeAt", new InternalMethodCallback (removeAt, this));
			this.SetAttribute ("contains", new InternalMethodCallback (contains, this));
			this.SetAttribute ("splice", new InternalMethodCallback (splice, this));
		}

		public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
		{
			IodineInteger index = key as IodineInteger;
			if (index.Value < Objects.Count)
				return this.Objects[(int)index.Value];
			vm.RaiseException (new IodineIndexException ());
			return null;
		}

		public override void SetIndex (VirtualMachine vm, IodineObject key, IodineObject value)
		{
			IodineInteger index = key as IodineInteger;
			if (index.Value < Objects.Count)
				this.Objects[(int)index.Value] = value;
			else
				vm.RaiseException (new IodineIndexException ());
		}

		public override IodineObject IterGetNext (VirtualMachine vm)
		{
			return this.Objects[iterIndex - 1];
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject collection)
		{
			switch (binop) {
			case BinaryOperation.Add:
				collection.IterReset (vm);
				while (collection.IterMoveNext (vm)) {
					IodineObject o = collection.IterGetNext (vm);
					this.Add (o);
				}
				break;
			}
			return base.PerformBinaryOperation (vm, binop, collection);
		}

		public override bool IterMoveNext (VirtualMachine vm)
		{
			if (iterIndex >= this.Objects.Count)
				return false;
			iterIndex++;
			return true;
		}

		public override void IterReset (VirtualMachine vm)
		{
			iterIndex = 0;
		}

		public void Add (IodineObject obj)
		{
			this.Objects.Add (obj);
		}

		private IodineObject getSize (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			return new IodineInteger (((IodineList)self).Objects.Count);
		}

		private IodineObject add (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			if (arguments.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineList list = self as IodineList;
			foreach (IodineObject obj in arguments) {
				list.Add (obj);
			}
			return null;
		}

		private IodineObject addRange (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			if (arguments.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineObject collection = arguments[0];
			collection.IterReset (vm);
			while (collection.IterMoveNext (vm)) {
				IodineObject o = collection.IterGetNext (vm);
				this.Add (o);
			}
			return null;
		}

		private IodineObject remove (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			if (arguments.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineObject key = arguments[0];
			if (this.Objects.Contains (key))
				this.Objects.Remove (key);
			else
				vm.RaiseException (new IodineKeyNotFound ());
			return null;
		}

		private IodineObject removeAt (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			if (arguments.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineInteger index = arguments[0] as IodineInteger;
			if (index != null)
				this.Objects.RemoveAt ((int)index.Value);
			else
				vm.RaiseException (new IodineTypeException ("Int"));
			return null;
		}

		private IodineObject contains (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			if (arguments.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineObject key = arguments[0];
			int hashCode = key.GetHashCode ();
			bool found = false;
			foreach (IodineObject obj in this.Objects) {
				if (obj.GetHashCode () == hashCode) {
					found = true;
				}
			}

			return new IodineBool (found);
		}

		private IodineObject splice (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			if (arguments.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			int start = 0;
			int end = this.Objects.Count;

			IodineInteger startInt = arguments[0] as IodineInteger;
			if (startInt == null) {
				vm.RaiseException (new IodineTypeException ("Int"));
				return null;
			}
			start = (int)startInt.Value;

			if (arguments.Length >= 2) {
				IodineInteger endInt = arguments[1] as IodineInteger;
				if (endInt == null) {
					vm.RaiseException (new IodineTypeException ("Int"));
					return null;
				}
				end = (int)endInt.Value;
			}

			if (start < 0) start = this.Objects.Count - start;
			if (end < 0) end = this.Objects.Count  - end;

			IodineList retList = new IodineList (new IodineObject[]{});

			for (int i = start; i < end; i++) {
				if (i < 0 || i > this.Objects.Count) {
					vm.RaiseException (new IodineIndexException ());
					return null;
				}
				retList.Add (this.Objects[i]);
			}

			return retList;
		}

		public override int GetHashCode ()
		{
			return Objects.GetHashCode ();
		}
	}
}
