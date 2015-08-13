﻿using System;

namespace Iodine
{
	public interface ITypeConverter
	{
		bool TryToConvertToPrimative (IodineObject obj, out object result);
		bool TryToConvertFromPrimative (object obj, out IodineObject result);
	}
}

