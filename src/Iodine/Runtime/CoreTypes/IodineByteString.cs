using System;
using System.Text;
using System.Linq;
using Iodine.Compiler;

namespace Iodine.Runtime
{
	public class IodineByteString : IodineObject
	{
		public static readonly IodineTypeDefinition TypeDefinition = new ByteStringTypeDef ();

		class ByteStringTypeDef : IodineTypeDefinition
		{
			public ByteStringTypeDef () 
				: base ("ByteStr")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
				}
				return new IodineByteString (args[0].ToString ());
			}
		}

		private int iterIndex = 0;

		public byte[] Value {
			private set;
			get;
		}

		public IodineByteString ()
			: base (TypeDefinition)
		{
		}

		public IodineByteString (byte[] val)
			: this ()
		{
			this.Value = val;
		}

		public IodineByteString (string val)
			: this ()
		{
			this.Value = Encoding.ASCII.GetBytes (val);
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			IodineByteString str = rvalue as IodineByteString;
			byte[] strVal = null;

			if (str == null) {
				if (rvalue is IodineNull) {
					return base.PerformBinaryOperation (vm, binop, rvalue);
				} else {
					vm.RaiseException ("Right value must be of type ByteStr!");
					return null;
				}
			} else {
				strVal = str.Value;
			}

			switch (binop) {
			case BinaryOperation.Equals:
				return new IodineBool (Enumerable.SequenceEqual<byte> (strVal, Value));
			case BinaryOperation.NotEquals:
				return new IodineBool (!Enumerable.SequenceEqual<byte> (strVal, Value));
			case BinaryOperation.Add:
				byte[] newArr = new byte[str.Value.Length + Value.Length];
				Array.Copy (Value, newArr, Value.Length);
				Array.Copy (strVal, 0, newArr, Value.Length, strVal.Length);
				return new IodineByteString (newArr);
			default:
				return base.PerformBinaryOperation (vm, binop, rvalue);
			}
		}
	
		public override string ToString ()
		{
			return Encoding.ASCII.GetString (this.Value);
		}

		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}

		public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
		{
			IodineInteger index = key as IodineInteger;
			if (index == null) {
				vm.RaiseException (new IodineTypeException ("Int"));
				return null;
			}
			if (index.Value >= this.Value.Length) {
				vm.RaiseException (new IodineIndexException ());
				return null;
			}
			return new IodineInteger ((long)this.Value [(int)index.Value]);
		}

		public override IodineObject IterGetNext (VirtualMachine vm)
		{
			return new IodineInteger ((long)this.Value[iterIndex - 1]);
		}

		public override bool IterMoveNext (VirtualMachine vm)
		{
			if (this.iterIndex >= this.Value.Length) {
				return false;
			}
			this.iterIndex++;
			return true;
		}

		public override void IterReset (VirtualMachine vm)
		{
			this.iterIndex = 0;
		}

	}
}

