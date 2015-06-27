using System;

namespace Iodine
{
	public class IodineString : IodineObject
	{
		private int iterIndex = 0;

		public string Value
		{
			private set;
			get;
		}

		public IodineString (string val)
		{
			this.Value = val;
			this.SetAttribute ("substr", new InternalMethodCallback (substring, this));
			this.SetAttribute ("getSize", new InternalMethodCallback (getSize, this));
			this.SetAttribute ("indexOf", new InternalMethodCallback (indexOf, this));
			this.SetAttribute ("contains", new InternalMethodCallback (contains, this));
			this.SetAttribute ("replace", new InternalMethodCallback (contains, this));
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			IodineString strVal = rvalue as IodineString;

			if (strVal == null) {
				vm.RaiseException ("Right value must be of type string!");
				return null;
			}

			switch (binop) {
			case BinaryOperation.Equals:
				return new IodineBool (strVal.Value == Value);
			case BinaryOperation.NotEquals:
				return new IodineBool (strVal.Value != Value);
			case BinaryOperation.Add:
				return new IodineString (Value + strVal.Value);
			}
			return null;
		}

		public override void PrintTest ()
		{
			Console.WriteLine (Value);
		}

		public override string ToString ()
		{
			return this.Value;
		}

		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}

		public override IodineObject GetIndex (IodineObject key)
		{
			IodineInteger index = key as IodineInteger;

			return new IodineChar (this.Value[(int)index.Value]);
		}

		public override IodineObject IterGetNext (VirtualMachine vm)
		{
			return new IodineChar (this.Value[iterIndex - 1]);
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

		private IodineObject substring (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 1) {
				vm.RaiseException ("Expected one or more arguments!");
				return null;
			}
			int start = 0;
			int len = 0;
			IodineString selfStr = self as IodineString;
			IodineInteger startObj = args[0] as IodineInteger;
			if (startObj == null) {
				vm.RaiseException ("Parameter start must be of type Integer!");
				return null;
			}
			start = (int)startObj.Value;
			if (args.Length == 1) {
				len = selfStr.Value.Length;
			} else {
				IodineInteger endObj = args[1] as IodineInteger;
				if (endObj == null) {
					vm.RaiseException ("Parameter end must be of type Integer!");
					return null;
				}
				len = (int)endObj.Value;
			}
			return new IodineString (selfStr.Value.Substring (start, len));
		}

		private IodineObject getSize (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString selfStr = self as IodineString;
			return new IodineInteger (selfStr.Value.Length);
		}

		private IodineObject indexOf (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 1) {
				vm.RaiseException ("Expected one or more arguments!");
				return null;
			}

			IodineString selfStr = self as IodineString;
			IodineChar ch = args[0] as IodineChar;
			if (ch == null) {
				vm.RaiseException ("Parameter must be of type char!");
				return null;
			}

			return new IodineInteger (selfStr.Value.IndexOf (ch.Value));
		}

		private IodineObject contains (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 1) {
				vm.RaiseException ("Expected one or more arguments!");
				return null;
			}
			IodineString selfStr = self as IodineString;

			return new IodineBool (selfStr.Value.Contains (args[0].ToString ()));
		}

		private IodineObject replace (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException ("Expected two or more arguments!");
				return null;
			}
			IodineString selfStr = self as IodineString;
			IodineString arg1 = args[0] as IodineString;
			IodineString arg2 = args[1] as IodineString;
			if (arg1 == null || arg2 == null) {
				vm.RaiseException ("Expected argument of type string!");
				return null;
			}
			return new IodineString (selfStr.Value.Replace (arg1.Value, arg2.Value));
		}
	}
}

