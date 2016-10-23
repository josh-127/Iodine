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
using System.Net;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using Iodine.Compiler;
using Iodine.Runtime;
using Iodine.Runtime.Debug;

namespace Iodine
{
    public class IodineEntry
    {
        class IodineOptions
        {
            public readonly List<string> Options = new List<string> ();

            public string FileName { get; set; }

            public IodineList IodineArguments { private set; get; }

            public static IodineOptions Parse (string[] args)
            {
                IodineOptions ret = new IodineOptions ();
                int i;
                for (i = 0; i < args.Length; i++) {
                    if (args [i].StartsWith ("-")) {
                        ret.Options.Add (args [i].Substring (1));
                    } else {
                        ret.FileName = args [i++];
                        if (!File.Exists (ret.FileName)) {
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
                ret.IodineArguments = new IodineList (arguments);
                return ret;
            }
        }

        private static IodineContext context;

        public static void Main (string[] args)
        {
            context = IodineContext.Create ();
            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => {
                if (e.ExceptionObject is UnhandledIodineExceptionException) {
                    HandleIodineException (e.ExceptionObject as UnhandledIodineExceptionException);
                }
            };
            if (args.Length == 0) {
                ReplShell shell = new ReplShell (context);
                shell.Run ();
                DisplayUsage ();
                Environment.Exit (0);
            }

            IodineOptions options = IodineOptions.Parse (args);

            options.Options.ForEach (p => ParseOption (context, p));

            try {
                SourceUnit code = SourceUnit.CreateFromFile (options.FileName);
                IodineModule module = code.Compile (context);
                context.Invoke (module, new IodineObject[] { });
                if (module.HasAttribute ("main")) {
                    context.Invoke (module.GetAttribute ("main"), new IodineObject[] {
                        options.IodineArguments
                    });
                }

            } catch (UnhandledIodineExceptionException ex) {
                HandleIodineException (ex);
            } catch (SyntaxException ex) {
                DisplayErrors (ex.ErrorLog, options.FileName);
                Panic ("Compilation failed with {0} errors!", ex.ErrorLog.ErrorCount);
            } catch (ModuleNotFoundException ex) {
                Console.Error.WriteLine (ex.ToString ());
                Panic ("Program terminated.");
            } catch (Exception e) {
                Console.Error.WriteLine ("Fatal exception has occured!");
                Console.Error.WriteLine (e.Message);
                Console.Error.WriteLine ("Stack trace: \n{0}", e.StackTrace);
                Console.Error.WriteLine ("\nIodine stack trace \n{0}",
                    context.VirtualMachine.GetStackTrace ());
                Panic ("Program terminated.");
            }

        }

        private static void HandleIodineException (UnhandledIodineExceptionException ex)
        {
            Console.Error.WriteLine (
                "An unhandled {0} has occured!",
                ex.OriginalException.TypeDef.Name
            );

            Console.Error.WriteLine (
                "\tMessage: {0}",
                ex.OriginalException.GetAttribute ("message").ToString ()
            );

            Console.WriteLine ();
            ex.PrintStack ();
            Console.Error.WriteLine ();
            Panic ("Program terminated.");
        }

        private static void ParseOption (IodineContext context, string option)
        {
            switch (option) {
            case "version":
                DisplayInfo ();
                break;
            case "help":
                DisplayUsage ();
                break;
            case "debug":
                RunDebugServer ();
                break;
            default:
                Panic ("Unknown command line option: '{0}'", option);
                break;
            }
        }

        private static void RunDebugServer ()
        {
            DebugServer server = new DebugServer (context.VirtualMachine);
            Thread debugThread = new Thread (() => {
                server.Start (new IPEndPoint (IPAddress.Loopback, 6569));
            });
            debugThread.Start ();
            Console.WriteLine ("Debug server listening on 127.0.0.1:6569");
        }

        private static IodineConfiguration LoadConfiguration ()
        {
            if (IsUnix ()) {
                if (File.Exists ("/etc/iodine.conf")) {
                    return IodineConfiguration.Load ("/etc/iodine.conf");
                }
            }
            string exePath = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
            string commonAppData = Environment.GetFolderPath (
                Environment.SpecialFolder.CommonApplicationData
            );
            string appData = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);

            if (File.Exists (Path.Combine (exePath, "iodine.conf"))) {
                return IodineConfiguration.Load (Path.Combine (exePath, "iodine.conf"));
            }

            if (File.Exists (Path.Combine (commonAppData, "iodine.conf"))) {
                return IodineConfiguration.Load (Path.Combine (commonAppData, "iodine.conf"));
            }

            if (File.Exists (Path.Combine (appData, "iodine.conf"))) {
                return IodineConfiguration.Load (Path.Combine (appData, "iodine.conf"));
            }

            return new IodineConfiguration (); // If we can't find a configuration file, load the default
        }

        private static void DisplayInfo ()
        {
            int major = Assembly.GetExecutingAssembly ().GetName ().Version.Major;
            int minor = Assembly.GetExecutingAssembly ().GetName ().Version.Minor;
            int patch = Assembly.GetExecutingAssembly ().GetName ().Version.Build;
            Console.WriteLine ("Iodine v{0}.{1}.{2}-alpha", major, minor, patch);
            Environment.Exit (0);
        }

        private static void DisplayErrors (ErrorSink errorLog, string filePath)
        {
            string[] lines = File.ReadAllLines (filePath);

            foreach (Error err in errorLog) {
                SourceLocation loc = err.Location;
                Console.Error.WriteLine ("{0} ({1}:{2}) error ID{3:d4}: {4}",
                    Path.GetFileName (loc.File),
                    loc.Line,
                    loc.Column,
                    (int)err.ErrorID,
                    err.Text
                );

                string source = lines [loc.Line];

                Console.Error.WriteLine ("    {0}", source);
                Console.Error.WriteLine ("    {0}", "^".PadLeft (loc.Column));
            }
        }

        private static void DisplayUsage ()
        {
            Console.WriteLine ("usage: iodine [option] ... [file] [arg] ...");
            Environment.Exit (0);
        }

        private static void Panic (string format, params object[] args)
        {
            Console.Error.WriteLine (format, args);
            Environment.Exit (-1);
        }

        public static bool IsUnix ()
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
    }
}
