using System;
using System.Collections.Generic;

namespace Iodine
{
	public static class BuiltInModules
	{
		public static readonly Dictionary<string, IodineModule> Modules = new Dictionary<string, IodineModule> ();

		static BuiltInModules ()
		{
			Modules["random"] = new RandomModule ();
			Modules["regex"] = new RegexModule ();
			Modules["hash"] = new HashModule ();
			Modules["threading"] = new ThreadingModule ();
			Modules["io"] = new IOModule ();
			Modules["os"] = new OSModule ();
		}
	}
}

