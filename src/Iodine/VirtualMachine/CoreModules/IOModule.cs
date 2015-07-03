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
		}


		class IodineFile : IodineObject
		{
			public readonly static IodineTypeDefinition FileTypeDef = new IodineTypeDefinition ("File");

			public IodineFile ()
				: base (FileTypeDef)
			{
				this.SetAttribute ("remove", new InternalMethodCallback (remove, this));
				this.SetAttribute ("exists", new InternalMethodCallback (exists, this));
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
		}

		public IOModule ()
			: base ("io")
		{
			this.SetAttribute ("Directory", new IodineDirectory ());
			this.SetAttribute ("File", new IodineFile ());
		}
	}
}

