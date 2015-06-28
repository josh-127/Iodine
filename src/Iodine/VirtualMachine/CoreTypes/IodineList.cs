using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineList : IodineObject
	{
		private static readonly IodineTypeDefinition ListTypeDef = new IodineTypeDefinition ("List"); 

		private int iterIndex = 0;
		public List<IodineObject> Objects
		{
			private set;
			get;
		}

		public IodineList (IodineObject[] items)
			: base (ListTypeDef)
		{
			this.Objects = new List<IodineObject> ();
			this.Objects.AddRange (items);
			this.SetAttribute ("getSize", new InternalMethodCallback (getSize, this));
			this.SetAttribute ("add", new InternalMethodCallback (add, this));
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
			IodineList list = self as IodineList;
			foreach (IodineObject obj in arguments) {
				list.Add (obj);
			}
			return null;
		}

		public override int GetHashCode ()
		{
			return Objects.GetHashCode ();
		}
	}
}

