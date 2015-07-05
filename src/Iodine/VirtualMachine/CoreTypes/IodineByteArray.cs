using System;
using System.Text;

namespace Iodine
{
	public class IodineByteArray : IodineObject
	{
		public static readonly IodineTypeDefinition TypeDefinition = new ByteArrayTypeDef ();

		class ByteArrayTypeDef : IodineTypeDefinition
		{
			public ByteArrayTypeDef () 
				: base ("ByteArray")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				return null;
			}
		}

		public byte[] Array {
			private set;
			get;
		}

		private int iterIndex = 0;

		public IodineByteArray (byte[] bytes)
			: base (TypeDefinition)
		{
			this.Array = bytes;
			this.SetAttribute ("getSize", new InternalMethodCallback (getSize, this));
		}


		public override void SetIndex (VirtualMachine vm, IodineObject key, IodineObject value)
		{
			IodineInteger index = key as IodineInteger;
			IodineInteger val = value as IodineInteger;
			if (index.Value < Array.Length)
				this.Array[(int)index.Value] = (byte)(val.Value & 0xFF);
			else
				vm.RaiseException (new IodineIndexException ());
		}

		public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
		{
			IodineInteger index = key as IodineInteger;
			if (index.Value < Array.Length)
				return new IodineInteger (this.Array[(int)index.Value]);
			vm.RaiseException (new IodineIndexException ());
			return null;
		}

		public override IodineObject IterGetNext (VirtualMachine vm)
		{
			return new IodineInteger (this.Array[iterIndex - 1]);
		}

		public override bool IterMoveNext (VirtualMachine vm)
		{
			if (iterIndex >= this.Array.Length)
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
			return new IodineInteger (this.Array.Length);
		}

		public override int GetHashCode ()
		{
			return Array.GetHashCode ();
		}

		public override string ToString ()
		{
			StringBuilder accum = new StringBuilder ();
			foreach (byte b in this.Array) {
				accum.Append (b.ToString ("x2"));
			}
			return accum.ToString ();
		}
	}
}

