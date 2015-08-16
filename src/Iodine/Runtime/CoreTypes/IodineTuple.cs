using System;
using System.Collections.Generic;

namespace Iodine.Runtime
{
	public class IodineTuple : IodineObject
	{
		public static readonly IodineTypeDefinition TypeDefinition = new TupleTypeDef ();

		class TupleTypeDef : IodineTypeDefinition
		{
			public TupleTypeDef ()
				: base ("Tuple")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				if (args.Length >= 1) {
					IodineList inputList = args [0] as IodineList;
					return new IodineTuple (inputList.Objects.ToArray ());
				}
				return null;
			}
		}

		private int iterIndex = 0;

		public IodineObject[] Objects {
			private set;
			get;
		}

		public IodineTuple (IodineObject[] items)
			: base (TypeDefinition)
		{
			this.Objects = items;
			this.SetAttribute ("getSize", new InternalMethodCallback (getSize, this));
		}

		public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
		{
			IodineInteger index = key as IodineInteger;
			if (index.Value < Objects.Length)
				return this.Objects [(int)index.Value];
			vm.RaiseException (new IodineIndexException ());
			return null;
		}

		public override IodineObject IterGetNext (VirtualMachine vm)
		{
			return this.Objects [iterIndex - 1];
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
			return new IodineInteger (this.Objects.Length);
		}

		public override int GetHashCode ()
		{
			return Objects.GetHashCode ();
		}
	}
}

