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

			this.SetAttribute ("searchPaths", new IodineList (IodineModule.SearchPaths)); // Obsolete

			this.SetAttribute ("getProcList", new InternalMethodCallback (getProcList, this));
			this.SetAttribute ("getEnv", new InternalMethodCallback (getEnv, this));
			this.SetAttribute ("setEnv", new InternalMethodCallback (setEnv, this));
		}

		private IodineObject getProcList (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineList list = new IodineList (new IodineObject[] {});
			foreach (Process proc in Process.GetProcesses ()) {
				list.Add (new IodineProc (proc));
			}
			return list;
		}

		private IodineObject getEnv (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}
			IodineString str = args[0] as IodineString;

			if (str == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}
			if (Environment.GetEnvironmentVariable (str.Value) != null)
				return new IodineString (Environment.GetEnvironmentVariable (str.Value));
			else 
				return null;
		}

		private IodineObject setEnv (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}
			IodineString str = args[0] as IodineString;
			Environment.SetEnvironmentVariable (str.Value, args[1].ToString (), EnvironmentVariableTarget.User);
			return null;
		}

		private IodineObject exec (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}

			IodineString str = args[0] as IodineString;
			string cmdArgs = "";
			bool wait = true;

			if (str == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			if (args.Length >= 2) {
				IodineString cmdArgsObj = args[1] as IodineString;
				if (cmdArgsObj == null) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}
				cmdArgs = cmdArgsObj.Value;
			}

			if (args.Length >= 3) {
				IodineBool waitObj = args[2] as IodineBool;
				if (waitObj == null) {
					vm.RaiseException (new IodineTypeException ("Bool"));
					return null;
				}
				wait = waitObj.Value;
			}

			ProcessStartInfo info = new ProcessStartInfo (str.Value, cmdArgs);
			info.UseShellExecute = false;
			Process proc = Process.Start (info);
			if (wait) {
				proc.WaitForExit ();
			}
			return new IodineInteger (proc.ExitCode);
		}
	}
}

