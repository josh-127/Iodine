using System;
using Iodine.Runtime;

namespace Iodine
{
	internal class StringTypeConverter : ITypeConverter
	{
		public bool TryToConvertToPrimative (IodineObject obj, out object result)
		{
			result = obj.ToString ();
			return true;
		}

		public bool TryToConvertFromPrimative (object obj, out IodineObject result)
		{
			result = new IodineString (obj.ToString ());
			return true;
		}
	}
}

