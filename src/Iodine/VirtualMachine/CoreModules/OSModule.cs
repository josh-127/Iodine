using System;
using System.IO;

namespace Iodine
{
	public class OSModule : IodineModule
	{
		public OSModule ()
			: base ("os")
		{
			this.SetAttribute ("userDir", new IodineString (Environment.GetFolderPath (
				Environment.SpecialFolder.UserProfile)));
			this.SetAttribute ("envSep", new IodineChar (Path.PathSeparator));
		}

	}
}

