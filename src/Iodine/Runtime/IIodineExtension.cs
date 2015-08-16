using System;
using System.Collections.Generic;

namespace Iodine
{
	public interface IIodineExtension
	{
		void Initialize (Dictionary<string, IodineObject> globalDict);
	}
}

