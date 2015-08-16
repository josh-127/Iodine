using System;
using Iodine.Compiler;

namespace Iodine
{
	[IodineBuiltinModule ("datetime")]
	public class DateTimeModule : IodineModule
	{
		public class IodineTimeStamp : IodineObject
		{
			public readonly static IodineTypeDefinition TimeStampTypeDef = new IodineTypeDefinition ("TimeStamp");

			public DateTime Value {
				private set;
				get;
			}

			public IodineTimeStamp (DateTime val)
				: base (TimeStampTypeDef)
			{
				this.Value = val;
				this.SetAttribute ("millisecond", new IodineInteger (val.Millisecond));
				this.SetAttribute ("second", new IodineInteger (val.Second));
				this.SetAttribute ("minute", new IodineInteger (val.Minute));
				this.SetAttribute ("hour", new IodineInteger (val.Hour));
				this.SetAttribute ("day", new IodineInteger (val.Day));
				this.SetAttribute ("month", new IodineInteger (val.Month));
				this.SetAttribute ("year", new IodineInteger (val.Year));
			}

			public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
			{
				if (rvalue is IodineTimeStamp) {
					IodineTimeStamp op = rvalue as IodineTimeStamp;
					switch (binop) {
					case BinaryOperation.GreaterThan:
						return new IodineBool (this.Value.CompareTo (op.Value) > 0);
					case BinaryOperation.LessThan:
						return new IodineBool (this.Value.CompareTo (op.Value) < 0);
					case BinaryOperation.GreaterThanOrEqu:
						return new IodineBool (this.Value.CompareTo (op.Value) >= 0);
					case BinaryOperation.LessThanOrEqu:
						return new IodineBool (this.Value.CompareTo (op.Value) <= 0);
					case BinaryOperation.Equals:
						return new IodineBool (this.Value.CompareTo (op.Value) == 0);
					}
				}
				return base.PerformBinaryOperation (vm, binop, rvalue);
			}
		}

		public DateTimeModule ()
			: base ("datetime")
		{
			this.SetAttribute ("now", new InternalMethodCallback (now, this));
		}

		private static IodineObject now (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineTimeStamp (DateTime.Now);
		}
	}

}

