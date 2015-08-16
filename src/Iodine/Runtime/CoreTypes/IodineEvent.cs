using System;
using System.Collections.Generic;
using Iodine.Compiler;

namespace Iodine
{
	public class IodineEvent : IodineObject
	{
		public static readonly IodineTypeDefinition TypeDefinition = new EventTypeDef ();

		class EventTypeDef : IodineTypeDefinition
		{
			public EventTypeDef () 
				: base ("Event")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				return new IodineEvent ();
			}
		}

		private List<IodineObject> handlers = new List<IodineObject> ();

		public IodineEvent ()
			: base (TypeDefinition)
		{
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			switch (binop) {
			case BinaryOperation.Add:
				this.handlers.Add (rvalue);
				break;
			case BinaryOperation.Sub:
				this.handlers.Remove (rvalue);
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

