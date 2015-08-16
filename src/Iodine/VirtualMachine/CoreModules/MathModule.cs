using System;
using System.IO;
using System.Reflection;

namespace Iodine
{
	[IodineBuiltinModule ("math")]
	public class MathModule : IodineModule
	{
		public MathModule ()
			: base ("math")
		{
			this.SetAttribute ("PI", new IodineFloat (Math.PI));
			this.SetAttribute ("E", new IodineFloat (Math.E));
			this.SetAttribute ("sin", new InternalMethodCallback (sin, this));
			this.SetAttribute ("cos", new InternalMethodCallback (cos, this));
			this.SetAttribute ("tan", new InternalMethodCallback (tan, this));
			this.SetAttribute ("asin", new InternalMethodCallback (asin, this));
			this.SetAttribute ("acos", new InternalMethodCallback (acos, this));
			this.SetAttribute ("atan", new InternalMethodCallback (atan, this));
			this.SetAttribute ("abs", new InternalMethodCallback (abs, this));
			this.SetAttribute ("sqrt", new InternalMethodCallback (sqrt, this));
			this.SetAttribute ("floor", new InternalMethodCallback (floor, this));
			this.SetAttribute ("ceiling", new InternalMethodCallback (ceiling, this));
			this.SetAttribute ("log", new InternalMethodCallback (log, this));
		}

		private IodineObject sin (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			double input = 0;
			if (args[0] is IodineInteger) {
				input = (double)((IodineInteger)args[0]).Value;
			} else if (args[0] is IodineFloat) {
				input = ((IodineFloat)args[0]).Value;
			} else {
				vm.RaiseException (new IodineTypeException ("Float"));
				return null;
			}

			return new IodineFloat (Math.Sin (input));
		}

		private IodineObject cos (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			double input = 0;
			if (args[0] is IodineInteger) {
				input = (double)((IodineInteger)args[0]).Value;
			} else if (args[0] is IodineFloat) {
				input = ((IodineFloat)args[0]).Value;
			} else {
				vm.RaiseException (new IodineTypeException ("Float"));
				return null;
			}

			return new IodineFloat (Math.Cos (input));
		}

		private IodineObject tan (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			double input = 0;
			if (args[0] is IodineInteger) {
				input = (double)((IodineInteger)args[0]).Value;
			} else if (args[0] is IodineFloat) {
				input = ((IodineFloat)args[0]).Value;
			} else {
				vm.RaiseException (new IodineTypeException ("Float"));
				return null;
			}

			return new IodineFloat (Math.Tan (input));
		}

		private IodineObject asin (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			double input = 0;
			if (args[0] is IodineInteger) {
				input = (double)((IodineInteger)args[0]).Value;
			} else if (args[0] is IodineFloat) {
				input = ((IodineFloat)args[0]).Value;
			} else {
				vm.RaiseException (new IodineTypeException ("Float"));
				return null;
			}

			return new IodineFloat (Math.Asin (input));
		}

		private IodineObject acos (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			double input = 0;
			if (args[0] is IodineInteger) {
				input = (double)((IodineInteger)args[0]).Value;
			} else if (args[0] is IodineFloat) {
				input = ((IodineFloat)args[0]).Value;
			} else {
				vm.RaiseException (new IodineTypeException ("Float"));
				return null;
			}

			return new IodineFloat (Math.Acos (input));
		}

		private IodineObject atan (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			double input = 0;
			if (args[0] is IodineInteger) {
				input = (double)((IodineInteger)args[0]).Value;
			} else if (args[0] is IodineFloat) {
				input = ((IodineFloat)args[0]).Value;
			} else {
				vm.RaiseException (new IodineTypeException ("Float"));
				return null;
			}

			return new IodineFloat (Math.Atan (input));
		}

		private IodineObject abs (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			double input = 0;
			if (args[0] is IodineInteger) {
				input = (double)((IodineInteger)args[0]).Value;
			} else if (args[0] is IodineFloat) {
				input = ((IodineFloat)args[0]).Value;
			} else {
				vm.RaiseException (new IodineTypeException ("Float"));
				return null;
			}

			return new IodineFloat (Math.Abs (input));
		}

		private IodineObject sqrt (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			double input = 0;
			if (args[0] is IodineInteger) {
				input = (double)((IodineInteger)args[0]).Value;
			} else if (args[0] is IodineFloat) {
				input = ((IodineFloat)args[0]).Value;
			} else {
				vm.RaiseException (new IodineTypeException ("Float"));
				return null;
			}

			return new IodineFloat (Math.Sqrt (input));
		}

		private IodineObject floor (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			double input = 0;
			if (args[0] is IodineInteger) {
				input = (double)((IodineInteger)args[0]).Value;
			} else if (args[0] is IodineFloat) {
				input = ((IodineFloat)args[0]).Value;
			} else {
				vm.RaiseException (new IodineTypeException ("Float"));
				return null;
			}

			return new IodineFloat (Math.Floor (input));
		}

		private IodineObject ceiling (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			double input = 0;
			if (args[0] is IodineInteger) {
				input = (double)((IodineInteger)args[0]).Value;
			} else if (args[0] is IodineFloat) {
				input = ((IodineFloat)args[0]).Value;
			} else {
				vm.RaiseException (new IodineTypeException ("Float"));
				return null;
			}

			return new IodineFloat (Math.Ceiling (input));
		}	

		private IodineObject log (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			double input = 0;
			if (args[0] is IodineInteger) {
				input = (double)((IodineInteger)args[0]).Value;
			} else if (args[0] is IodineFloat) {
				input = ((IodineFloat)args[0]).Value;
			} else {
				vm.RaiseException (new IodineTypeException ("Float"));
				return null;
			}

			return new IodineFloat (Math.Log (input));
		}	
	}
}

