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
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Iodine.Compiler;

namespace Iodine.Runtime
{
    public class IodineBytes : IodineObject
    {
        public static readonly IodineTypeDefinition TypeDefinition = new BytesTypeDef ();

        [BuiltinDocString (
            "Returns a new Bytes instance and attempts to convert the supplied argument into a Bytes object.",
            "@param value A value to convert into a bytes object."
        )]
        class BytesTypeDef : IodineTypeDefinition
        {
            public BytesTypeDef ()
                : base ("Bytes")
            {
            }

            public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
            {
                if (args.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                }

                if (args [0] is IodineString) {
                    return new IodineBytes (args [0].ToString ());
                }

                IodineObject iter = args [0];

                iter.IterReset (vm);

                List<byte> bytes = new List<byte> ();

                while (iter.IterMoveNext (vm)) {
                    IodineInteger b = iter.IterGetCurrent (vm) as IodineInteger;

                    if (b == null) {
                        vm.RaiseException (new IodineException ("Int"));
                        return null;
                    }
                    bytes.Add ((byte)(b.Value & 0xFF));
                }

                return new IodineBytes (bytes.ToArray ());
            }
        }

        private int iterIndex = 0;

        public byte[] Value { private set; get; }

        public IodineBytes ()
            : base (TypeDefinition)
        {
            SetAttribute ("index", new BuiltinMethodCallback (IndexOf, this));
            SetAttribute ("rindex", new BuiltinMethodCallback (IndexOf, this));
            SetAttribute ("substr", new BuiltinMethodCallback (Substring, this));
            SetAttribute ("contains", new BuiltinMethodCallback (Contains, this));

            // HACK: Add __iter__ attribute to match Iterable trait
            SetAttribute ("__iter__", new BuiltinMethodCallback ((VirtualMachine vm, IodineObject self, IodineObject [] args) => {
                return GetIterator (vm);
            }, this));
        }

        public IodineBytes (byte[] val)
            : this ()
        {
            Value = val;
        }

        public IodineBytes (string val)
            : this ()
        {
            Value = Encoding.ASCII.GetBytes (val);
        }

        public override IodineObject Represent (VirtualMachine vm)
        {
            return new IodineString (String.Format ("b'{0}'", Encoding.ASCII.GetString (Value)));
        }

        public override IodineObject Len (VirtualMachine vm)
        {
            return new IodineInteger (Value.Length);
        }

        public override IodineObject Add (VirtualMachine vm, IodineObject right)
        {
            IodineBytes str = right as IodineBytes;

            if (str == null) {
                vm.RaiseException ("Right hand value must be of type Bytes!");
                return null;
            }

            byte[] newArr = new byte[str.Value.Length + Value.Length];
            Array.Copy (Value, newArr, Value.Length);
            Array.Copy (str.Value, 0, newArr, Value.Length, str.Value.Length);
            return new IodineBytes (newArr);
        }

        public override IodineObject Equals (VirtualMachine vm, IodineObject right)
        {
            IodineBytes str = right as IodineBytes;
            if (str == null) {
                return base.Equals (vm, right);
            }
            return IodineBool.Create (Enumerable.SequenceEqual<byte> (str.Value, Value));
        }

        public override IodineObject NotEquals (VirtualMachine vm, IodineObject right)
        {
            IodineBytes str = right as IodineBytes;
            if (str == null) {
                return base.NotEquals (vm, right);
            }
            return IodineBool.Create (!Enumerable.SequenceEqual<byte> (str.Value, Value));
        }

        public override string ToString ()
        {
            return Encoding.ASCII.GetString (Value);
        }

        public override int GetHashCode ()
        {
            return Value.GetHashCode ();
        }

        public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
        {
            IodineInteger index = key as IodineInteger;
            if (index == null) {
                vm.RaiseException (new IodineTypeException ("Int"));
                return null;
            }
            if (index.Value >= this.Value.Length) {
                vm.RaiseException (new IodineIndexException ());
                return null;
            }
            return new IodineInteger ((long)Value [(int)index.Value]);
        }

        public override IodineObject GetIterator (VirtualMachine vm)
        {
            return this;
        }

        public override IodineObject IterGetCurrent (VirtualMachine vm)
        {
            return new IodineInteger ((long)Value [iterIndex - 1]);
        }

        public override bool IterMoveNext (VirtualMachine vm)
        {
            if (iterIndex >= Value.Length) {
                return false;
            }
            iterIndex++;
            return true;
        }

        public override void IterReset (VirtualMachine vm)
        {
            iterIndex = 0;
        }

        /**
         * Iodine Function: Bytes.indexOf (self, value)
         * Description: Returns the first position of value
         */
        private IodineObject IndexOf (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            int val = ConvertToByte (args [0]);

            if (val < 0) {
                vm.RaiseException (new IodineTypeException ("Int"));
                return null;
            }

            for (int i = 0; i < Value.Length; i++) {
                if (Value [i] == val) {
                    return new IodineInteger (i);
                }
            }

            return new IodineInteger (-1);
        }

        /**
         * Iodine Function: Bytes.lastIndexOf (self, value)
         * Description: Returns the last position of value
         */
        private IodineObject LastIndexOf (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            int val = ConvertToByte (args [0]);

            if (val < 0) {
                vm.RaiseException (new IodineTypeException ("Int"));
                return null;
            }

            int lastI = -1;

            for (int i = 0; i < Value.Length; i++) {
                if (Value [i] == val) {
                    lastI = i;
                }
            }

            return new IodineInteger (lastI);
        }

        /**
         * Iodine Function: Bytes.substr (self, start, [length])
         * Description: Returns the substring of this value starting at start
         */
        private IodineObject Substring (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            if (args.Length == 1) {
                IodineInteger i1 = args [0] as IodineInteger;
                if (i1 == null) {
                    vm.RaiseException (new IodineTypeException ("Int"));
                    return null;
                }
                return Substring (vm, i1);
            } else {
                IodineInteger i1 = args [0] as IodineInteger;
                IodineInteger i2 = args [1] as IodineInteger;

                if (i1 == null || i2 == null) {
                    vm.RaiseException (new IodineTypeException ("Int"));
                    return null;
                }

                return Substring (vm, i1, i2);
            }
        }

        private IodineObject Substring (VirtualMachine vm, IodineInteger i1)
        {
            byte[] newBytes = new byte[Value.Length - (int)i1.Value];
            int nI = 0;

            for (int i = (int)i1.Value; i < newBytes.Length; i++) {
                newBytes [nI++] = Value [i];
            }

            return new IodineBytes (newBytes);
        }

        private IodineObject Substring (VirtualMachine vm, IodineInteger i1, IodineInteger i2)
        {
            byte[] newBytes = new byte[(int)i2.Value];

            int nI = 0;

            for (int i = (int)i1.Value; nI < (int)i2.Value; i++) {
                newBytes [nI++] = Value [i];
            }

            return new IodineBytes (newBytes);

        }

        /**
         * Iodine Function: Bytes.contains (self, value)
         * Description: Returns true if this byte string contains value
         */
        private IodineObject Contains (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineBytes needle = args [0] as IodineBytes;

            if (needle == null) {
                vm.RaiseException (new IodineTypeException ("Bytes"));
                return null;
            }

            for (int i = 0; i < Value.Length; i++) {
                bool found = true;

                for (int sI = 0; sI < needle.Value.Length; sI++) {
                    if (needle.Value [sI] != Value [i]) {
                        found = false;
                        break;
                    }
                }

                if (found) {
                    return IodineBool.True;
                }
            }

            return IodineBool.False;
        }

        private static int ConvertToByte (IodineObject obj)
        {
            if (obj is IodineInteger) {
                return (byte)((IodineInteger)obj).Value;
            } 

            if (obj is IodineString) {
                string val = obj.ToString ();
                if (val.Length == 1) {
                    return (byte)val [0];
                }
            }

            return -1;
        }
    }
}

