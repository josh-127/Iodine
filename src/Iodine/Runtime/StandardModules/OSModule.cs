﻿/**
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
using System.Text;
using System.Diagnostics;

namespace Iodine.Runtime
{
	[IodineBuiltinModule ("os")]
	public class OSModule : IodineModule
	{
		class IodineProc : IodineObject
		{
			public static readonly IodineTypeDefinition ProcTypeDef = new IodineTypeDefinition ("Process");

			public readonly Process Value;

			public IodineProc (Process proc)
				: base (ProcTypeDef)
			{
				Value = proc;
				//SetAttribute ("id", new IodineInteger (proc.Id));
				//SetAttribute ("name", new IodineString (proc.ProcessName));
				SetAttribute ("kill", new BuiltinMethodCallback (kill, this));
			}

			private IodineObject kill (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				Value.Kill ();
				return null;
			}

		}

		class IodineSubprocess : IodineProc {

			private bool _started = false;

			public IodineSubprocess (Process proc, bool read, bool write)
				: base (proc)
			{
				if (!read) {
					StartIfNotStarted ();
				}
			}

			public override IodineObject RightShift (VirtualMachine vm, IodineObject right)
			{
				IodineStream stream = right as IodineStream;

				if (stream == null) {
					vm.RaiseException (new IodineTypeException ("Stream"));
					return null;
				}

				Value.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
					stream.File.Write (System.Text.Encoding.ASCII.GetBytes (e.Data), 0, e.Data.Length);
				};

				Value.Start ();

				Value.WaitForExit ();
				return null;
			}

			public override IodineObject LeftShift (VirtualMachine vm, IodineObject right)
			{
				if (right is IodineStream) {
					StdinWriteFile (vm, right as IodineStream);
				} else if (right is IodineString) {
					StdinWriteString (vm, right as IodineString);
				}
				return null;
			}
				
			private void StdinWriteFile (VirtualMachine vm, IodineStream stream)
			{
				StartIfNotStarted ();
				if (stream == null) {
					vm.RaiseException (new IodineTypeException ("Stream"));
				}

				while (stream.File.Position < stream.File.Length) {
					Value.StandardInput.Write ((char)stream.File.ReadByte ());
				}
			}

			private void StdinWriteString (VirtualMachine vm, IodineString str)
			{
				StartIfNotStarted ();
				Value.StandardInput.Write (str.Value);
			}


			private void StartIfNotStarted ()
			{
				if (!_started) {
					_started = true;
					Value.Start ();
				}
			}
		}

		public OSModule ()
			: base ("os")
		{
			SetAttribute ("USER_DIR", new IodineString (Environment.GetFolderPath (
				Environment.SpecialFolder.UserProfile)));
			SetAttribute ("ENV_SEP", new IodineString (Path.PathSeparator.ToString ()));

			SetAttribute ("getEnv", new BuiltinMethodCallback (GetEnv, this)); // DEPRECATED
			SetAttribute ("setEnv", new BuiltinMethodCallback (SetEnv, this)); // DEPRECATED

			SetAttribute ("putenv", new BuiltinMethodCallback (SetEnv, this));
			SetAttribute ("getenv", new BuiltinMethodCallback (GetEnv, this));
			SetAttribute ("getcwd", new BuiltinMethodCallback (GetCwd, this));
			SetAttribute ("setcwd", new BuiltinMethodCallback (SetCwd, this));
			SetAttribute ("getUsername", new BuiltinMethodCallback (GetUsername, this));
			SetAttribute ("call", new BuiltinMethodCallback (Call, this));
			SetAttribute ("spawn", new BuiltinMethodCallback (Spawn, this));
			SetAttribute ("popen", new BuiltinMethodCallback (Popen, this));
			SetAttribute ("getProcList", new BuiltinMethodCallback (GetProcList, this));
			SetAttribute ("unlink", new BuiltinMethodCallback (Unlink, this));
			SetAttribute ("mkdir", new BuiltinMethodCallback (Mkdir, this));
			SetAttribute ("rmdir", new BuiltinMethodCallback (Rmdir, this));
			SetAttribute ("rmtree", new BuiltinMethodCallback (Rmtree, this));
		}

		/**
		 * Iodine Function: getProcList ()
		 * Description: Returns a list of running processes
		 */
		private IodineObject GetProcList (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineList list = new IodineList (new IodineObject[] { });
			foreach (Process proc in Process.GetProcesses ()) {
				list.Add (new IodineProc (proc));
			}
			return list;
		}

		/**
		 * Iodine Function: getUsername ()
		 * Description: Returns the username of the current user
		 */
		private IodineObject GetUsername (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineString (Environment.UserName);
		}

		/**
		 * Iodine Function: getcwd ();
		 * Description: Gets the current working directory
		 */
		private IodineObject GetCwd (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineString (Environment.CurrentDirectory);
		}

		/**
		 * Iodine Function: setcwd (cwd)
		 * Description: Sets the current working directory
		 */
		private IodineObject SetCwd (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineString cwd = args [0] as IodineString;

			if (cwd == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			Environment.CurrentDirectory = args [0].ToString ();
			return null;
		}

		/**
		 * Iodine Function: getenv (name)
		 * Description: Gets an environmental variable
		 */
		private IodineObject GetEnv (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineString str = args [0] as IodineString;

			if (str == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			if (Environment.GetEnvironmentVariable (str.Value) != null) {
				return new IodineString (Environment.GetEnvironmentVariable (str.Value));
			}

			return null;
		}

		/**
		 * Iodine Function: putenv (name, value)
		 * Description: Sets an environmental variable
		 */
		private IodineObject SetEnv (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException (new IodineArgumentException (2));
			}
			IodineString str = args [0] as IodineString;
			Environment.SetEnvironmentVariable (str.Value, args [1].ToString (), EnvironmentVariableTarget.User);
			return null;
		}
			
		private IodineObject Spawn (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}

			IodineString str = args [0] as IodineString;
			string cmdArgs = "";
			bool wait = true;

			if (str == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			if (args.Length >= 2) {
				IodineString cmdArgsObj = args [1] as IodineString;
				if (cmdArgsObj == null) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}
				cmdArgs = cmdArgsObj.Value;
			}

			if (args.Length >= 3) {
				IodineBool waitObj = args [2] as IodineBool;
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

		/*
		 * Iodine Function: call (program. [arguments, [useShell = false]])
		 * Description: Executes program, waiting for it to exit and returning its exit code
		 */
		private IodineObject Call (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 1) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineString program = args [0] as IodineString;

			string arguments = "";
			bool useShell = false;

			if (program == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			if (args.Length > 1) {

				IodineString argObj = args [1] as IodineString;

				if (argObj == null) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				arguments = argObj.Value;
			}

			if (args.Length > 2) {
				IodineBool useShellObj = args [1] as IodineBool;

				if (useShellObj == null) {
					vm.RaiseException (new IodineTypeException ("Bool"));
					return null;
				}

				useShell = useShellObj.Value;
			}

			ProcessStartInfo info = new ProcessStartInfo ();
			info.FileName = program.Value;
			info.Arguments = arguments;
			info.UseShellExecute = useShell;

			Process proc = Process.Start (info);

			proc.WaitForExit ();

			return new IodineInteger (proc.ExitCode);

		}

		/*
		 * Iodine Function: popen (command, mode)
		 * Description: Executes command, returning a new stream representing the newly
		 * created processes standard input and output
		 */
		private IodineObject Popen (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}
			IodineString command = args [0] as IodineString;
			IodineString mode = args [1] as IodineString;

			if (command == null || mode == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			bool read = false;
			bool write = false;

			foreach (char c in mode.Value) {
				switch (c) {
				case 'r':
					read = true;
					break;
				case 'w':
					write = true;
					break;
				}

			}

			bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT
			                 || Environment.OSVersion.Platform == PlatformID.Win32S
			                 || Environment.OSVersion.Platform == PlatformID.Win32Windows
			                 || Environment.OSVersion.Platform == PlatformID.WinCE
			                 || Environment.OSVersion.Platform == PlatformID.Xbox;

			if (isWindows) {
				return Popen_Win32 (command.Value, read, write);
			} else {
				return Popen_Unix (command.Value, read, write);
			}

		}

		private IodineObject Popen_Win32 (string command, bool read, bool write)
		{
			string systemPath = Environment.GetFolderPath (Environment.SpecialFolder.System);
			string args = String.Format ("/K \"{0}\"", command);
			ProcessStartInfo info = new ProcessStartInfo (Path.Combine (systemPath, "cmd.exe"), args);
			info.UseShellExecute = false;
			info.RedirectStandardOutput = read;
			info.RedirectStandardError = read;
			info.RedirectStandardInput = write;
			Process proc = new Process ();
			proc.StartInfo = info;
			return new IodineSubprocess (proc, read, write);
		}

		private IodineObject Popen_Unix (string command, bool read, bool write)
		{
			string args = String.Format ("-c \"{0}\"", command);
			ProcessStartInfo info = new ProcessStartInfo ("/bin/sh", args);
			info.UseShellExecute = false;
			info.RedirectStandardOutput = read;
			info.RedirectStandardError = read;
			info.RedirectStandardInput = write;
			Process proc = new Process ();
			proc.StartInfo = info;
			return new IodineSubprocess (proc, read, write);
		}

		/**
		* Iodine Function: unlink (file)
		* Description: Removes file
		*/
		private IodineObject Unlink (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineString fileString = args [0] as IodineString;

			if (fileString == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			if (File.Exists (fileString.Value)) {
				File.Delete (fileString.Value);
			} else {
				vm.RaiseException (new IodineIOException ("File not found!"));
				return null;
			}
			return null;
		}

		/**
		* Iodine Function: mkdir (dir)
		* Description: Creates directory dir
		*/
		private IodineObject Mkdir (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
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
		private IodineObject Rmdir (VirtualMachine vm, IodineObject self, IodineObject[] args)
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
		private IodineObject Rmtree (VirtualMachine vm, IodineObject self, IodineObject[] args)
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

			if (!Directory.Exists (path.Value)) {
				vm.RaiseException (new IodineIOException ("Directory '" + args [0].ToString () +
					"' does not exist!"
				));
				return null;
			}

			RemoveRecursive (path.Value);

			return null;
		}

		/*
		 * Recurisively remove a directory
		 */
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
	}
}
