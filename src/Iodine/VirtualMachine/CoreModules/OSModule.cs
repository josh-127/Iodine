using System;

namespace Iodine
{
	public class OSModule : IodineModule
	{
		public OSModule ()
			: base ("os")
		{
			this.SetAttribute ("userDir", new IodineString (Environment.GetFolderPath (
				Environment.SpecialFolder.UserProfile)));
		}

	}
}

