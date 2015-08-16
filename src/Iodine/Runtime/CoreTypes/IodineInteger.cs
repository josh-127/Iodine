using System;
using Iodine.Compiler;

namespace Iodine
{
	public class IodineInteger : IodineObject
	{
		public static readonly IodineTypeDefinition TypeDefinition = new IntTypeDef ();

		class IntTypeDef : IodineTypeDefinition
		{
			public IntTypeDef () 
				: base ("Int")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
				}
				return new IodineInteger (Int64.Parse (args[0].ToString ()));
			}
		}

		public long Value {
			private set;
			get;
		}

		public IodineInteger (long val)
			: base (TypeDefinition)
		{
			this.Value = val;
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			IodineInteger intVal = rvalue as IodineInteger;

			if (intVal == null) {
				vm.RaiseException (new IodineTypeException ("Int"));
				return null;
			}

			switch (binop) {
			case BinaryOperation.Add:
				return new IodineInteger (Value + intVal.Value);
			case BinaryOperation.Sub:
				return new IodineInteger (Value - intVal.Value);
			case BinaryOperation.Mul:
				return new IodineInteger (Value * intVal.Value);
			case BinaryOperation.Div:
				return new IodineInteger (Value / intVal.Value);
			case BinaryOperation.Mod:
				return new IodineInteger (Value % intVal.Value);
			case BinaryOperation.And:
				return new IodineInteger (Value & intVal.Value);
			case BinaryOperation.Or:
				return new IodineInteger (Value | intVal.Value);
			case BinaryOperation.Xor:
				return new IodineInteger (Value ^ intVal.Value);
			case BinaryOperation.LeftShift:
				return new IodineInteger (Value << (int)intVal.Value);
			case BinaryOperation.RightShift:
				return new IodineInteger (Value >> (int)intVal.Value);
			case BinaryOperation.Equals:
				return new IodineBool (Value == intVal.Value);
			case BinaryOperation.NotEquals:
				return new IodineBool (Value != intVal.Value);
			case BinaryOperation.GreaterThan:
				return new IodineBool (Value > intVal.Value);
			case BinaryOperation.GreaterThanOrEqu:
				return new IodineBool (Value >= intVal.Value);
			case BinaryOperation.LessThan:
				return new IodineBool (Value < intVal.Value);
			case BinaryOperation.LessThanOrEqu:
				return new IodineBool (Value <= intVal.Value);
			default:
				return base.PerformBinaryOperation (vm, binop, rvalue);
			}
		}


		public override IodineObject PerformUnaryOperation (VirtualMachine vm, UnaryOperation op)
		{
			switch (op) {
			case UnaryOperation.Not:
				return new IodineInteger (~this.Value);
			case UnaryOperation.Negate:
				return new IodineInteger (-this.Value);
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

