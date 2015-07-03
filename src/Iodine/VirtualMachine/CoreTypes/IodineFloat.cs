using System;

namespace Iodine
{
	public class IodineFloat : IodineObject
	{
		private static readonly IodineTypeDefinition FloatTypeDef = new IodineTypeDefinition ("Float"); 

		public double Value {
			private set;
			get;
		}

		public IodineFloat (double val)
			: base (FloatTypeDef)
		{
			this.Value = val;
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			IodineFloat floatVal = rvalue as IodineFloat;

			double op2 = 0;
			if (floatVal == null) {
				if (rvalue is IodineInteger) {
					IodineInteger intVal = rvalue as IodineInteger;
					op2 = (double)intVal.Value;
				} else {
					vm.RaiseException (new IodineTypeException ("Float"));
					return null;
				}
			}

			switch (binop) {
			case BinaryOperation.Add:
				return new IodineFloat (Value + op2);
			case BinaryOperation.Sub:
				return new IodineFloat (Value - op2);
			case BinaryOperation.Mul:
				return new IodineFloat (Value * op2);
			case BinaryOperation.Div:
				return new IodineFloat (Value / op2);
			case BinaryOperation.Mod:
				return new IodineFloat (Value % op2);
			case BinaryOperation.Equals:
				return new IodineBool (Value == op2);
			case BinaryOperation.NotEquals:
				return new IodineBool (Value != op2);
			case BinaryOperation.GreaterThan:
				return new IodineBool (Value > op2);
			case BinaryOperation.GreaterThanOrEqu:
				return new IodineBool (Value >= op2);
			case BinaryOperation.LessThan:
				return new IodineBool (Value < op2);
			case BinaryOperation.LessThanOrEqu:
				return new IodineBool (Value <= op2);
			default:
				return base.PerformBinaryOperation (vm, binop, rvalue);
			}
		}


		public override IodineObject PerformUnaryOperation (VirtualMachine vm, UnaryOperation op)
		{
			switch (op) {
			case UnaryOperation.Negate:
				return new IodineFloat (-this.Value);
			}
			return null;
		}
		public override void PrintTest ()
		{
			Console.WriteLine (this.Value);
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

