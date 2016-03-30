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
using Iodine.Compiler;
using Iodine.Runtime;
using Iodine.Interop;

namespace Iodine
{
	public sealed class ReplShell
	{

		public ReplShell (IodineContext context)
		{
		}

		public void Run ()
		{
			var version = Assembly.GetEntryAssembly ().GetName ().Version;
			Console.WriteLine ("Iodine v{0}-alpha", version.ToString (3));
			Console.WriteLine ("Enter expressions to have them be evaluated");

			IodineContext context = new IodineContext ();
			while (true) {
				Console.Write (">>> ");
				var source = Console.ReadLine ();
				try {
					if (source.Length > 0) {
						SourceUnit unit = SourceUnit.CreateFromSource (source);
						var result = unit.Compile (context);
						Console.WriteLine (context.Invoke (result, new IodineObject[] { }));
					}
				} catch (UnhandledIodineExceptionException ex) {
					Console.Error.WriteLine ("*** {0}", ex.OriginalException.GetAttribute ("message"));
					ex.PrintStack ();
					Console.Error.WriteLine ();
				} catch (ModuleNotFoundException ex) {
					Console.Error.WriteLine (ex.ToString ());
				} catch (SyntaxException syntaxException) {
					DisplayErrors (syntaxException.ErrorLog);
				} catch (Exception ex) {
					Console.Error.WriteLine ("Fatal exception has occured!");
					Console.Error.WriteLine (ex.Message);
					Console.Error.WriteLine ("Stack trace: \n{0}", ex.StackTrace);
					//Console.Error.WriteLine ("\nIodine stack trace \n{0}", engine.VirtualMachine.GetStackTrace ());
				}
			}
		}

		public static void DisplayErrors (ErrorSink errorLog)
		{
			foreach (Error err in errorLog) {
				SourceLocation loc = err.Location;
				Console.Error.WriteLine ("{0} ({1}:{2}) error: {3}", Path.GetFileName (loc.File),
					loc.Line, loc.Column, err.Text);
			}
		}
	}
}

