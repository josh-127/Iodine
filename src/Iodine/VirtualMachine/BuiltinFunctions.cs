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
			private long iterMax = 0;
			private long iterIndex = 0;
			public RangeIterator (long max) 
				: base (RangeIteratorTypeDef)
			{
				this.iterMax = max;
			}


			public override IodineObject IterGetNext (VirtualMachine vm)
			{
				return new IodineInteger (iterIndex - 1);
			}

			public override bool IterMoveNext (VirtualMachine vm)
			{
				if (iterIndex >= iterMax) {
					return false;
				}
				iterIndex++;
				return true;
			}

			public override void IterReset (VirtualMachine vm)
			{
				this.iterIndex = 0;
			}

		}

		public void Initialize (Dictionary<string, IodineObject> globalDict)
		{
			globalDict["system"] = new InternalMethodCallback (system, null);
			globalDict["getEnv"] = new InternalMethodCallback (getEnv, null);
			globalDict["setEnv"] = new InternalMethodCallback (setEnv, null);
			globalDict["raise"] = new InternalMethodCallback (raise, null);
			globalDict["input"] = new InternalMethodCallback (input, null);
			globalDict["toInt"] = new InternalMethodCallback (toInt, null);
			globalDict["toStr"] = new InternalMethodCallback (toStr, null);
			globalDict["toBool"] = new InternalMethodCallback (toBool, null);
			globalDict["toChar"] = new InternalMethodCallback (toChar, null);
			globalDict["list"] = new InternalMethodCallback (list, null);
			globalDict["object"] = new InternalMethodCallback (Object, null);
			globalDict["hashMap"] = new InternalMethodCallback (hashMap, null);
			globalDict["filter"] = new InternalMethodCallback (filter, null);
			globalDict["map"] = new InternalMethodCallback (map, null);
			globalDict["range"] = new InternalMethodCallback (range, null);
			globalDict["open"] = new InternalMethodCallback (open, null);
			globalDict["isWhiteSpace"] = new InternalMethodCallback (isWhiteSpace, null);
			globalDict["isDigit"] = new InternalMethodCallback (isDigit, null);
			globalDict["isLetter"] = new InternalMethodCallback (isLetter, null);
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

		private IodineObject getEnv (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString str = args[0] as IodineString;
			return new IodineString (Environment.GetEnvironmentVariable (str.Value));
		}

		private IodineObject setEnv (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString str = args[0] as IodineString;
			Environment.SetEnvironmentVariable (str.Value, args[1].ToString ());
			return null;
		}

		private IodineObject raise (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString str = args[0] as IodineString;
			vm.RaiseException (str.Value);
			return null;
		}

		private IodineObject exec (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
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
			return new IodineInteger (Int64.Parse (args[0].ToString ()));
		}

		private IodineObject toStr (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineString (args[0].ToString ());
		}

		private IodineObject toBool (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineBool (Boolean.Parse (args[0].ToString ()));
		}

		private IodineObject toChar (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineChar ((char)((IodineInteger)args[0]).Value);
		}

		private IodineObject list (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineList (args);
		}

		private IodineObject Object (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineObject (IodineObject.ObjectTypeDef);
		}

		private IodineObject hashMap (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
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
			IodineInteger n = args[0] as IodineInteger;
			return new RangeIterator (n.Value);
		}

		private IodineObject open (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException ("Expected two or morw argumetns!");
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
				return new IodineFile (File.Open (filePath.Value, FileMode.OpenOrCreate), canWrite, canRead);
			else if (canRead) 
				return new IodineFile (File.OpenRead (filePath.Value), canWrite, canRead);
			else if (canWrite) 
				return new IodineFile (File.OpenWrite (filePath.Value), canWrite, canRead);
			return null;
		}

		private IodineObject isDigit (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			foreach (IodineObject arg in args) {
				IodineChar ch = arg as IodineChar;
				if (ch == null) {
					vm.RaiseException ("Expected type char!");
					return null;
				}
				if (!char.IsDigit (ch.Value)) {
					return IodineBool.False;
				}
			}
			return IodineBool.True;
		}

		private IodineObject isWhiteSpace (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			foreach (IodineObject arg in args) {
				IodineChar ch = arg as IodineChar;
				if (ch == null) {
					vm.RaiseException ("Expected type char!");
					return null;
				}
				if (!char.IsWhiteSpace (ch.Value)) {
					return IodineBool.False;
				}
			}
			return IodineBool.True;
		}

		private IodineObject isLetter (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			foreach (IodineObject arg in args) {
				IodineChar ch = arg as IodineChar;
				if (ch == null) {
					vm.RaiseException ("Expected type char!");
					return null;
				}
				if (!char.IsLetter (ch.Value)) {
					return IodineBool.False;
				}
			}
			return IodineBool.True;
		}

	}
}

