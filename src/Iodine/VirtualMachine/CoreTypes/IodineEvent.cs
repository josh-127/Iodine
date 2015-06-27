using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineEvent : IodineObject
	{
		private static readonly IodineTypeDefinition EventTypeDef = new IodineTypeDefinition ("Event");
		private List<IodineObject> handlers = new List<IodineObject> ();

		public IodineEvent ()
			: base (EventTypeDef)
		{
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			switch (binop) {
			case BinaryOperation.Add:
				this.handlers.Add (rvalue);
				break;
			case BinaryOperation.Sub:
				this.handlers.Add (rvalue);
				break;
			default:
				return base.PerformBinaryOperation (vm, binop, rvalue);
			}
			return this;
		}

		public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			foreach (IodineObject obj in this.handlers) {
				obj.Invoke (vm, arguments);
			}
			return null;
		}
	}
}

