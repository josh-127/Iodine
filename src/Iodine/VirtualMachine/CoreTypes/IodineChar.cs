using System;

namespace Iodine
{
	public class IodineChar : IodineObject
	{
		public char Value
		{
			private set;
			get;
		}

		public IodineChar (char value)
		{
			this.Value = value;
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			IodineChar otherChr = rvalue as IodineChar;

			if (otherChr == null) {
				vm.RaiseException ("Right value must be of type char!");
				return null;
			}

			switch (binop) {
			case BinaryOperation.Equals:
				return new IodineBool (otherChr.Value == this.Value);
			case BinaryOperation.NotEquals:
				return new IodineBool (otherChr.Value != this.Value);
			default:
				return base.PerformBinaryOperation (vm, binop, rvalue);	
			}
		}

		public override string ToString ()
		{
			return Value.ToString ();
		}

		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}
	}
}

