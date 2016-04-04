/**
  * Copyright (c) 2015, GruntTheDivine All rights reserved.

  * Redistribution and use in source and binary forms, with or without modification,
  * are permitted provided that the following conditions are met:
  * 
  *  * Redistributions of source code must retain the above copyright notice, this list
  *    of conditions and the following disclaimer.
  * 
  *  * Redistributions in binary form must reproduce the above copyright notice, this
  *    list of conditions and the following disclaimer in the documentation and/or
  *    other materials provided with the distribution.

  * Neither the name of the copyright holder nor the names of its contributors may be
  * used to endorse or promote products derived from this software without specific
  * prior written permission.
  * 
  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
  * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
  * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
  * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
  * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
  * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
  * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
  * CONTRACT ,STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
  * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
  * DAMAGE.
**/

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Iodine.Runtime
{
	[IodineBuiltinModule ("io")]
	public class IOModule : IodineModule
	{
		class IodineDirectory : IodineObject
		{
			public readonly static IodineTypeDefinition DirectoryTypeDef = new IodineTypeDefinition ("Directory");

			public IodineDirectory ()
				: base (DirectoryTypeDef)
			{
				SetAttribute ("separator", new IodineString (Path.DirectorySeparatorChar.ToString ()));
				SetAttribute ("getFiles", new BuiltinMethodCallback (listFiles, this));
				SetAttribute ("getDirectories", new BuiltinMethodCallback (listDirectories, this));
				SetAttribute ("remove", new BuiltinMethodCallback (remove, this));
				SetAttribute ("removeTree", new BuiltinMethodCallback (removeTree, this));
				SetAttribute ("exists", new BuiltinMethodCallback (exists, this));
				SetAttribute ("create", new BuiltinMethodCallback (create, this));
				SetAttribute ("copy", new BuiltinMethodCallback (copy, this));
			}

			/**
			 * Iodine Function: Directory.getFiles (dir)
			 * Description: Returns a list of all files in dir
			 */
			private IodineObject listFiles (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args [0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				if (!Directory.Exists (args [0].ToString ())) {
					vm.RaiseException (new IodineIOException ("Directory '" + args [0].ToString () +
					"' does not exist!"));
					return null;
				}

				IodineList ret = new IodineList (new IodineObject[]{ });

				foreach (string dir in Directory.GetFiles (args[0].ToString ())) {
					ret.Add (new IodineString (dir));
				}
				return ret;
			}

			/**
			 * Iodine Function: Directory.getDirectories (dir)
			 * Description: Returns a list of all directories in dir
			 */
			private IodineObject listDirectories (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args [0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				if (!Directory.Exists (args [0].ToString ())) {
					vm.RaiseException (new IodineIOException ("Directory '" + args [0].ToString () +
						"' does not exist!")
					);
					return null;
				}

				IodineList ret = new IodineList (new IodineObject[]{ });

				foreach (string dir in Directory.GetDirectories (args[0].ToString ())) {
					ret.Add (new IodineString (dir));
				}
				return ret;
			}

			/**
			 * Iodine Function: Directory.remove (dir)
			 * Description: Removes a directory
			 */
			private IodineObject remove (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args [0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				if (!Directory.Exists (args [0].ToString ())) {
					vm.RaiseException (new IodineIOException ("Directory '" + args [0].ToString () +
					"' does not exist!"));
					return null;
				}

				Directory.Delete (args [0].ToString ());

				return null;
			}

			/**
			 * Iodine Function: Directory.removeTree (dir)
			 * Description: Removes a directory and all associated sub directories
			 */
			private IodineObject removeTree (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args [0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				if (!Directory.Exists (args [0].ToString ())) {
					vm.RaiseException (new IodineIOException ("Directory '" + args [0].ToString () +
					"' does not exist!"));
					return null;
				}

				rmDir (args [0].ToString ());

				return null;
			}

			/**
			 * Iodine Function: Directory.exists (dir)
			 * Description: Returns true if dir exists
			 */
			private IodineObject exists (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args [0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				return IodineBool.Create (Directory.Exists (args [0].ToString ()));
			}

			/**
			 * Iodine Function: Directory.create (dir)
			 * Description: Creates directory dir
			 */
			private IodineObject create (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args [0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}
				Directory.CreateDirectory (args [0].ToString ());
				return null;
			}

			private IodineObject copy (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
				if (args.Length <= 1) {
					vm.RaiseException (new IodineArgumentException (2));
					return null;
				}

				if (!(args [0] is IodineString) || !(args [1] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}
				bool recurse = false;
				if (args.Length >= 3) {
					if (!(args [2] is IodineBool)) {
						vm.RaiseException (new IodineTypeException ("Bool"));
						return null;
					}
					recurse = ((IodineBool)args [2]).Value;
				}
				bool res = copyDir (args [0].ToString (), args [1].ToString (), recurse);
				if (!res) {
					vm.RaiseException (new IodineIOException ("Could not find directory " + args [0].ToString ()));
				}
				return null;
			}

			private static bool copyDir (string src, string dest, bool recurse)
			{
				DirectoryInfo dir = new DirectoryInfo (src);
				DirectoryInfo[] dirs = dir.GetDirectories ();

				if (!dir.Exists) {
					return false;
				}

				if (!Directory.Exists (dest)) {
					Directory.CreateDirectory (dest);
				}

				FileInfo[] files = dir.GetFiles ();
				foreach (FileInfo file in files) {
					string temppath = Path.Combine (dest, file.Name);
					file.CopyTo (temppath, false);
				}

				if (recurse) {
					foreach (DirectoryInfo subdir in dirs) {
						string temppath = Path.Combine (dest, subdir.Name);
						copyDir (subdir.FullName, temppath, recurse);
					}
				}
				return true;
			}

			private static bool rmDir (string target)
			{
				DirectoryInfo dir = new DirectoryInfo (target);
				DirectoryInfo[] dirs = dir.GetDirectories ();

				if (!dir.Exists) {
					return false;
				}

				FileInfo[] files = dir.GetFiles ();
				foreach (FileInfo file in files) {
					string temppath = Path.Combine (target, file.Name);
					File.Delete (temppath);
				}

				foreach (DirectoryInfo subdir in dirs) {
					string temppath = Path.Combine (target, subdir.Name);
					rmDir (temppath);
				}
				Directory.Delete (target);
				return true;
			}
		}


		class IodineFile : IodineObject
		{
			public readonly static IodineTypeDefinition FileTypeDef = new IodineTypeDefinition ("File");

			public IodineFile ()
				: base (FileTypeDef)
			{
				SetAttribute ("join", new BuiltinMethodCallback (join, this));
				SetAttribute ("remove", new BuiltinMethodCallback (remove, this));
				SetAttribute ("exists", new BuiltinMethodCallback (exists, this));
				SetAttribute ("getNameWithoutExt", new BuiltinMethodCallback (getNameWithoutExt, this));
				SetAttribute ("getName", new BuiltinMethodCallback (getName, this));
				SetAttribute ("copy", new BuiltinMethodCallback (copy, this));
			}

			private IodineObject join (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
				string[] paths = args.Select (p => p.ToString ()).ToArray ();
				return new IodineString (Path.Combine (paths));
			}

			private IodineObject remove (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args [0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				if (!File.Exists (args [0].ToString ())) {
					vm.RaiseException (new IodineIOException ("File '" + args [0].ToString () +
					"' does not exist!"));
					return null;
				}

				File.Delete (args [0].ToString ());

				return null;
			}

			private IodineObject exists (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args [0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				return IodineBool.Create (File.Exists (args [0].ToString ()));
			}

			private IodineObject getNameWithoutExt (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args [0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}
				return new IodineString (Path.GetFileNameWithoutExtension (args [0].ToString ()));
			}

			private IodineObject getName (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args [0] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}
				return new IodineString (Path.GetFileName (args [0].ToString ()));
			}

			private IodineObject copy (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}

				if (!(args [0] is IodineString) || !(args [1] is IodineString)) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				if (!File.Exists (args [0].ToString ())) {
					vm.RaiseException (new IodineIOException ("File '" + args [0].ToString () +
					"' does not exist!"));
					return null;
				}

				File.Copy (args [0].ToString (), args [1].ToString (), true);
				return null;
			}
		}

		public IOModule ()
			: base ("io")
		{
			SetAttribute ("Directory", new IodineDirectory ());
			SetAttribute ("File", new IodineFile ());
			SetAttribute ("exists", new BuiltinMethodCallback (exists, this));
			SetAttribute ("isDir", new BuiltinMethodCallback (isDir, this));
			SetAttribute ("isFile", new BuiltinMethodCallback (isFile, this));
			SetAttribute ("mkdir", new BuiltinMethodCallback (mkdir, this));
			SetAttribute ("rmdir", new BuiltinMethodCallback (rmdir, this));
			SetAttribute ("rmtree", new BuiltinMethodCallback (rmtree, this));
			SetAttribute ("getCreationTime", new BuiltinMethodCallback (getModifiedTime, this));
			SetAttribute ("getModifiedTime", new BuiltinMethodCallback (getCreationTime, this));
		}

		/**
		 * Iodine Function: exists (path)
		 * Description: Returns true if path is a valid file on the disk
		 */
		private IodineObject exists (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineString path = args [0] as IodineString;

			if (path == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			return IodineBool.Create (Directory.Exists (path.Value) || File.Exists (path.Value));
		}

		/**
		 * Iodine Function: isDir (path)
		 * Description: Returns true if path is a valid directory
		 */
		private IodineObject isDir (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineString path = args [0] as IodineString;

			if (path == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			return IodineBool.Create (Directory.Exists (path.Value));
		}

		/**
		 * Iodine Function: isFile (path)
		 * Description: Returns true if path is a valid file
		 */
		private IodineObject isFile (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineString path = args [0] as IodineString;

			if (path == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			return IodineBool.Create (File.Exists (path.Value));
		}

		/**
		 * Iodine Function: mkdir (dir)
		 * Description: Creates directory dir
		 */
		private IodineObject mkdir (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			if (!(args [0] is IodineString)) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}
			Directory.CreateDirectory (args [0].ToString ());
			return null;
		}

		/**
		 * Iodine Function: rmdir (dir)
		 * Description: Removes an empty directory
		 */
		private IodineObject rmdir (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineString path = args [0] as IodineString;

			if (path == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			if (!Directory.Exists (path.Value)) {
				vm.RaiseException (new IodineIOException ("Directory '" + args [0].ToString () +
					"' does not exist!"
				));
				return null;
			}

			Directory.Delete (path.Value);

			return null;
		}

		/**
		 * Iodine Function: rmtree (dir)
		 * Description: Removes an directory, deleting all subfiles
		 */
		private IodineObject rmtree (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			vm.Context.Warn (WarningType.DeprecationWarning, "The io module has been deprecated. Use os or fsutils");
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineString path = args [0] as IodineString;

			if (path == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			if (!Directory.Exists (path.Value)) {
				vm.RaiseException (new IodineIOException ("Directory '" + args [0].ToString () +
					"' does not exist!"
				));
				return null;
			}

			RemoveRecursive (path.Value);

			return null;
		}

		private static bool RemoveRecursive (string target)
		{
			DirectoryInfo dir = new DirectoryInfo (target);
			DirectoryInfo[] dirs = dir.GetDirectories ();

			if (!dir.Exists) {
				return false;
			}

			FileInfo[] files = dir.GetFiles ();
			foreach (FileInfo file in files) {
				string temppath = Path.Combine (target, file.Name);
				File.Delete (temppath);
			}

			foreach (DirectoryInfo subdir in dirs) {
				string temppath = Path.Combine (target, subdir.Name);
				RemoveRecursive (temppath);
			}
			Directory.Delete (target);
			return true;
		}

		/**
		 * Iodine Function: getModifiedTime (dir)
		 * Description: Removes an directory, deleting all subfiles
		 */
		private IodineObject getModifiedTime (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			if (!(args [0] is IodineString)) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;

			}
			if (!File.Exists (args [0].ToString ())) {
				vm.RaiseException (new IodineIOException ("File '" + args [0].ToString () +
				"' does not exist!"));
				return null;
			}
			return new DateTimeModule.IodineTimeStamp (File.GetLastAccessTime (args [0].ToString ()));
		}

		private IodineObject getCreationTime (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			if (!(args [0] is IodineString)) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}
			if (!File.Exists (args [0].ToString ())) {
				vm.RaiseException (new IodineIOException ("File '" + args [0].ToString () +
				"' does not exist!"));
				return null;
			}
			return new DateTimeModule.IodineTimeStamp (File.GetCreationTime (args [0].ToString ()));
		}
	}
}

