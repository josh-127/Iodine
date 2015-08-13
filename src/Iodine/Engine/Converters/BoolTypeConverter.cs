using System;

namespace Iodine
{
	internal class BoolTypeConverter : ITypeConverter
	{
		public bool TryToConvertToPrimative (IodineObject obj, out object result)
		{
			IodineBool boolean = obj as IodineBool;
			if (boolean != null) {
				result = boolean.Value;
				return true;
			}
			result = null;
			return false;
		}

		public bool TryToConvertFromPrimative (object obj, out IodineObject result)
		{
			if (obj is Boolean) {
				result = ((IodineBool)obj).Value ? IodineBool.True : IodineBool.False;
				return true;
			}
			result = null;
			return false;
		}
	}
}

