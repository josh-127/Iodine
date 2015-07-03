using System;

namespace Iodine
{
	public class IodineChar : IodineObject
	{
		private static readonly IodineTypeDefinition CharTypeDef = new IodineTypeDefinition ("Char"); 

		public char Value {
			private set;
			get;
		}

		public IodineChar (char value)
			: base (CharTypeDef)
		{
			this.Value = value;
			this.SetAttribute ("isLetter", new InternalMethodCallback (isLetter, this));
			this.SetAttribute ("isDigit", new InternalMethodCallback (isDigit, this));
			this.SetAttribute ("isLetterOrDigit", new InternalMethodCallback (isLetterOrDigit, this));
			this.SetAttribute ("isWhiteSpace", new InternalMethodCallback (isWhiteSpace, this));
			this.SetAttribute ("isSymbol", new InternalMethodCallback (isSymbol, this));
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			
			IodineChar otherChr = rvalue as IodineChar;
			char otherVal;
			if (otherChr == null) {
				if (rvalue is IodineString) {
					otherVal = rvalue.ToString ()[0];
				} else if (rvalue is IodineNull) {
					return base.PerformBinaryOperation (vm, binop, rvalue);
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
			return Value.ToString ().GetHashCode ();
		}

		private IodineObject isLetter (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineBool (char.IsLetter (this.Value));
		}

		private IodineObject isDigit (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineBool (char.IsDigit (this.Value));
		}

		private IodineObject isLetterOrDigit (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineBool (char.IsLetterOrDigit (this.Value));
		}

		private IodineObject isWhiteSpace (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineBool (char.IsWhiteSpace (this.Value));
		}

		private IodineObject isSymbol (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineBool (char.IsSymbol (this.Value));
		}
	}
}

