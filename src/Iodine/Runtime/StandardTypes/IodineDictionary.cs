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
        }

        private int iterIndex = 0;

        protected readonly ObjectDictionary dict = new ObjectDictionary ();

        public IEnumerable<IodineObject> Keys {
            get {
                return dict.GetKeys ();
            }
        }

        public IodineDictionary ()
            : base (TypeDefinition)
        {
            SetAttribute ("contains", new BuiltinMethodCallback (Contains, this));
            SetAttribute ("getSize", new BuiltinMethodCallback (GetSize, this));
            SetAttribute ("clear", new BuiltinMethodCallback (Clear, this));
            SetAttribute ("set", new BuiltinMethodCallback (Set, this));
            SetAttribute ("get", new BuiltinMethodCallback (Get, this));
            SetAttribute ("remove", new BuiltinMethodCallback (Remove, this));
        }

        public override IodineObject Len (VirtualMachine vm)
        {
            return new IodineInteger (dict.Count);
        }

        public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
        {
            return dict.Get (key);
        }

        public override void SetIndex (VirtualMachine vm, IodineObject key, IodineObject value)
        {
            dict.Set (key, value);
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
       
        public override IodineObject GetIterator (VirtualMachine vm)
        {
            return this;
        }

        public override IodineObject IterGetCurrent (VirtualMachine vm)
        {
            return dict.Get (iterIndex - 1);
        }

        public override bool IterMoveNext (VirtualMachine vm)
        {
            if (iterIndex >= dict.Count) {
                return false;
            }
            iterIndex++;
            return true;
        }

        public override void IterReset (VirtualMachine vm)
        {
            iterIndex = 0;
        }

        /// <summary>
        /// Set the specified key and valuw.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="val">Value.</param>
        public void Set (IodineObject key, IodineObject val)
        {
            dict.Set (key, val);
        }

        /// <summary>
        /// Get the specified key.
        /// </summary>
        /// <param name="key">Key.</param>
        public IodineObject Get (IodineObject key)
        {
            return dict.Get (key);
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

            foreach (IodineObject key in dict.GetKeys ()) {
                if (!hash.dict.ContainsKey (key)) {
                    return false;
                }
                if (!hash.dict.Get (key).Equals (dict.Get (key))) {
                    return false;
                }
            }
            return true;
        }

        /**
         * Iodine Function: contains (self, key)
         * Description: Returns true if this dictionary contains key
         */
        private IodineObject Contains (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            return IodineBool.Create (dict.ContainsKey (args [0]));
        }

        /**
         * == DEPRECATED ==
         * Iodine Function: getSize (self)
         * Description: Returns the size of this dictionary
         */
        private IodineObject GetSize (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            return new IodineInteger (dict.Count);
        }

        /**
         * Iodine Function: clear (self)
         * Description: Clears the dictionary
         */
        private IodineObject Clear (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            dict.Clear ();
            return null;
        }

        /**
         * Iodine Function: set (self, key, value)
         * Description: Sets a specified key to value, if it does not exists it will be created
         */
        private IodineObject Set (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            if (arguments.Length >= 2) {
                IodineObject key = arguments [0];
                IodineObject val = arguments [1];
                dict.Set (key, val);
                return null;
            }
            vm.RaiseException (new IodineArgumentException (2));
            return null;
        }

        /**
         * Iodine Function: get (self, key, [default])
         * Description: Returns a value specified by key
         */
        private IodineObject Get (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            if (arguments.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            } else if (arguments.Length == 1) {
                IodineObject key = arguments [0];
                if (dict.ContainsKey (key)) {
                    return dict.Get (key);
                }
                vm.RaiseException (new IodineKeyNotFound ());
                return null;
            } else {
                IodineObject key = arguments [0];
                if (dict.ContainsKey (key)) {
                    return dict.Get (key);
                }
                return arguments [1];
            }
        }

        /**
         * Iodine Function: remove (self, key)
         * Description: Removes a specified entry from the dictionary
         */
        private IodineObject Remove (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            if (arguments.Length >= 1) {
                IodineObject key = arguments [0];
                if (!dict.ContainsKey (key)) {
                    vm.RaiseException (new IodineKeyNotFound ());
                    return null;
                }
                dict.Remove (key);
                return null;
            }
            vm.RaiseException (new IodineArgumentException (2));
            return null;
        }
    }
}
