using System;

namespace Iodine
{
	public class IodineName : IodineObject
	{
		public string Value
		{
			private set;
			get;
		}

		public IodineName (string val)
		{
			this.Value = val;
		}

		public override string ToString ()
		{
			return this.Value;
		}

		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}
	}
}

