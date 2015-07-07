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
			IodineList searchList = new IodineList (new IodineObject[] {});
			foreach (string path in IodineModule.SearchPaths) {
				searchList.Add (new IodineString (path));
			}
			this.SetAttribute ("searchPaths", searchList);
		}

	}
}

