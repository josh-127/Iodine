using System;
using System.Collections.Generic;

namespace Iodine
{
	public class BuiltinFunctions : IIodineExtension
	{
		public void Initialize (Dictionary<string, IodineObject> globalDict)
		{
			globalDict["input"] = new InternalMethodCallback (input, null);
			globalDict["toInt"] = new InternalMethodCallback (toInt, null);
			globalDict["toStr"] = new InternalMethodCallback (toStr, null);
			globalDict["toBool"] = new InternalMethodCallback (toBool, null);
			globalDict["list"] = new InternalMethodCallback (list, null);
			globalDict["object"] = new InternalMethodCallback (Object, null);
			globalDict["hashMap"] = new InternalMethodCallback (hashMap, null);
			globalDict["filter"] = new InternalMethodCallback (filter, null);
			globalDict["map"] = new InternalMethodCallback (map, null);
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

		private IodineObject list (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineList (args);
		}

		private IodineObject Object (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineObject ();
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
	}
}

