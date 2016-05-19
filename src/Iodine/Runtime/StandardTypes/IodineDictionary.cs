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
using System.Collections.Generic;
using Iodine.Util;
using Iodine.Compiler;

namespace Iodine.Runtime
{
    public class IodineDictionary : IodineObject
    {
        public static readonly IodineTypeDefinition TypeDefinition = new MapTypeDef ();

        class MapTypeDef : IodineTypeDefinition
        {
            public MapTypeDef ()
                : base ("Dict")
            {
                BindAttributes (this);
            }


            public override IodineObject BindAttributes (IodineObject obj)
            {
                obj.SetAttribute ("contains", new BuiltinMethodCallback (Contains, obj));
                obj.SetAttribute ("getSize", new BuiltinMethodCallback (GetSize, obj));
                obj.SetAttribute ("clear", new BuiltinMethodCallback (Clear, obj));
                obj.SetAttribute ("set", new BuiltinMethodCallback (Set, obj));
                obj.SetAttribute ("get", new BuiltinMethodCallback (Get, obj));
                obj.SetAttribute ("remove", new BuiltinMethodCallback (Remove, obj));
                return obj;
            }

            public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
            {
                if (args.Length >= 1) {
                    IodineList inputList = args [0] as IodineList;
                    IodineDictionary ret = new IodineDictionary ();
                    if (inputList != null) {
                        foreach (IodineObject item in inputList.Objects) {
                            IodineTuple kv = item as IodineTuple;
                            if (kv != null) {
                                ret.Set (kv.Objects [0], kv.Objects [1]);
                            }
                        }
                    } 
                    return ret;
                }
                return new IodineDictionary ();
            }

            [BuiltinDocString (
                "Tests to see if the dictionary contains a key, returning true if it does.",
                "@param key The key to test if this dictionary contains."
            )] 
            private IodineObject Contains (VirtualMachine vm, IodineObject self, IodineObject[] args)
            {
                IodineDictionary thisObj = self as IodineDictionary;
                if (args.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }
                return IodineBool.Create (thisObj.dict.ContainsKey (args [0]));
            }

            private IodineObject GetSize (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineDictionary thisObj = self as IodineDictionary;
                return new IodineInteger (thisObj.dict.Count);
            }

            [BuiltinDocString (
                "Clears the dictionary, removing all items."
            )]
            private IodineObject Clear (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineDictionary thisObj = self as IodineDictionary;
                thisObj.dict.Clear ();
                return null;
            }

            [BuiltinDocString (
                "Sets a key to a specified value, if the key does not exist, it will be created.",
                "@param key The key of the specified value",
                "@param value The value associated with [key]"
            )]
            private IodineObject Set (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineDictionary thisObj = self as IodineDictionary;
                if (arguments.Length >= 2) {
                    IodineObject key = arguments [0];
                    IodineObject val = arguments [1];
                    thisObj.dict [key] = val;
                    return null;
                }
                vm.RaiseException (new IodineArgumentException (2));
                return null;
            }

            [BuiltinDocString (
                "Returns the value specified by [key], raising a KeyNotFound exception if the given key does not exist.",
                "@param key The key whose value will be returned."
            )]
            private IodineObject Get (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineDictionary thisObj = self as IodineDictionary;
                if (arguments.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                } else if (arguments.Length == 1) {
                    IodineObject key = arguments [0];
                    if (thisObj.dict.ContainsKey (key)) {
                        return thisObj.dict [key];
                    }
                    vm.RaiseException (new IodineKeyNotFound ());
                    return null;
                } else {
                    IodineObject key = arguments [0];
                    if (thisObj.dict.ContainsKey (key)) {
                        return thisObj.dict [key];
                    }
                    return arguments [1];
                }
            }

            [BuiltinDocString (
                "Removes a specified entry from the dictionary, raising a KeyNotFound exception if the given key does not exist.",
                "@param key The key which is to be removed."
            )]
            private IodineObject Remove (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
            {
                IodineDictionary thisObj = self as IodineDictionary;
                if (arguments.Length >= 1) {
                    IodineObject key = arguments [0];
                    if (!thisObj.dict.ContainsKey (key)) {
                        vm.RaiseException (new IodineKeyNotFound ());
                        return null;
                    }
                    thisObj.dict.Remove (key);
                    return null;
                }
                vm.RaiseException (new IodineArgumentException (2));
                return null;
            }
        }

        class DictIterator : IodineObject
        {
            private static IodineTypeDefinition TypeDefinition = new IodineTypeDefinition ("DictIterator");

            private IEnumerator<KeyValuePair<IodineObject, IodineObject>> enumerator;

            public DictIterator (Dictionary<IodineObject, IodineObject> dict)
                : base (TypeDefinition)
            {
                enumerator = dict.GetEnumerator ();
            }

            public override IodineObject IterGetCurrent (VirtualMachine vm)
            {
                KeyValuePair<IodineObject, IodineObject> current = enumerator.Current;

                return new IodineTuple (new IodineObject [] {
                    current.Key,
                    current.Value
                });
            }

            public override bool IterMoveNext (VirtualMachine vm)
            {
                return enumerator.MoveNext ();
            }

            public override void IterReset (VirtualMachine vm)
            {
                enumerator.Reset ();
            }
        }

        protected readonly Dictionary<IodineObject, IodineObject> dict = new Dictionary<IodineObject, IodineObject> ();

        public IEnumerable<IodineObject> Keys {
            get {
                return dict.Keys;
            }
        }

        public IodineDictionary ()
            : base (TypeDefinition)
        {
            SetAttribute ("__iter__", new BuiltinMethodCallback ((VirtualMachine vm, IodineObject self, IodineObject [] args) => {
                return GetIterator (vm);
            }, this));
        }

        public override IodineObject Len (VirtualMachine vm)
        {
            return new IodineInteger (dict.Count);
        }

        public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
        {
            return dict [key];
        }

        public override void SetIndex (VirtualMachine vm, IodineObject key, IodineObject value)
        {
            dict [key] = value;
        }

        public override bool Equals (IodineObject obj)
        {
            IodineDictionary map = obj as IodineDictionary;

            if (map != null) {
                return CompareTo (map);
            }

            return false;
        }

        public override IodineObject Equals (VirtualMachine vm, IodineObject right)
        {
            IodineDictionary hash = right as IodineDictionary;
            if (hash == null) {
                vm.RaiseException (new IodineTypeException ("HashMap"));
                return null;
            }
            return IodineBool.Create (CompareTo (hash));
        }
       
        public override int GetHashCode ()
        {
            return dict.GetHashCode ();
        }

        public override IodineObject GetIterator (VirtualMachine vm)
        {
            return new DictIterator (dict);
        }

        /// <summary>
        /// Set the specified key and valuw.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="val">Value.</param>
        public void Set (IodineObject key, IodineObject val)
        {
            dict [key] = val;
        }

        /// <summary>
        /// Get the specified key.
        /// </summary>
        /// <param name="key">Key.</param>
        public IodineObject Get (IodineObject key)
        {
            return dict [key];
        }

        /// <summary>
        /// Compares two iodine dictionaries
        /// </summary>
        /// <returns><c>true</c>, if they are equal, <c>false</c> otherwise.</returns>
        /// <param name="hash">Dictionary</param>
        private bool CompareTo (IodineDictionary hash)
        {
            if (hash.dict.Count != this.dict.Count) {
                return false;
            }

            foreach (IodineObject key in dict.Keys) {
                if (!hash.dict.ContainsKey (key)) {
                    return false;
                }
                if (!hash.dict [key].Equals (dict [key])) {
                    return false;
                }
            }
            return true;
        }

    }
}
