using System;
using System.Text;

namespace Iodine
{
	public class IodineString : IodineObject
	{
		private static readonly IodineTypeDefinition StringTypeDef = new IodineTypeDefinition ("Str"); 
		private int iterIndex = 0;

		public string Value {
			private set;
			get;
		}

		public IodineString (string val)
			: base (StringTypeDef)
		{
			this.Value = val;
			this.SetAttribute ("toLower", new InternalMethodCallback (toLower, this));
			this.SetAttribute ("toUpper", new InternalMethodCallback (toUpper, this));
			this.SetAttribute ("substr", new InternalMethodCallback (substring, this));
			this.SetAttribute ("getSize", new InternalMethodCallback (getSize, this));
			this.SetAttribute ("indexOf", new InternalMethodCallback (indexOf, this));
			this.SetAttribute ("contains", new InternalMethodCallback (contains, this));
			this.SetAttribute ("replace", new InternalMethodCallback (replace, this));
			this.SetAttribute ("startsWith", new InternalMethodCallback (startsWith, this));
			this.SetAttribute ("endsWith", new InternalMethodCallback (endsWith, this));
			this.SetAttribute ("split", new InternalMethodCallback (split, this));
			this.SetAttribute ("join", new InternalMethodCallback (join, this));
			this.SetAttribute ("trim", new InternalMethodCallback (trim, this));
			this.SetAttribute ("format", new InternalMethodCallback (format, this));
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			IodineString str = rvalue as IodineString;
			string strVal = "";

			if (str == null) {
				if (rvalue is IodineChar) {
					strVal = rvalue.ToString ();
				} else if (rvalue is IodineNull) {
					return base.PerformBinaryOperation (vm, binop, rvalue);
				} else {
					vm.RaiseException ("Right value must be of type string!");
					return null;
				}
			} else {
				strVal = str.Value;
			}

			switch (binop) {
			case BinaryOperation.Equals:
				return new IodineBool (strVal == Value);
			case BinaryOperation.NotEquals:
				return new IodineBool (strVal != Value);
			case BinaryOperation.Add:
				return new IodineString (Value + strVal);
			default:
				return base.PerformBinaryOperation (vm, binop, rvalue);
			}
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

		public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
		{
			IodineInteger index = key as IodineInteger;
			if (index == null) {
				vm.RaiseException (new IodineTypeException ("Int"));
				return null;
			}
			if (index.Value >= this.Value.Length) {
				vm.RaiseException (new IodineIndexException ());
				return null;
			}
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

		private IodineObject toUpper (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineString (this.Value.ToUpper ());
		}

		private IodineObject toLower (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineString (this.Value.ToLower ());
		}

		private IodineObject substring (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 1) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			int start = 0;
			int len = 0;
			IodineInteger startObj = args[0] as IodineInteger;
			if (startObj == null) {
				vm.RaiseException (new IodineTypeException ("Int"));
				return null;
			}
			start = (int)startObj.Value;
			if (args.Length == 1) {
				len = this.Value.Length;
			} else {
				IodineInteger endObj = args[1] as IodineInteger;
				if (endObj == null) {
					vm.RaiseException (new IodineTypeException ("Int"));
					return null;
				}
				len = (int)endObj.Value;
			}

			if (start < this.Value.Length && len <= this.Value.Length)  {
				return new IodineString (this.Value.Substring (start, len - start));
			}
			vm.RaiseException (new IodineIndexException ());
			return null;
		}

		private IodineObject getSize (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineInteger (this.Value.Length);
		}

		private IodineObject indexOf (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 1) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineChar ch = args[0] as IodineChar;
			char val;
			if (ch == null) {
				if (args[0] is IodineString) {
					val = args[0].ToString ()[0];
				} else {
					vm.RaiseException (new IodineTypeException ("Char"));
					return null;
				}
			} else {
				val = ch.Value;
			}

			return new IodineInteger (this.Value.IndexOf (val));
		}

		private IodineObject contains (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 1) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			return new IodineBool (this.Value.Contains (args[0].ToString ()));
		}

		private IodineObject startsWith (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 1) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			return new IodineBool (this.Value.StartsWith (args[0].ToString ()));
		}

		private IodineObject endsWith (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 1) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			return new IodineBool (this.Value.EndsWith (args[0].ToString ()));
		}

		private IodineObject replace (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}
			IodineString arg1 = args[0] as IodineString;
			IodineString arg2 = args[1] as IodineString;
			if (arg1 == null || arg2 == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}
			return new IodineString (this.Value.Replace (arg1.Value, arg2.Value));
		}

		private IodineObject split (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 1) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineString selfStr = self as IodineString;
			IodineChar ch = args[0] as IodineChar;
			char val;
			if (ch == null) {
				if (args[0] is IodineString) {
					val = args[0].ToString ()[0];
				} else {
					vm.RaiseException (new IodineTypeException ("Char"));
					return null;
				}
			} else {
				val = ch.Value;
			}
			IodineList list = new IodineList (new IodineObject[]{});
			foreach (string str in selfStr.Value.Split (val)) {
				list.Add (new IodineString (str));
			}
			return list;
		}

		private IodineObject trim (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineString (this.Value.Trim ());
		}

		private IodineObject join (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			StringBuilder accum = new StringBuilder ();
			IodineObject collection = args[0];
			collection.IterReset (vm);
			string last = "";
			string sep = "";
			while (collection.IterMoveNext (vm)) {
				IodineObject o = collection.IterGetNext (vm);
				accum.AppendFormat ("{0}{1}", last, sep);
				last = o.ToString ();
				sep = this.Value;
			}
			accum.Append (last);
			return new IodineString (accum.ToString ());
		}

		private IodineObject format (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			string format = this.Value;
			IodineFormatter formatter = new IodineFormatter ();
			return new IodineString (formatter.Format (vm, format, args));
		}
	}
}

