using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineTuple : IodineObject
	{
		private static readonly IodineTypeDefinition TupleTypeDef = new IodineTypeDefinition ("Tuple"); 

		private int iterIndex = 0;

		public IodineObject[] Objects
		{
			private set;
			get;
		}

		public IodineTuple (IodineObject[] items)
			: base (TupleTypeDef)
		{
			this.Objects = items;
			this.SetAttribute ("getSize", new InternalMethodCallback (getSize, this));
		}

		public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
		{
			IodineInteger index = key as IodineInteger;
			if (index.Value < Objects.Length)
				return this.Objects[(int)index.Value];
			vm.RaiseException (new IodineIndexException ());
			return null;
		}

		public override IodineObject IterGetNext (VirtualMachine vm)
		{
			return this.Objects[iterIndex - 1];
		}

		public override bool IterMoveNext (VirtualMachine vm)
		{
			if (iterIndex >= this.Objects.Length)
				return false;
			iterIndex++;
			return true;
		}

		public override void IterReset (VirtualMachine vm)
		{
			iterIndex = 0;
		}

		private IodineObject getSize (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			return new IodineInteger (((IodineList)self).Objects.Count);
		}

		public override int GetHashCode ()
		{
			return Objects.GetHashCode ();
		}
	}
}

