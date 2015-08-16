using System;
using System.IO;

namespace Iodine
{
	public class ReplShell
	{
		private IodineEngine engine = new IodineEngine ();

		public void Run ()
		{
			engine ["prompt"] = ">>> ";
			while (true) {
				Console.Write (engine ["prompt"].ToString ());
				string source = Console.ReadLine ();
				try {
					Console.WriteLine (engine.DoString (source).ToString ());
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

