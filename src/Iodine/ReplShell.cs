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
using System.Reflection;
using Iodine.Runtime;

namespace Iodine
{
	public class ReplShell
	{
		private IodineEngine engine = new IodineEngine ();

		public void Run ()
		{
			int major = Assembly.GetExecutingAssembly ().GetName ().Version.Major;
			int minor = Assembly.GetExecutingAssembly ().GetName ().Version.Minor;
			int patch = Assembly.GetExecutingAssembly ().GetName ().Version.Build;
			Console.WriteLine ("Iodine v{0}.{1}.{2}", major, minor, patch);
			Console.WriteLine ("Enter expressions to have them be evaluated");
			engine ["prompt"] = ">>> ";
			while (true) {
				Console.Write (engine ["prompt"].ToString ());
				string source = Console.ReadLine ().Trim ();
				try {
					if (source.Length > 0) {
						Console.WriteLine (engine.DoString (source).ToString ());
					}
				} catch (UnhandledIodineExceptionException ex) {
					Console.Error.WriteLine ("An unhandled {0} has occured!", ex.OriginalException.TypeDef.Name);
					Console.Error.WriteLine ("\tMessage: {0}", ex.OriginalException.Message);
					Console.WriteLine ();
					ex.PrintStack ();
					Console.Error.WriteLine ();
				} catch (SyntaxException syntaxException) {
					DisplayErrors (syntaxException.ErrorLog);
				} catch (Exception ex) {
					Console.Error.WriteLine ("Fatal exception has occured!");
					Console.Error.WriteLine (ex.Message);
					Console.Error.WriteLine ("Stack trace: \n{0}", ex.StackTrace);
					Console.Error.WriteLine ("\nIodine stack trace \n{0}", engine.VirtualMachine.Stack.Trace ());
				}
			}
		}

		public static void DisplayErrors (ErrorLog errorLog)
		{
			foreach (Error err in errorLog) {
				Location loc = err.Location;
				Console.Error.WriteLine ("{0} ({1}:{2}) error: {3}", Path.GetFileName (loc.File),
					loc.Line, loc.Column, err.Text);
			}
		}
	}
}
