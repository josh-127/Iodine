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
using System.Diagnostics;
using System.Collections.Generic;

namespace Iodine.Runtime
{
	[IodineBuiltinModule ("__builtins__")]
	public class BuiltinsModule : IodineModule
	{
		class RangeIterator : IodineObject
		{
			private static IodineTypeDefinition RangeIteratorTypeDef = new IodineTypeDefinition ("RangeIterator");
			private long iterIndex = 0;
			private long min;
			private long end;
			private long step;

			public RangeIterator (long min, long max, long step)
				: base (RangeIteratorTypeDef)
			{
				this.end = max;
				this.step = step;
				this.min = min;
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
				this.iterIndex = min;
			}

		}

		public BuiltinsModule ()
			: base ("__builtins__")
		{
			SetAttribute ("stdin", new IodineStream (Console.OpenStandardInput (), false, true));
			SetAttribute ("stdout", new IodineStream (Console.OpenStandardOutput (), true, false));
			SetAttribute ("stderr", new IodineStream (Console.OpenStandardError (), true, false));
			SetAttribute ("eval", new InternalMethodCallback (eval, null));
			SetAttribute ("print", new InternalMethodCallback (print, null));
			SetAttribute ("input", new InternalMethodCallback (input, null));
			SetAttribute ("Int", IodineInteger.TypeDefinition);
			SetAttribute ("Float", IodineFloat.TypeDefinition);
			SetAttribute ("Str", IodineString.TypeDefinition);
			SetAttribute ("ByteStr", IodineByteString.TypeDefinition);
			SetAttribute ("Bool", IodineBool.TypeDefinition);
			SetAttribute ("Tuple", IodineTuple.TypeDefinition);
			SetAttribute ("List", IodineList.TypeDefinition);
			SetAttribute ("Event", IodineEvent.TypeDefinition);
			SetAttribute ("Object", new InternalMethodCallback (Object, null));
			SetAttribute ("HashMap", IodineMap.TypeDefinition);
			SetAttribute ("repr", new InternalMethodCallback (repr, null));
			SetAttribute ("filter", new InternalMethodCallback (filter, null));
			SetAttribute ("map", new InternalMethodCallback (map, null)); 
			SetAttribute ("reduce", new InternalMethodCallback (reduce, null));
			SetAttribute ("range", new InternalMethodCallback (range, null));
			SetAttribute ("open", new InternalMethodCallback (open, null));
			SetAttribute ("Exception", IodineException.TypeDefinition);
			SetAttribute ("TypeException", IodineTypeException.TypeDefinition);
			SetAttribute ("ArgumentException", IodineArgumentException.TypeDefinition);
			SetAttribute ("InternalException", IodineInternalErrorException.TypeDefinition);
			SetAttribute ("IndexException", IodineIndexException.TypeDefinition);
			SetAttribute ("IOException", IodineIOException.TypeDefinition);
			SetAttribute ("KeyNotFoundException", IodineKeyNotFound.TypeDefinition);
			SetAttribute ("AttributeNotFoundException", IodineAttributeNotFoundException.TypeDefinition);
			SetAttribute ("SyntaxException", IodineSyntaxException.TypeDefinition);
			SetAttribute ("NotSupportedException", IodineNotSupportedException.TypeDefinition);
			ExistsInGlobalNamespace = true;
		}

		private IodineObject eval (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineString str = args [0] as IodineString;
			IodineMap map = null;
			if (str == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			if (args.Length >= 2) {
				map = args [1] as IodineMap;
				if (map == null) {
					vm.RaiseException (new IodineTypeException ("HashMap"));
					return null;
				}
			}

			return eval (vm, str.ToString (), map);
		}

		private IodineObject eval (VirtualMachine host, string source, IodineMap dict)
		{
			VirtualMachine vm = host;

			if (dict != null) {
				vm = new VirtualMachine (new Dictionary<string, IodineObject> ());

				foreach (string glob in host.Globals.Keys) {
					vm.Globals [glob] = host.Globals [glob];
				}

				foreach (IodineObject key in dict.Keys.Values) {
					vm.Globals [key.ToString ()] = dict.Dict [key.GetHashCode ()];
				}
			}
			ErrorLog log = new ErrorLog ();
			IodineModule module = IodineModule.CompileModuleFromSource (log, source);
			if (module == null || log.ErrorCount > 0) {
				IodineSyntaxException e = new IodineSyntaxException (log);
				vm.RaiseException (e);
				return null;
			}
			return vm.InvokeMethod (module.Initializer, null, new IodineObject[]{ });
		}

		private IodineObject print (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			foreach (IodineObject arg in args) {
				Console.WriteLine (arg.ToString ());
			}
			return null;
		}

		private IodineObject input (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			foreach (IodineObject arg in args) {
				Console.Write (arg.ToString ());
			}

			return new IodineString (Console.ReadLine ());
		}

		private IodineObject Object (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineObject (IodineObject.ObjectTypeDef);
		}

		private IodineObject repr (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			return args [0].Represent (vm);
		}

		private IodineObject filter (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 1) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}

			IodineList list = new IodineList (new IodineObject[]{ });
			IodineObject collection = args [0];
			IodineObject func = args [1];
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
			if (args.Length <= 1) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}

			IodineList list = new IodineList (new IodineObject[]{ });
			IodineObject collection = args [0];
			IodineObject func = args [1];

			collection.IterReset (vm);
			while (collection.IterMoveNext (vm)) {
				IodineObject o = collection.IterGetNext (vm);
				list.Add (func.Invoke (vm, new IodineObject[] { o }));
			}
			return list;
		}

		private IodineObject reduce (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 1) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}

			IodineObject result = null;
			IodineObject collection = args [0];
			IodineObject func = args [1];

			collection.IterReset (vm);
			while (collection.IterMoveNext (vm)) {
				IodineObject o = collection.IterGetNext (vm);
				if (result == null)
					result = o;
				result = func.Invoke (vm, new IodineObject[] { result, o });
			}
			return result;
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
				IodineInteger stepObj = args [0] as IodineInteger;
				if (stepObj == null) {
					vm.RaiseException (new IodineTypeException ("Int"));
					return null;
				}
				end = stepObj.Value;
			} else if (args.Length == 2) {
				IodineInteger startObj = args [0] as IodineInteger;
				IodineInteger endObj = args [1] as IodineInteger;
				if (startObj == null || endObj == null) {
					vm.RaiseException (new IodineTypeException ("Int"));
					return null;
				}
				start = startObj.Value;
				end = endObj.Value;
			} else {
				IodineInteger startObj = args [0] as IodineInteger;
				IodineInteger endObj = args [1] as IodineInteger;
				IodineInteger stepObj = args [2] as IodineInteger;
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
			IodineString filePath = args [0] as IodineString;
			IodineString mode = args [1] as IodineString;

			if (filePath == null) {
				vm.RaiseException ("Expected filePath to be of type string!");
				return null;
			} else if (mode == null) {
				vm.RaiseException ("Expected mode to be of type string!");
				return null;
			}

			bool canRead = false;
			bool canWrite = false;
			bool append = false;

			foreach (char c in mode.Value) {
				switch (c) {
				case 'w':
					canWrite = true;
					break;
				case 'r':
					canRead = true;
					break;
				case 'a':
					append = true;
					break;
				}
			}

			if (!File.Exists (filePath.Value) && (canRead && !canWrite)) {
				vm.RaiseException (new IodineIOException ("File does not exist!"));
				return null;
			}

			if (append)
				return new IodineStream (File.Open (filePath.Value, FileMode.Append), true, true);
			else if (canRead && canWrite)
				return new IodineStream (File.Open (filePath.Value, FileMode.Create), canWrite, canRead);
			else if (canRead)
				return new IodineStream (File.OpenRead (filePath.Value), canWrite, canRead);
			else if (canWrite)
				return new IodineStream (File.Open (filePath.Value, FileMode.Create), canWrite, canRead);
			return null;
		}

	}
}

