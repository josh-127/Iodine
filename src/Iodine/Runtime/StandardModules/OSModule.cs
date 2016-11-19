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
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace Iodine.Runtime
{
    [BuiltinDocString (
        "Provides a portable way for interacting with the host operating system"
    )]
    [IodineBuiltinModule ("os")]
    public class OSModule : IodineModule
    {
        /// <summary>
        /// IodineProc, a process on the host operating system
        /// </summary>
        class IodineProc : IodineObject
        {
            // Note: Having this much nesting really bothers me, but, I can't
            // really think of a better way to do this. Anonymous classes would
            // be a cool thing in C# for singletons
            class ProcTypeDef : IodineTypeDefinition
            {
                public ProcTypeDef ()
                    : base ("Process")
                {
                    BindAttributes (this);
                }

                public override IodineObject BindAttributes (IodineObject obj)
                {
                    obj.SetAttribute ("kill", new BuiltinMethodCallback (Kill, obj));
                    return obj;
                }

                [BuiltinDocString ("Attempts to kill the associated process.")]
                private static IodineObject Kill (
                    VirtualMachine vm,
                    IodineObject self,
                    IodineObject[] args)
                {
                    IodineProc procObj = self as IodineProc;

                    procObj.Value.Kill ();

                    return null;
                }
            }

            public static new readonly IodineTypeDefinition TypeDef = new ProcTypeDef ();

            public readonly Process Value;

            public IodineProc (Process proc)
                : base (TypeDef)
            {
                Value = proc;
                SetAttribute ("id", new IodineInteger (proc.Id));
                SetAttribute ("name", new IodineString (proc.ProcessName));
            }
        }

        /// <summary>
        /// Subprocess, a process spawned by iodine
        /// </summary>
        class IodineSubprocess : IodineObject
        {
            class SubProcessTypeDef : IodineTypeDefinition
            {
                public SubProcessTypeDef ()
                    : base ("Subprocess")
                {
                    BindAttributes (this);
                }

                public override IodineObject BindAttributes (IodineObject obj)
                {
                    obj.SetAttribute ("write", new BuiltinMethodCallback (Write, obj));
                    obj.SetAttribute ("writeln", new BuiltinMethodCallback (Writeln, obj));
                    obj.SetAttribute ("read", new BuiltinMethodCallback (Read, obj));
                    obj.SetAttribute ("readln", new BuiltinMethodCallback (Readln, obj));
                    obj.SetAttribute ("kill", new BuiltinMethodCallback (Kill, obj));
                    obj.SetAttribute ("empty", new BuiltinMethodCallback (Empty, obj));
                    obj.SetAttribute ("alive", new BuiltinMethodCallback (Alive, obj));
                    return base.BindAttributes (obj);
                }

                [BuiltinDocString (
                    "@param *args" +
                    "Writes each string passed in *args to the process's standard input stream"
                )]
                private IodineObject Write (VirtualMachine vm, IodineObject self, IodineObject[] args)
                {
                    IodineSubprocess proc = self as IodineSubprocess;

                    if (proc == null) {
                        vm.RaiseException (new IodineTypeException (TypeDefinition.Name));
                        return null;
                    }

                    foreach (IodineObject obj in args) {
                        IodineString str = obj as IodineString;

                        if (str == null) {
                            vm.RaiseException (new IodineTypeException ("Str"));
                            return null;
                        }

                        proc.StdinWriteString (vm, str.Value);
                    }
                    return null;
                }

                [BuiltinDocString (
                    "@param *args.",
                    "Writes each string passed in *args to the process's standard input stream, and ",
                    "appends a new line."
                )]
                private IodineObject Writeln (VirtualMachine vm, IodineObject self, IodineObject[] args)
                {
                    IodineSubprocess proc = self as IodineSubprocess;

                    if (proc == null) {
                        vm.RaiseException (new IodineTypeException (TypeDefinition.Name));
                        return null;
                    }

                    foreach (IodineObject obj in args) {
                        IodineString str = obj as IodineString;

                        if (str == null) {
                            vm.RaiseException (new IodineTypeException ("Str"));
                            return null;
                        }

                        proc.StdinWriteString (vm, str.Value + "\n");

                    }
                    return null;
                }

                [BuiltinDocString (
                    "Reads a single line from the process's standard output stream."
                )]
                private IodineObject Readln (VirtualMachine vm, IodineObject self, IodineObject[] args)
                {
                    IodineSubprocess proc = self as IodineSubprocess;

                    if (proc == null) {
                        vm.RaiseException (new IodineTypeException (TypeDefinition.Name));
                        return null;
                    }

                    return new IodineString (proc.Value.StandardOutput.ReadLine ());
                }

                [BuiltinDocString (
                    "Reads all text written to the process's standard output stream."
                )]
                private IodineObject Read (VirtualMachine vm, IodineObject self, IodineObject[] args)
                {
                    IodineSubprocess proc = self as IodineSubprocess;

                    if (proc == null) {
                        vm.RaiseException (new IodineTypeException (TypeDefinition.Name));
                        return null;
                    }

                    return new IodineString (proc.Value.StandardOutput.ReadToEnd ());
                }

                [BuiltinDocString ("Attempts to kill the associated process.")]
                private static IodineObject Kill (
                    VirtualMachine vm,
                    IodineObject self,
                    IodineObject[] args)
                {
                    IodineSubprocess procObj = self as IodineSubprocess;

                    if (procObj == null) {
                        vm.RaiseException (new IodineTypeException (TypeDefinition.Name));
                        return null;
                    }
                        
                    procObj.Value.Kill ();

                    return null;
                }

                [BuiltinDocString ("Returns true if the process is alive.")]
                private IodineObject Alive (VirtualMachine vm, IodineObject self, IodineObject[] args)
                {
                    IodineSubprocess proc = self as IodineSubprocess;

                    if (proc == null) {
                        vm.RaiseException (new IodineTypeException (TypeDefinition.Name));
                        return null;
                    }

                    return IodineBool.Create (proc.Value.HasExited);
                }

                [BuiltinDocString ("Returns true if there is no more data to be read from stdout.")]
                private IodineObject Empty (VirtualMachine vm, IodineObject self, IodineObject[] args)
                {
                    IodineSubprocess proc = self as IodineSubprocess;

                    if (proc == null) {
                        vm.RaiseException (new IodineTypeException (TypeDefinition.Name));
                        return null;
                    }

                    return IodineBool.Create (proc.Value.StandardOutput.Peek () < 0);
                }
            }

            public static new IodineTypeDefinition TypeDef = new SubProcessTypeDef ();

            public readonly Process Value;

            private bool canRead;
            private bool canWrite;

            public IodineSubprocess (Process proc, bool read, bool write)
                : base (TypeDef)
            {
                canRead = read;
                canWrite = write;
                Value = proc;
                SetAttribute ("id", new IodineInteger (proc.Id));
                SetAttribute ("name", new IodineString (proc.ProcessName));
            }

            public override void Exit (VirtualMachine vm)
            {
                if (canRead) {
                    Value.StandardOutput.Close ();
                    Value.StandardError.Close ();
                }

                if (canWrite) {
                    Value.StandardInput.Close ();
                }
            }

            private void StdinWriteString (VirtualMachine vm, string str)
            {
                Value.StandardInput.Write (str);
            }
        }

        public OSModule ()
            : base ("os")
        {
            SetAttribute ("USER_DIR", new IodineString (
                Environment.GetFolderPath (Environment.SpecialFolder.UserProfile))
            );
            SetAttribute ("ENV_SEP", new IodineString (Path.PathSeparator.ToString ()));
            SetAttribute ("SEEK_SET", new IodineInteger (IodineStream.SEEK_SET));
            SetAttribute ("SEEK_CUR", new IodineInteger (IodineStream.SEEK_CUR));
            SetAttribute ("SEEK_END", new IodineInteger (IodineStream.SEEK_END));

            SetAttribute ("getEnv", new BuiltinMethodCallback (GetEnv, this)); // DEPRECATED
            SetAttribute ("setEnv", new BuiltinMethodCallback (SetEnv, this)); // DEPRECATED
            SetAttribute ("putenv", new BuiltinMethodCallback (SetEnv, this));
            SetAttribute ("getenv", new BuiltinMethodCallback (GetEnv, this));
            SetAttribute ("getcwd", new BuiltinMethodCallback (GetCwd, this));
            SetAttribute ("setcwd", new BuiltinMethodCallback (SetCwd, this));
            SetAttribute ("getlogin", new BuiltinMethodCallback (GetUsername, this));
            SetAttribute ("call", new BuiltinMethodCallback (Call, this));
            SetAttribute ("spawn", new BuiltinMethodCallback (Spawn, this));
            SetAttribute ("popen", new BuiltinMethodCallback (Popen, this));
            SetAttribute ("system", new BuiltinMethodCallback (System, this));
            SetAttribute ("procs", new BuiltinMethodCallback (GetProcList, this));
            SetAttribute ("unlink", new BuiltinMethodCallback (Unlink, this));
            SetAttribute ("mkdir", new BuiltinMethodCallback (Mkdir, this));
            SetAttribute ("rmdir", new BuiltinMethodCallback (Rmdir, this));
            SetAttribute ("rmtree", new BuiltinMethodCallback (Rmtree, this));
            SetAttribute ("list", new BuiltinMethodCallback (List, this));
        }

        [BuiltinDocString ("Returns a list of processes running on the machine.")]
        private IodineObject GetProcList (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            IodineList list = new IodineList (new IodineObject[] { });
            foreach (Process proc in Process.GetProcesses ()) {
                list.Add (new IodineProc (proc));
            }
            return list;
        }

        [BuiltinDocString ("Returns the login name of the current user.")]
        private IodineObject GetUsername (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            return new IodineString (Environment.UserName);
        }

        [BuiltinDocString ("Returns the current working directory.")]
        private IodineObject GetCwd (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            return new IodineString (Environment.CurrentDirectory);
        }

        [BuiltinDocString (
            "Sets the current working directory.",
            "@param cwd The new current working directory."
        )]
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

        [BuiltinDocString (
            "Returns the value of an environmental variable.",
            "@param env The name of the environmental variable."
        )]
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

        [BuiltinDocString (
            "Sets an environmental variable to a specified value",
            "@param env The name of the environmental variable.",
            "@param value The value to set the environmental variable."
        )]
        private IodineObject SetEnv (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 2) {
                vm.RaiseException (new IodineArgumentException (2));
            }

            IodineString str = args [0] as IodineString;

            if (str == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }

            Environment.SetEnvironmentVariable (str.Value, args [1].ToString (), EnvironmentVariableTarget.User);
            return null;
        }

        [BuiltinDocString (
            "Spawns a new process.",
            "@param executable The executable to run",
            "@param [args] Command line arguments",
            "@param [wait] Should we wait to exit"
        )]
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

            return new IodineProc (Process.Start (info));
        }
         
        [BuiltinDocString (
            "Executes program, waiting for it to exit and returning its exit code.",
            "@param executable The executable to run.",
            "@param [args] Command line arguments.",
            "@param [useShell] Should we use a shell."
        )]
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
            
        [BuiltinDocString (
            "Opens up a new process, returning a Proc object.",
            "@param commmand Command to run.",
            "@param mode Mode to open up the process in ('r' or 'w')."
        )]
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
                return new IodineSubprocess (Popen_Win32 (command.Value, read, write), read, write);
            } else {
                Process proc = Popen_Unix (command.Value, read, write);
                proc.Start ();
                return new IodineSubprocess (proc, read, write);
            }

        }

        private Process Popen_Win32 (string command, bool read, bool write)
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
            return proc;
        }

        private Process Popen_Unix (string command, bool read, bool write)
        {
            string args = String.Format ("-c \"{0}\"", command);
            ProcessStartInfo info = new ProcessStartInfo ("/bin/sh", args);
            info.UseShellExecute = false;
            info.RedirectStandardOutput = read;
            info.RedirectStandardError = read;
            info.RedirectStandardInput = write;
            Process proc = new Process ();
            proc.StartInfo = info;
            return proc;
        }

        [BuiltinDocString (
            "Executes a command using the default shell.",
            "@param commmand Command to run."
        )]
        private IodineObject System (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }


            IodineString command = args [0] as IodineString;

            if (command == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }

            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT
                             || Environment.OSVersion.Platform == PlatformID.Win32S
                             || Environment.OSVersion.Platform == PlatformID.Win32Windows
                             || Environment.OSVersion.Platform == PlatformID.WinCE
                             || Environment.OSVersion.Platform == PlatformID.Xbox;


            Process proc = null;

            if (isWindows) {
                proc = Popen_Win32 (command.Value, false, false);
            } else {
                proc = Popen_Unix (command.Value, false, false);
            }

            proc.Start ();

            proc.WaitForExit ();

            return new IodineInteger (proc.ExitCode);
        }

        [BuiltinDocString (
            "Removes a file from the filesystem.",
            "@param path The file to delete."
        )]
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

        [BuiltinDocString (
            "Creates a new directory.",
            "@param path The directory to create."
        )]
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

        [BuiltinDocString (
            "Removes an empty directory.",
            "@param path The directory to remove."
        )]
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

        [BuiltinDocString (
            "Removes an directory, deleting all subfiles.",
            "@param path The directory to remove."
        )]
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

        [BuiltinDocString (
            "Returns a list of all subfiles in a directory.",
            "@param path The directory to list."
        )]
        private IodineObject List (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineString dir = args [0] as IodineString;

            if (dir == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }

            if (!Directory.Exists (dir.Value)) {
                vm.RaiseException (new IodineIOException ("Directory does not exist"));
                return null;
            }

            List<string> items = new List<string> ();

            items.AddRange (Directory.GetFiles (dir.Value));
            items.AddRange (Directory.GetDirectories (dir.Value));

            IodineList retList = new IodineList (new IodineObject[] { });

            items.ForEach (p => retList.Add (new IodineString (p)));

            return retList;
        }
    }
}

