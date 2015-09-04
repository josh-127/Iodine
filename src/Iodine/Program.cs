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
using System.Reflection;
using System.Collections.Generic;
using Iodine.Runtime;

namespace Iodine
{
	public class IodineEntry
	{
		class IodineOptions
		{
			public string FileName {
				get; 
				set;
			}

			public IodineList Arguments { private set; get; }

			public bool ShowVersion { private set; get; }

			public static IodineOptions Parse (string[] args)
			{
				IodineOptions ret = new IodineOptions ();
				int i;
				for (i = 0; i < args.Length; i++) {
					if (args [i].StartsWith ("-")) {
						switch (args [i]) {
						default:
							Panic ("Unknown command line argument '{0}'", args [i]);
							break;
						}
					} else {
						ret.FileName = args [i++];
						if (!System.IO.File.Exists (ret.FileName)) {
							Panic ("Could not find file {0}!", ret.FileName);
						}
						break;
					}
				}
				IodineObject[] arguments = new IodineObject [args.Length - i];
				int start = i;
				for (; i < args.Length; i++) {
					arguments [i - start] = new IodineString (args [i]);
				}
				ret.Arguments = new IodineList (arguments);
				return ret;
			}
		}

		public static void Main (string[] args)
		{
			if (args.Length == 0) {
				ReplShell shell = new ReplShell ();
				shell.Run ();
				Environment.Exit (0);
			}
			IodineOptions options = IodineOptions.Parse (args);
			if (options.ShowVersion) {
				int major = Assembly.GetExecutingAssembly ().GetName ().Version.Major;
				int minor = Assembly.GetExecutingAssembly ().GetName ().Version.Minor;
				int patch = Assembly.GetExecutingAssembly ().GetName ().Version.Build;
				Console.WriteLine ("Iodine v{0}.{1}.{2}", major, minor, patch);
				Environment.Exit (0);
			}

			ErrorLog errorLog = new ErrorLog ();
			IodineModule module = IodineModule.LoadModule (errorLog, options.FileName);

			if (module == null) {
				DisplayErrors (errorLog);
				Panic ("Compilation failed with {0} errors!", errorLog.ErrorCount);
			} else {
				VirtualMachine vm = new VirtualMachine ();
				try {
					module.Invoke (vm, new IodineObject[] { });
					if (module.HasAttribute ("main")) {
						module.GetAttribute ("main").Invoke (vm, new IodineObject[] { options.Arguments });
					}
				} catch (UnhandledIodineExceptionException ex) {
					Console.Error.WriteLine ("An unhandled {0} has occured!", ex.OriginalException.TypeDef.Name);
					Console.Error.WriteLine ("\tMessage: {0}", ex.OriginalException.GetAttribute ("message").ToString ());
					Console.WriteLine ();
					ex.PrintStack ();
					Console.Error.WriteLine ();
					Panic ("Program terminated.");
				} catch (SyntaxException ex) {
					DisplayErrors (ex.ErrorLog);
				} catch (Exception e) {
					Console.Error.WriteLine ("Fatal exception has occured!");
					Console.Error.WriteLine (e.Message);
					Console.Error.WriteLine ("Stack trace: \n{0}", e.StackTrace);
					Console.Error.WriteLine ("\nIodine stack trace \n{0}", vm.Trace ());
					Panic ("Program terminated.");
				}
			}
		}

		private static void DisplayErrors (ErrorLog errorLog)
		{
			foreach (Error err in errorLog) {
				Location loc = err.Location;
				Console.Error.WriteLine ("{0} ({1}:{2}) error: {3}", Path.GetFileName (loc.File),
					loc.Line, loc.Column, err.Text);
			}
		}

		private static void DisplayUsage ()
		{
			Console.WriteLine ("usage: [option] ... [file] [arg] ...");
			Environment.Exit (0);
		}

		private static void Panic (string format, params object[] args)
		{
			Console.Error.WriteLine (format, args);
			Environment.Exit (-1);
		}
	}
}
