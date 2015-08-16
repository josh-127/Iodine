using System;

namespace Iodine
{
	public class IodineName : IodineObject
	{
		private static readonly IodineTypeDefinition NameTypeDef = new IodineTypeDefinition ("Name"); 

		public string Value {
			private set;
			get;
		}

		public IodineName (string val)
			: base (NameTypeDef)
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

