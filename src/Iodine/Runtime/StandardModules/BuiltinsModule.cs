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
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Iodine.Compiler;

namespace Iodine.Runtime
{
    [IodineBuiltinModule ("__builtins__")]
    public class BuiltinsModule : IodineModule
    {
        public BuiltinsModule ()
            : base ("__builtins__")
        {
            SetAttribute ("stdin", new IodineStream (Console.OpenStandardInput (), false, true));
            SetAttribute ("stdout", new IodineStream (Console.OpenStandardOutput (), true, false));
            SetAttribute ("stderr", new IodineStream (Console.OpenStandardError (), true, false));
            SetAttribute ("invoke", new BuiltinMethodCallback (Invoke, null));
            SetAttribute ("require", new BuiltinMethodCallback (Require, null));
            SetAttribute ("compile", new BuiltinMethodCallback (Compile, null));
            SetAttribute ("chr", new BuiltinMethodCallback (Chr, null));
            SetAttribute ("ord", new BuiltinMethodCallback (Ord, null));
            SetAttribute ("len", new BuiltinMethodCallback (Len, null));
            SetAttribute ("hex", new BuiltinMethodCallback (Hex, null));
            SetAttribute ("property", new BuiltinMethodCallback (Property, null));
            SetAttribute ("eval", new BuiltinMethodCallback (Eval, null));
            SetAttribute ("type", new BuiltinMethodCallback (Typeof, null));
            SetAttribute ("typecast", new BuiltinMethodCallback (Typecast, null));
            SetAttribute ("print", new BuiltinMethodCallback (Print, null));
            SetAttribute ("input", new BuiltinMethodCallback (Input, null));
            SetAttribute ("Complex", IodineComplex.TypeDefinition);
            SetAttribute ("Int", IodineInteger.TypeDefinition);
            SetAttribute ("BigInt", IodineBigInt.TypeDefinition);
            SetAttribute ("Float", IodineFloat.TypeDefinition);
            SetAttribute ("Str", IodineString.TypeDefinition);
            SetAttribute ("Bytes", IodineBytes.TypeDefinition);
            SetAttribute ("Bool", IodineBool.TypeDefinition);
            SetAttribute ("Tuple", IodineTuple.TypeDefinition);
            SetAttribute ("List", IodineList.TypeDefinition);
            SetAttribute ("Object", new BuiltinMethodCallback (Object, null));
            SetAttribute ("Dict", IodineDictionary.TypeDefinition);
            SetAttribute ("repr", new BuiltinMethodCallback (Repr, null));
            SetAttribute ("filter", new BuiltinMethodCallback (Filter, null));
            SetAttribute ("map", new BuiltinMethodCallback (Map, null)); 
            SetAttribute ("reduce", new BuiltinMethodCallback (Reduce, null));
            SetAttribute ("zip", new BuiltinMethodCallback (Zip, null)); 
            SetAttribute ("sum", new BuiltinMethodCallback (Sum, null)); 
            SetAttribute ("range", new BuiltinMethodCallback (Range, null));
            SetAttribute ("open", new BuiltinMethodCallback (Open, null));
            SetAttribute ("Exception", IodineException.TypeDefinition);
            SetAttribute ("TypeException", IodineTypeException.TypeDefinition);
            SetAttribute ("TypeCastException", IodineTypeCastException.TypeDefinition);
            SetAttribute ("ArgumentException", IodineArgumentException.TypeDefinition);
            SetAttribute ("InternalException", IodineInternalErrorException.TypeDefinition);
            SetAttribute ("IndexException", IodineIndexException.TypeDefinition);
            SetAttribute ("IOException", IodineIOException.TypeDefinition);
            SetAttribute ("KeyNotFoundException", IodineKeyNotFound.TypeDefinition);
            SetAttribute ("AttributeNotFoundException", IodineAttributeNotFoundException.TypeDefinition);
            SetAttribute ("SyntaxException", IodineSyntaxException.TypeDefinition);
            SetAttribute ("NotSupportedException", IodineNotSupportedException.TypeDefinition);
            SetAttribute ("StringBuffer", IodineStringBuilder.TypeDefinition);
            SetAttribute ("Null", IodineNull.Instance.TypeDef);
            SetAttribute ("TypeDef", IodineTypeDefinition.TypeDefinition);
            SetAttribute ("__globals__", IodineGlobals.Instance);
            ExistsInGlobalNamespace = true;
        }

        /**
         * Iodine Function: compile (source)
         * Description: Compiles a string of Iodine source code, returning an invokable iodine module
         */
        private IodineObject Compile (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            IodineString source = args [0] as IodineString;
            SourceUnit unit = SourceUnit.CreateFromSource (source.Value);
            return unit.Compile (vm.Context);
        }

        /**
         * Iodine Function: property (getter, [setter])
         * Description: Returns a property using the getter and setter method provided
         */
        private IodineObject Property (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            IodineObject getter = args [0];
            IodineObject setter = args.Length > 1 ? args [1] : null;
            return new IodineProperty (getter, setter, null);
        }

        /**
         * Iodine Function: require ()
         * Description: Internal use for use statement, not intended to be called directly!!!
         */
        private IodineObject Require (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineString path = args [0] as IodineString;

            if (path == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }

            string name = path.Value;
            string fullPath = Path.GetFullPath (name);

            if (args.Length == 1) {
                // use <module>
                if (VirtualMachine.ModuleCache.ContainsKey (fullPath)) {
                    IodineModule module = VirtualMachine.ModuleCache [fullPath];
                    vm.SetGlobal (Path.GetFileNameWithoutExtension (fullPath), module);
                } else {
                    IodineModule module = vm.LoadModule (name);
                    vm.SetGlobal (Path.GetFileNameWithoutExtension (fullPath), module);

                    VirtualMachine.ModuleCache [fullPath] = module;

                    if (module.Initializer != null) {
                        module.Initializer.Invoke (vm, new IodineObject[] { });
                    }
                }
            } else {
                // use <types> from <module>
                IodineTuple names = args [1] as IodineTuple;
                if (names == null) {
                    vm.RaiseException (new IodineTypeCastException ("Tuple"));
                    return null;
                }
                IodineModule module = null;

                if (VirtualMachine.ModuleCache.ContainsKey (fullPath)) {
                    module = VirtualMachine.ModuleCache [fullPath];
                } else {
                    module = vm.LoadModule (name);
                    VirtualMachine.ModuleCache [fullPath] = module;
                    if (module.Initializer != null) {
                        module.Initializer.Invoke (vm, new IodineObject[] { });
                    }
                }

                vm.SetGlobal (Path.GetFileNameWithoutExtension (fullPath), module);
                
                if (names.Objects.Length > 0) {
                    foreach (IodineObject item in names.Objects) {
                        vm.SetGlobal (item.ToString (),
                            module.GetAttribute (item.ToString ())
                        );
                    }
                } else {
                    foreach (KeyValuePair<string, IodineObject> kv in module.Attributes) {
                        vm.SetGlobal (kv.Key, kv.Value);
                    }
                }
            }
            return null;
        }

        /*
         * Iodine Function: invoke (obj, globals);
         * Description: Invokes an iodine object under a new Iodine context
         */
        private IodineObject Invoke (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 1) {
                vm.RaiseException (new IodineArgumentException (2));
                return null;
            }
            IodineDictionary hash = args [1] as IodineDictionary;
            Dictionary<string, IodineObject> items = new Dictionary<string, IodineObject> ();

            foreach (IodineObject key in hash.Keys) {
                items [key.ToString ()] = hash.Get (key);
            }

            VirtualMachine newVm = new VirtualMachine (vm.Context, items);

            try {
                return args [0].Invoke (newVm, new IodineObject[]{ });
            } catch (UnhandledIodineExceptionException ex) {
                vm.RaiseException (ex.OriginalException);
                return null;
            }
        }

        /**
         * Iodine Function: chr (val)
         * Description: Returns the character representation of val
         */
        private IodineObject Chr (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            IodineInteger ascii = args [0] as IodineInteger;
            return new IodineString (((char)(int)ascii.Value).ToString ());
        }

        /**
         * Iodine Function: ord (val)
         * Description: Returns the numeric representation of character val
         */
        private IodineObject Ord (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            IodineString str = args [0] as IodineString;

            if (str == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }

            return new IodineInteger ((int)str.Value [0]);
        }

        /**
         * Iodine Function: hex (val)
         * Description: Returns the hex representation of val
         */
        private IodineObject Hex (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            string[] lut = new string[] { 
                "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "0a", "0b", "0c", "0d", "0e", "0f",
                "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "1a", "1b", "1c", "1d", "1e", "1f",
                "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "2a", "2b", "2c", "2d", "2e", "2f",
                "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "3a", "3b", "3c", "3d", "3e", "3f",
                "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "4a", "4b", "4c", "4d", "4e", "4f",
                "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "5a", "5b", "5c", "5d", "5e", "5f",
                "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6a", "6b", "6c", "6d", "6e", "6f",
                "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "7a", "7b", "7c", "7d", "7e", "7f",
                "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "8a", "8b", "8c", "8d", "8e", "8f",
                "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "9a", "9b", "9c", "9d", "9e", "9f",
                "a0", "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", "a9", "aa", "ab", "ac", "ad", "ae", "af",
                "b0", "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "b9", "ba", "bb", "bc", "bd", "be", "bf",
                "c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9", "ca", "cb", "cc", "cd", "ce", "cf",
                "d0", "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8", "d9", "da", "db", "dc", "dd", "de", "df",
                "e0", "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "e9", "ea", "eb", "ec", "ed", "ee", "ef",
                "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8", "f9", "fa", "fb", "fc", "fd", "fe", "ff"
            };

            if (args.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }


            StringBuilder accum = new StringBuilder ();

            if (args [0] is IodineBytes) {
                IodineBytes bytes = args [0] as IodineBytes;

                foreach (byte b in bytes.Value) {
                    accum.Append (lut [b]);
                }

                return new IodineString (accum.ToString ());
            }

            if (args [0] is IodineString) {
                IodineString str = args [0] as IodineString;

                foreach (byte b in str.Value) {
                    accum.Append (lut [b]);
                }

                return new IodineString (accum.ToString ());
            }

            IodineObject iterator = args [0].GetIterator (vm);


            if (iterator != null) {
                while (iterator.IterMoveNext (vm)) {
                    IodineInteger b = iterator.IterGetCurrent (vm) as IodineInteger;

                    if (b == null) {
                        vm.RaiseException (new IodineTypeException ("Int"));
                        return null;
                    }

                    accum.Append (lut [b.Value & 0xFF]);

                }
            }

            return new IodineString (accum.ToString ());
        }

        /**
         * Iodine Function: len (val)
         * Description: Returns the length of val, calling val.__len__ ()
         */
        private IodineObject Len (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            return args [0].Len (vm);
        }

        private IodineObject Eval (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineString str = args [0] as IodineString;
            IodineDictionary map = null;
            if (str == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }

            if (args.Length >= 2) {
                map = args [1] as IodineDictionary;
                if (map == null) {
                    vm.RaiseException (new IodineTypeException ("Dict"));
                    return null;
                }
            }

            return Eval (vm, str.ToString (), map);
        }

        private IodineObject Eval (VirtualMachine host, string source, IodineDictionary dict)
        {
            VirtualMachine vm = host;

            if (dict != null) {
                vm = new VirtualMachine (host.Context, new Dictionary<string, IodineObject> ());

                foreach (string glob in host.Globals.Keys) {
                    vm.Globals [glob] = host.Globals [glob];
                }

                foreach (IodineObject key in dict.Keys) {
                    vm.SetGlobal (key.ToString (), dict.Get (key));
                }
            }
            IodineContext context = new IodineContext ();
            SourceUnit code = SourceUnit.CreateFromSource (source);
            IodineModule module = null;
            try {
                module = code.Compile (context);
            } catch (SyntaxException ex) {
                vm.RaiseException (new IodineSyntaxException (ex.ErrorLog));
                return null;
            }
            return vm.InvokeMethod (module.Initializer, null, new IodineObject[]{ });
        }

        /**
         * Iodine Function: typeof (val)
         * Description: Returns the type of val
         */
        private IodineObject Typeof (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            return args [0].TypeDef;
        }

        /**
         * Iodine Function: typecast (type, val)
         * Description: Casts val to type, raising an exception of val is not of the specified type
         */
        private IodineObject Typecast (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 1) {
                vm.RaiseException (new IodineArgumentException (2));
                return null;
            }
            IodineTypeDefinition typedef = args [0] as IodineTypeDefinition;
            if (typedef == null) {
                vm.RaiseException (new IodineTypeException ("TypeDef"));
                return null;
            }

            if (!args [1].InstanceOf (typedef)) {
                vm.RaiseException (new IodineTypeCastException (typedef.ToString ()));
                return null;
            }

            return args [1];
        }

        /**
         * Iodine Function: print (*args)
         * Description: Prints each string in args
         */
        private IodineObject Print (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            foreach (IodineObject arg in args) {
                Console.WriteLine (arg.ToString ());
            }
            return null;
        }

        /**
         * Iodine Function: input ([prompt])
         * Description: Reads a line from stdin, displaying prompt
         */
        private IodineObject Input (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            foreach (IodineObject arg in args) {
                Console.Write (arg.ToString ());
            }

            return new IodineString (Console.ReadLine ());
        }

        /**
         * Iodine Function: Object ()
         * Description: Returns a new Iodine Object with no associated type information
         */
        private IodineObject Object (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            return new IodineObject (IodineObject.ObjectTypeDef);
        }

        /**
         * Iodine Function: repr (obj)
         * Description: Returns the string representation of an obj, calling obj.__repr__ ()
         */
        private IodineObject Repr (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            return args [0].Represent (vm);
        }

        /**
         * Iodine Function: filter (iterable, func) 
         * Description: Iterates though each item in iterable, passing them to func. If func returns
         * true, the value is appened to a list which is returned to the caller
         */
        private IodineObject Filter (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 1) {
                vm.RaiseException (new IodineArgumentException (2));
                return null;
            }

            IodineList list = new IodineList (new IodineObject[]{ });
            IodineObject collection = args [0].GetIterator (vm);
            IodineObject func = args [1];
            collection.IterReset (vm);

            while (collection.IterMoveNext (vm)) {
                IodineObject o = collection.IterGetCurrent (vm);
                if (func.Invoke (vm, new IodineObject[] { o }).IsTrue ()) {
                    list.Add (o);
                }
            }
            return list;
        }

        /**
         * Iodine Function: map (iterable, func)
         * Description: Iterates through each item in iterable, passing each item to func and appending
         * the result to a list which is returned to the caller
         */
        private IodineObject Map (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 1) {
                vm.RaiseException (new IodineArgumentException (2));
                return null;
            }

            IodineList list = new IodineList (new IodineObject[]{ });
            IodineObject collection = args [0].GetIterator (vm);
            IodineObject func = args [1];

            collection.IterReset (vm);
            while (collection.IterMoveNext (vm)) {
                IodineObject o = collection.IterGetCurrent (vm);
                list.Add (func.Invoke (vm, new IodineObject[] { o }));
            }
            return list;
        }

        private IodineObject Reduce (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 1) {
                vm.RaiseException (new IodineArgumentException (2));
                return null;
            }

            IodineObject result = args.Length > 2 ? args [1] : null;
            IodineObject collection = args [0].GetIterator (vm);
            IodineObject func = args.Length > 2 ? args [2] : args [1];

            collection.IterReset (vm);
            while (collection.IterMoveNext (vm)) {
                IodineObject o = collection.IterGetCurrent (vm);
                if (result == null)
                    result = o;
                result = func.Invoke (vm, new IodineObject[] { result, o });
            }
            return result;
        }

        /**
         * 
         */
        private IodineObject Zip (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineList result = new IodineList (new IodineObject[0]);
            IodineObject[] iterators = new IodineObject[args.Length];
            for (int i = 0; i < args.Length; i++) {
                iterators [i] = args [i].GetIterator (vm);
                iterators [i].IterReset (vm);
            }

            while (true) {
                IodineObject[] objs = new IodineObject[iterators.Length];
                for (int i = 0; i < iterators.Length; i++) {
                    if (!iterators [i].IterMoveNext (vm))
                        return result;
                    IodineObject o = iterators [i].IterGetCurrent (vm);
                    objs [i] = o;
                }
                result.Add (new IodineTuple (objs));
            }
        }

        /**
         * Iodine Function: sum (iterable)
         * Description: Adds each item in the supplied iterable object
         */
        private IodineObject Sum (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineObject initial = args.Length > 1 ? args [1] : new IodineInteger (0);
            IodineObject collection = args [0].GetIterator (vm);

            collection.IterReset (vm);
            while (collection.IterMoveNext (vm)) {
                IodineObject o = collection.IterGetCurrent (vm);
                initial = initial.Add (vm, o);
            }
            return initial;
        }

        /**
         * Iodine Function: range (start, [end], step = 1)
         * Description: Returns a new iterator that will yield integers between the specified range
         */
        private IodineObject Range (VirtualMachine vm, IodineObject self, IodineObject[] args)
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
            return new IodineRange (start, end, step);
        }

        /**
         * Iodine Function: open (path, mode)
         * Description: Attempts to open a file, returning a new Stream object
         */
        private IodineObject Open (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 2) {
                vm.RaiseException (new IodineArgumentException (2));
                return null;
            }
            IodineString filePath = args [0] as IodineString;
            IodineString mode = args [1] as IodineString;

            if (filePath == null || mode == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }

            bool canRead = false;
            bool canWrite = false;
            bool append = false;
            bool binary = false;

            foreach (char c in mode.Value) {
                switch (c) {
                case 'w':
                    canWrite = true;
                    break;
                case 'r':
                    canRead = true;
                    break;
                case 'b':
                    binary = true;
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
                return new IodineStream (File.Open (filePath.Value, FileMode.Append), true, true, binary);
            else if (canRead && canWrite)
                return new IodineStream (File.Open (filePath.Value, FileMode.Create), canWrite, canRead, binary);
            else if (canRead)
                return new IodineStream (File.OpenRead (filePath.Value), canWrite, canRead, binary);
            else if (canWrite)
                return new IodineStream (File.Open (filePath.Value, FileMode.Create), canWrite, canRead, binary);
            return null;
        }

    }
}

