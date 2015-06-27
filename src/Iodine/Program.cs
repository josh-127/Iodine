using System;
using System.IO;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineEntry
	{
		class IodineOptions
		{
			public string FileName { get; set; } 

			public bool DisplayAst { set; get; }
			public IodineList Arguments { set; get; }

			public static IodineOptions Parse (string[] args)
			{
				IodineOptions ret = new IodineOptions();
				int i;
				for (i = 0; i < args.Length; i++) {
					if (args[i].StartsWith ("-")) {
						switch (args[i]) {
						default:
							Panic ("Unknown command line argument '{0}'", args[i]);
							break;
						}
					} else {
						ret.FileName = args[i++];
						break;
					}
				}
				IodineObject[] arguments = new IodineObject[args.Length - i];
				int start = i;
				for (; i < args.Length; i++) {
					arguments[i - start] = new IodineString (args[i]);
				}
				ret.Arguments = new IodineList (arguments);
				return ret;
			}
		}

		public static void Main (string[] args)
		{
			IodineOptions options = IodineOptions.Parse (args);
			ErrorLog errorLog = new ErrorLog ();
			IodineModule module = IodineModule.CompileModule (errorLog, options.FileName);

			if (module == null) {
				DisplayErrors (errorLog);
				Panic ("Compilation failed with {0} errors!", errorLog.ErrorCount);
			} else {
				try {
					VirtualMachine vm = new VirtualMachine ();
					module.GetAttribute ("main").Invoke (vm, new IodineObject[] {options.Arguments });
				} catch (UnhandledIodineExceptionException ex) {
					Console.WriteLine ("An unhandled exception has occured!");
					Console.WriteLine ("\tMessage: {0}", ex.OriginalException.Message);
					Panic ("Program terminated.");
				}

			}

		}

		public static void DisplayErrors (ErrorLog errorLog)
		{
			foreach (Error err in errorLog) {
				Console.Error.WriteLine (err.Text);
			}
		}

		public static void Panic (string format, params object[] args)
		{
			Console.Error.WriteLine (format, args);
			Environment.Exit(-1);
		}
	}
}
