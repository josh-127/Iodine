using System;
using System.Collections.Generic;

namespace Iodine.Runtime
{
	public interface IIodineExtension
	{
		void Initialize (Dictionary<string, IodineObject> globalDict);
	}
}

