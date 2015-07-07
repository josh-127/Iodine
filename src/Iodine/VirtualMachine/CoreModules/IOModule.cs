using System;
using System.IO;

namespace Iodine
{
	public class IOModule : IodineModule
	{
		class IodineDirectory : IodineObject
		{
			public readonly static IodineTypeDefinition DirectoryTypeDef = new IodineTypeDefinition ("Directory");

			public IodineDirectory ()
				: base (DirectoryTypeDef)
			{
				this.SetAttribute ("separator", new IodineChar (Path.DirectorySeparatorChar));
				this.SetAttribute ("listFiles", new InternalMethodCallback (listFiles, this));
				this.SetAttribute ("listDirectories", new InternalMethodCallback (listDirectories, this));
				this.SetAttribute ("remove", new InternalMethodCallback (remove, this));
				this.SetAttribute ("exists", new InternalMethodCallback (exists, this));
				this.SetAttribute ("create", new InternalMethodCallback (create, this));
				this.SetAttribute ("copy", new InternalMethodCallback (copy, this));
			}

			private IodineObject listFiles (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args[0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				if (!Directory.Exists (args[0].ToString ())) {
					vm.RaiseException (new IodineIOException ("Directory '" + args[0].ToString () + 
						"' does not exist!"));
					return null;
				}

				IodineList ret = new IodineList (new IodineObject[]{});

				foreach (string dir in Directory.GetFiles (args[0].ToString ())) {
					ret.Add (new IodineString (dir));
				}
				return ret;
			}

			private IodineObject listDirectories (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args[0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				if (!Directory.Exists (args[0].ToString ())) {
					vm.RaiseException (new IodineIOException ("Directory '" + args[0].ToString () + 
						"' does not exist!"));
					return null;
				}

				IodineList ret = new IodineList (new IodineObject[]{});

				foreach (string dir in Directory.GetDirectories (args[0].ToString ())) {
					ret.Add (new IodineString (dir));
				}
				return ret;
			}

			private IodineObject remove (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args[0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				if (!Directory.Exists (args[0].ToString ())) {
					vm.RaiseException (new IodineIOException ("Directory '" + args[0].ToString () + 
						"' does not exist!"));
					return null;
				}

				Directory.Delete (args[0].ToString ());

				return null;
			}

			private IodineObject exists (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args[0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				return new IodineBool (Directory.Exists (args[0].ToString ()));
			}

			private IodineObject create (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args[0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}
				Directory.CreateDirectory (args[0].ToString ());
				return null;
			}

			private IodineObject copy (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				if (args.Length <= 1) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args[0] is IodineString) || !(args[1] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}
				bool recurse = false;
				if (args.Length >= 3) {
					if (!(args[2] is IodineBool)) {
						vm.RaiseException (new IodineTypeException ("Bool"));
						return null;
					}
					recurse = ((IodineBool)args[2]).Value;
				}
				bool res = copyDir (args[0].ToString (), args[1].ToString (), recurse);
				if (!res) {
					vm.RaiseException (new IodineIOException ("Could not find directory " + args[0].ToString () ));
				}
				return null;
			}

			private static bool copyDir (string src, string dest, bool recurse)
			{
				DirectoryInfo dir = new DirectoryInfo(src);
				DirectoryInfo[] dirs = dir.GetDirectories();

				if (!dir.Exists) {
					return false;
				}

				if (!Directory.Exists(dest)) {
					Directory.CreateDirectory(dest);
				}

				FileInfo[] files = dir.GetFiles();
				foreach (FileInfo file in files) {
					string temppath = Path.Combine(dest, file.Name);
					file.CopyTo(temppath, false);
				}

				if (recurse) {
					foreach (DirectoryInfo subdir in dirs) {
						string temppath = Path.Combine(dest, subdir.Name);
						copyDir(subdir.FullName, temppath, recurse);
					}
				}
				return true;
			}
		}


		class IodineFile : IodineObject
		{
			public readonly static IodineTypeDefinition FileTypeDef = new IodineTypeDefinition ("File");

			public IodineFile ()
				: base (FileTypeDef)
			{
				this.SetAttribute ("remove", new InternalMethodCallback (remove, this));
				this.SetAttribute ("exists", new InternalMethodCallback (exists, this));
				this.SetAttribute ("getNameWithoutExt", new InternalMethodCallback (getNameWithoutExt, this));
				this.SetAttribute ("getName", new InternalMethodCallback (getName, this));
				this.SetAttribute ("copy", new InternalMethodCallback (copy, this));
			}

			private IodineObject remove (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args[0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				if (!File.Exists (args[0].ToString ())) {
					vm.RaiseException (new IodineIOException ("File '" + args[0].ToString () + 
						"' does not exist!"));
					return null;
				}

				File.Delete (args[0].ToString ());

				return null;
			}

			private IodineObject exists (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args[0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				return new IodineBool (File.Exists (args[0].ToString ()));
			}

			private IodineObject getNameWithoutExt (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args[0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}
				return new IodineString (Path.GetFileNameWithoutExtension (args[0].ToString ()));
			}

			private IodineObject getName (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args[0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}
				return new IodineString (Path.GetFileName (args[0].ToString ()));
			}

			private IodineObject copy (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args[0] is IodineString) || !(args[1] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				if (!File.Exists (args[0].ToString ())) {
					vm.RaiseException (new IodineIOException ("File '" + args[0].ToString () + 
						"' does not exist!"));
					return null;
				}

				File.Copy (args[0].ToString (), args[1].ToString (), true);
				return null;
			}
		}

		public IOModule ()
			: base ("io")
		{
			this.SetAttribute ("Directory", new IodineDirectory ());
			this.SetAttribute ("File", new IodineFile ());
			this.SetAttribute ("getCreationTime", new InternalMethodCallback (getModifiedTime, this));
			this.SetAttribute ("getModifiedTime", new InternalMethodCallback (getCreationTime, this));
		}

		private IodineObject getModifiedTime (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			if (!(args[0] is IodineString)) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			
			}
			if (!File.Exists (args[0].ToString ())) {
				vm.RaiseException (new IodineIOException ("File '" + args[0].ToString () + 
					"' does not exist!"));
				return null;
			}
			return new DateTimeModule.IodineTimeStamp (File.GetLastAccessTime (args[0].ToString ()));
		}

		private IodineObject getCreationTime (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			if (!(args[0] is IodineString)) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}
			if (!File.Exists (args[0].ToString ())) {
				vm.RaiseException (new IodineIOException ("File '" + args[0].ToString () + 
					"' does not exist!"));
				return null;
			}
			return new DateTimeModule.IodineTimeStamp (File.GetCreationTime (args[0].ToString ()));
		}
	}
}

