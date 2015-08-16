using System;

namespace Iodine.Runtime
{
	public class IodineNull : IodineObject
	{
		private static IodineTypeDefinition NullTypeDef = new IodineTypeDefinition ("Null");
		public static readonly IodineNull Instance = new IodineNull ();

		protected IodineNull ()
			: base (NullTypeDef) {

		}
	}
}

