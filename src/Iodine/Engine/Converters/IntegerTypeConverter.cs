using System;
using Iodine.Runtime;

namespace Iodine
{
	internal class IntegerTypeConverter : ITypeConverter
	{
		public bool TryToConvertToPrimative (IodineObject obj, out object result)
		{
			IodineInteger integer = obj as IodineInteger;
			if (integer != null) {
				result = integer.Value;
				return true;
			}
			result = null;
			return false;
		}

		public bool TryToConvertFromPrimative (object obj, out IodineObject result)
		{
			if (obj is IConvertible) {
				result = new IodineInteger (Convert.ToInt64 (obj));
				return true;
			}
			result = null;
			return false;
		}
	}
}

