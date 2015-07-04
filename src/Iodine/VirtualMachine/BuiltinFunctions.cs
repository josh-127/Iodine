using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Iodine
{
	public class BuiltinFunctions : IIodineExtension
	{
		class RangeIterator : IodineObject
		{
			private static IodineTypeDefinition RangeIteratorTypeDef  = new IodineTypeDefinition ("RangeIterator");
			private long iterIndex = 0;
			private long end;
			private long step;

			public RangeIterator (long min, long max, long step) 
				: base (RangeIteratorTypeDef)
			{
				this.end = max;
				this.step = step;
				this.iterIndex = min;
			}


			public override IodineObject IterGetNext (VirtualMachine vm)
			{
				return new IodineInteger (iterIndex - 1);
			}

			public override bool IterMoveNext (VirtualMachine vm)
			{
				if (iterIndex >= this.end) {
					return false;
				}
				iterIndex += this.step;
				return true;
			}

			public override void IterReset (VirtualMachine vm)
			{
				this.iterIndex = 0;
			}

		}

		public void Initialize (Dictionary<string, IodineObject> globalDict)
		{
			globalDict["stdin"] = new IodineStream (Console.OpenStandardInput (), false, true);
			globalDict["stdout"] = new IodineStream (Console.OpenStandardOutput (), true, false);
			globalDict["stderr"] = new IodineStream (Console.OpenStandardError (), true, false);
			globalDict["eval"] = new InternalMethodCallback (eval, null);
			globalDict["system"] = new InternalMethodCallback (system, null);
			globalDict["getEnv"] = new InternalMethodCallback (getEnv, null);
			globalDict["setEnv"] = new InternalMethodCallback (setEnv, null);
			globalDict["raise"] = new InternalMethodCallback (raise, null);
			globalDict["input"] = new InternalMethodCallback (input, null);
			globalDict["toInt"] = new InternalMethodCallback (toInt, null);
			globalDict["toStr"] = new InternalMethodCallback (toStr, null);
			globalDict["Int"] = new InternalMethodCallback (toInt, null);
			globalDict["Float"] = new InternalMethodCallback (toFloat, null);
			globalDict["Str"] = new InternalMethodCallback (toStr, null);
			globalDict["Bool"] = new InternalMethodCallback (toBool, null);
			globalDict["Char"] = new InternalMethodCallback (toChar, null);
			globalDict["toBool"] = new InternalMethodCallback (toBool, null);
			globalDict["toChar"] = new InternalMethodCallback (toChar, null);
			globalDict["list"] = new InternalMethodCallback (list, null);
			globalDict["event"] = new InternalMethodCallback (createEvent, null);
			globalDict["object"] = new InternalMethodCallback (Object, null);
			globalDict["hashMap"] = new InternalMethodCallback (hashMap, null);
			globalDict["Tuple"] = new InternalMethodCallback (tuple, null);
			globalDict["List"] = new InternalMethodCallback (list, null);
			globalDict["Event"] = new InternalMethodCallback (createEvent, null);
			globalDict["Object"] = new InternalMethodCallback (Object, null);
			globalDict["HashMap"] = new InternalMethodCallback (hashMap, null);
			globalDict["filter"] = new InternalMethodCallback (filter, null);
			globalDict["map"] = new InternalMethodCallback (map, null);
			globalDict["range"] = new InternalMethodCallback (range, null);
			globalDict["open"] = new InternalMethodCallback (open, null);
			globalDict["sleep"] = new InternalMethodCallback (sleep, null);
		}


		private IodineObject eval (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString str = args[0] as IodineString;
			IodineMap map = args[1] as IodineMap;
			IodineBool restrict = args[2] as IodineBool;
			return eval (vm, str.ToString (), map, restrict.Value);
		}

		private IodineObject eval (VirtualMachine vm, string source, IodineMap dict, bool restricted)
		{
			if (restricted) {
				vm = new VirtualMachine (new Dictionary<string, IodineObject> ());
			}
			foreach (IodineObject key in dict.Keys.Values) {
				vm.Globals[key.ToString ()] = dict.Dict[key.GetHashCode ()];
			}

			ErrorLog log = new ErrorLog ();
			Lexer iLexer = new Lexer (log, source);
			TokenStream tokens = iLexer.Scan ();
			if (log.ErrorCount > 0) 
				return null;
			Parser iParser = new Parser (tokens);
			AstNode root = iParser.Parse ();
			if (log.ErrorCount > 0) 
				return null;
			SemanticAnalyser iAnalyser = new SemanticAnalyser (log);
			SymbolTable sym = iAnalyser.Analyse ((Ast)root);
			if (log.ErrorCount > 0) 
				return null;
			IodineMethod tmpMethod = new IodineMethod (vm.Stack.CurrentModule, "eval", false, 0, 0);
			FunctionCompiler compiler = new FunctionCompiler (log, sym, tmpMethod);
			root.Visit (compiler);
			tmpMethod.FinalizeLabels ();
			return vm.InvokeMethod (tmpMethod, null, new IodineObject[]{});
		}

		private IodineObject system (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString str = args[0] as IodineString;
			IodineString cmdArgs = args[1] as IodineString;
			ProcessStartInfo info = new ProcessStartInfo (str.Value, cmdArgs.Value);
			info.UseShellExecute = false;
			Process proc = Process.Start (info);
			proc.WaitForExit ();
			return new IodineInteger (proc.ExitCode);
		}

		private IodineObject sleep (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}
			IodineInteger time = args[0] as IodineInteger;
			System.Threading.Thread.Sleep ((int)time.Value);
			return null;
		}
		private IodineObject getEnv (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}
			IodineString str = args[0] as IodineString;

			if (str == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}
			if (Environment.GetEnvironmentVariable (str.Value) != null)
				return new IodineString (Environment.GetEnvironmentVariable (str.Value));
			else 
				return null;
		}

		private IodineObject setEnv (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}
			IodineString str = args[0] as IodineString;
			Environment.SetEnvironmentVariable (str.Value, args[1].ToString ());
			return null;
		}

		private IodineObject raise (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}
			IodineString str = args[0] as IodineString;
			vm.RaiseException (str.Value);
			return null;
		}

		private IodineObject input (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			foreach (IodineObject arg in args) {
				Console.Write (arg.ToString ());
			}

			return new IodineString (Console.ReadLine ());
		}

		private IodineObject toInt (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}

			return new IodineInteger (Int64.Parse (args[0].ToString ()));
		}

		private IodineObject toFloat (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}

			return new IodineFloat (Double.Parse (args[0].ToString ()));
		}

		private IodineObject toStr (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}
			if (args[0].HasAttribute ("_toStr")) {
				IodineString ret = args[0].GetAttribute ("_toStr").Invoke (vm, new IodineObject[]{})
					as IodineString;
				return ret;
			}
			return new IodineString (args[0].ToString ());
		}

		private IodineObject toBool (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}

			return new IodineBool (Boolean.Parse (args[0].ToString ()));
		}

		private IodineObject toChar (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			if (args[0] is IodineInteger)
				return new IodineChar ((char)((IodineInteger)args[0]).Value);
			else if (args[0] is IodineString)
				return new IodineChar ((char)args[0].ToString()[0]);
			return null;
		}

		private IodineObject list (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineList (args);
		}

		private IodineObject tuple (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length >= 1) {
				IodineList inputList = args[0] as IodineList;

				return new IodineTuple (inputList.Objects.ToArray ());

			}
			return null;
		}

		private IodineObject createEvent (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineEvent ();
		}

		private IodineObject Object (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineObject (IodineObject.ObjectTypeDef);
		}

		private IodineObject hashMap (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length >= 1) {
				IodineList inputList = args[0] as IodineList;
				IodineMap ret = new IodineMap ();
				if (inputList != null) {
					foreach (IodineObject item in inputList.Objects) {
						IodineTuple kv = item as IodineTuple;
						if (kv != null) {
							ret.Dict.Add (kv.Objects[0].GetHashCode (), kv.Objects[1]);
						}
					}
				} 
				return ret;
			}
			return new IodineMap ();
		}

		private IodineObject filter (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineList list = new IodineList (new IodineObject[]{});
			IodineObject collection = args[0];
			IodineObject func = args[1];
			collection.IterReset (vm);
			while (collection.IterMoveNext (vm)) {
				IodineObject o = collection.IterGetNext (vm);
				if (func.Invoke (vm, new IodineObject[] { o }).IsTrue ()) {
					list.Add (o);
				}
			}
			return list;
		}

		private IodineObject map (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineList list = new IodineList (new IodineObject[]{});
			IodineObject collection = args[0];
			IodineObject func = args[1];
			collection.IterReset (vm);
			while (collection.IterMoveNext (vm)) {
				IodineObject o = collection.IterGetNext (vm);
				list.Add (func.Invoke (vm, new IodineObject[] { o }));
			}
			return list;
		}

		private IodineObject range (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			long start = 0;
			long end = 0;
			long step = 1;
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			if (args.Length == 1) {
				IodineInteger stepObj = args[0] as IodineInteger;
				if (stepObj == null) {
					vm.RaiseException (new IodineTypeException ("Int"));
					return null;
				}
				end = stepObj.Value;
			} else if (args.Length == 2) {
				IodineInteger startObj = args[0] as IodineInteger;
				IodineInteger endObj = args[0] as IodineInteger;
				if (startObj == null || endObj == null) {
					vm.RaiseException (new IodineTypeException ("Int"));
					return null;
				}
				start = startObj.Value;
				end = endObj.Value;
			} else {
				IodineInteger startObj = args[0] as IodineInteger;
				IodineInteger endObj = args[1] as IodineInteger;
				IodineInteger stepObj = args[2] as IodineInteger;
				if (startObj == null || endObj == null || stepObj == null) {
					vm.RaiseException (new IodineTypeException ("Int"));
					return null;
				}
				start = startObj.Value;
				end = endObj.Value;
				step = stepObj.Value;
			}
			return new RangeIterator (start, end, step);
		}

		private IodineObject open (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}
			IodineString filePath = args[0] as IodineString;
			IodineString mode = args[1] as IodineString;

			if (filePath == null) {
				vm.RaiseException ("Expected filePath to be of type string!");
				return null;
			} else if (mode == null) {
				vm.RaiseException ("Expected mode to be of type string!");
				return null;
			}

			bool canRead = false;
			bool canWrite = false;

			foreach (char c in mode.Value) {
				switch (c) {
				case 'w':
					canWrite = true;
					break;
				case 'r':
					canRead = true;
					break;
				}
			}

			if (!File.Exists (filePath.Value) && (canRead && !canWrite)) {
				vm.RaiseException ("File does not exist!");
				return null;
			}

			if (canRead && canWrite)
				return new IodineStream (File.Open (filePath.Value, FileMode.OpenOrCreate), canWrite, canRead);
			else if (canRead) 
				return new IodineStream (File.OpenRead (filePath.Value), canWrite, canRead);
			else if (canWrite) 
				return new IodineStream (File.OpenWrite (filePath.Value), canWrite, canRead);
			return null;
		}

	}
}

