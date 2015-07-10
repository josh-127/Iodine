using System;
using System.IO;
using System.Reflection;

namespace Iodine
{
	public class SysModule : IodineModule
	{
		public SysModule ()
			: base ("sys")
		{
			this.SetAttribute ("prefix", new IodineString (Assembly.GetExecutingAssembly ().Location));

			this.SetAttribute ("path", new IodineList (IodineModule.SearchPaths)); // Obsolete
		}

	}
}

