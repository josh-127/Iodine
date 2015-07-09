using System;
using System.IO;
using System.Diagnostics;

namespace Iodine
{
	public class OSModule : IodineModule
	{
		class IodineProc : IodineObject
		{
			public static readonly IodineTypeDefinition ProcTypeDef = new IodineTypeDefinition ("Proc");

			public Process Value {
				private set;
				get;
			}

			public IodineProc (Process proc)
				: base (ProcTypeDef)
			{
				this.Value = proc;
				this.SetAttribute ("id", new IodineInteger (proc.Id));
				this.SetAttribute ("name", new IodineString (proc.ProcessName));
				this.SetAttribute ("kill", new InternalMethodCallback (kill, this));
			}

			private IodineObject kill (VirtualMachine vm, IodineObject self, IodineObject[] args) 
			{
				this.Value.Kill ();
				return null;
			}
		}

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
			this.SetAttribute ("getProcList", new InternalMethodCallback (getProcList, this));
		}

		private IodineObject getProcList (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineList list = new IodineList (new IodineObject[] {});
			foreach (Process proc in Process.GetProcesses ()) {
				list.Add (new IodineProc (proc));
			}
			return list;
		}

	}
}

