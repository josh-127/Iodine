using System;

namespace Iodine
{
	public class IodineChar : IodineObject
	{
		private static readonly IodineTypeDefinition CharTypeDef = new IodineTypeDefinition ("Char"); 

		public char Value
		{
			private set;
			get;
		}

		public IodineChar (char value)
			: base (CharTypeDef)
		{
			this.Value = value;
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			
			IodineChar otherChr = rvalue as IodineChar;
			char otherVal;
			if (otherChr == null) {
				if (rvalue is IodineString) {
					otherVal = rvalue.ToString ()[0];
				} else {
					vm.RaiseException ("Right value must be of type char!");
					return null;
				}
			} else {
				otherVal = otherChr.Value;
			}

			switch (binop) {
			case BinaryOperation.Equals:
				return new IodineBool (otherVal == this.Value);
			case BinaryOperation.NotEquals:
				return new IodineBool (otherVal != this.Value);
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

