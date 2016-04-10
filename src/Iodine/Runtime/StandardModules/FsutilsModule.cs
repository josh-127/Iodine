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
	[IodineBuiltinModule ("fsutils")]
	public class FSUtilsModule : IodineModule
	{
		public FSUtilsModule ()
			: base ("fsutils")
		{
			SetAttribute ("copy", new BuiltinMethodCallback (Copy, this));
			SetAttribute ("copytree", new BuiltinMethodCallback (Exists, this));
			SetAttribute ("exists", new BuiltinMethodCallback (Exists, this));
			SetAttribute ("isDir", new BuiltinMethodCallback (IsDir, this));
			SetAttribute ("isFile", new BuiltinMethodCallback (IsFile, this));;
			SetAttribute ("getCreationTime", new BuiltinMethodCallback (GetModifiedTime, this));
			SetAttribute ("getModifiedTime", new BuiltinMethodCallback (GetCreationTime, this));
		}

		/**
		 * Iodine Function: copy (src, dest)
		 * Description: Copies a file 
		 */
		private IodineObject Copy (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}

			IodineString src = args [0] as IodineString;
			IodineString dest = args [1] as IodineString;

			if (dest == null || src == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			if (File.Exists (src.Value)) {
				File.Copy (src.Value, dest.Value, true);
			} else if (Directory.Exists (src.Value)) {
				CopyDir (src.Value, dest.Value, false);
			} else {
				vm.RaiseException (new IodineIOException ("File does not exist"));
			}
			return null;
		}

		/**
		 * Iodine Function: copytree (src, dest)
		 * Description: Copies a directory and its contents
		 */
		private IodineObject Copytree (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}

			IodineString src = args [0] as IodineString;
			IodineString dest = args [1] as IodineString;

			if (dest == null || src == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			CopyDir (src.Value, dest.Value, true);

			return null;
		}

		/**
		 * Iodine Function: exists (path)
		 * Description: Returns true if path is a valid file on the disk
		 */
		private IodineObject Exists (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
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
		private IodineObject IsDir (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
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
		private IodineObject IsFile (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
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
		 * Iodine Function: getModifiedTime (dir)
		 * Description: Removes an directory, deleting all subfiles
		 */
		private IodineObject GetModifiedTime (VirtualMachine vm, IodineObject self, IodineObject[] args)
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

		private IodineObject GetCreationTime (VirtualMachine vm, IodineObject self, IodineObject[] args)
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

		private static bool CopyDir (string src, string dest, bool recurse)
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
					CopyDir (subdir.FullName, temppath, recurse);
				}
			}
			return true;
		}
	}
}

