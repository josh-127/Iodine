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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Iodine.Compiler;

namespace Iodine.Runtime
{
    // TODO: Rewrite this
    public class IodineList : IodineObject
    {
        public static readonly IodineTypeDefinition TypeDefinition = new ListTypeDef ();

        class ListTypeDef : IodineTypeDefinition
        {
            public ListTypeDef ()
                : base ("List")
            {
            }

            public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
            {
                IodineList list = new IodineList (new IodineObject[0]);
                if (args.Length > 0) {
                    foreach (IodineObject arg in args) {
                        IodineObject collection = arg.GetIterator (vm);
                        collection.IterReset (vm);
                        while (collection.IterMoveNext (vm)) {
                            IodineObject o = collection.IterGetCurrent (vm);
                            list.Add (o);
                        }
                    }
                }
                return list;
            }
        }

        class ListIterator : IodineObject
        {
            private static IodineTypeDefinition TypeDefinition = new IodineTypeDefinition ("ListIterator");

            private int iterIndex = 0;
            private List<IodineObject> objects;

            public ListIterator (List<IodineObject> objects)
                : base (TypeDefinition)
            {
                this.objects = objects;
            }

            public override IodineObject IterGetCurrent (VirtualMachine vm)
            {
                return objects [iterIndex - 1];
            }

            public override bool IterMoveNext (VirtualMachine vm)
            {
                if (iterIndex >= objects.Count) {
                    return false;
                }
                iterIndex++;
                return true;
            }

            public override void IterReset (VirtualMachine vm)
            {
                iterIndex = 0;
            }
        }

        public List<IodineObject> Objects { private set; get; }

        public IodineList (List<IodineObject> list)
            : base (TypeDefinition)
        {
            SetAttribute ("append", new BuiltinMethodCallback (Add, this));
            SetAttribute ("prepend", new BuiltinMethodCallback (Prepend, this));
            SetAttribute ("appendRange", new BuiltinMethodCallback (AddRange, this));
            SetAttribute ("discard", new BuiltinMethodCallback (Discard, this));
            SetAttribute ("remove", new BuiltinMethodCallback (Remove, this));
            SetAttribute ("removeAt", new BuiltinMethodCallback (RemoveAt, this));
            SetAttribute ("contains", new BuiltinMethodCallback (Contains, this));
            SetAttribute ("clear", new BuiltinMethodCallback (Clear, this));
            SetAttribute ("index", new BuiltinMethodCallback (Index, this));
            SetAttribute ("rindex", new BuiltinMethodCallback (RightIndex, this));
            SetAttribute ("find", new BuiltinMethodCallback (Find, this));
            SetAttribute ("rfind", new BuiltinMethodCallback (RightFind, this));

            SetAttribute ("__iter__", new BuiltinMethodCallback ((VirtualMachine vm, IodineObject self, IodineObject[] args) => {
                return GetIterator (vm);
            }, this));

            Objects = list;
        }

        public IodineList (IodineObject[] items)
            : this (new List<IodineObject> (items))
        {
        }

        public override bool Equals (IodineObject obj)
        {
            IodineList listVal = obj as IodineList;

            if (listVal != null && listVal.Objects.Count == Objects.Count) {
                for (int i = 0; i < Objects.Count; i++) {
                    if (!Objects [i].Equals (listVal.Objects [i])) {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override IodineObject Len (VirtualMachine vm)
        {
            return new IodineInteger (Objects.Count);
        }

        public override IodineObject Slice (VirtualMachine vm, IodineSlice slice)
        {
            return Sublist (
                slice.Start,
                slice.Stop,
                slice.Stride,
                slice.DefaultStart,
                slice.DefaultStop
            );
        }

        private IodineList Sublist (int start, int end, int stride, bool defaultStart, bool defaultEnd)
        {
            int actualStart = start >= 0 ? start : Objects.Count - (start + 2);
            int actualEnd = end >= 0 ? end : Objects.Count - (end + 2);

            List<IodineObject> accum = new List<IodineObject> ();

            if (stride >= 0) {

                if (defaultStart) {
                    actualStart = 0;
                }

                if (defaultEnd) {
                    actualEnd = Objects.Count;
                }

                for (int i = actualStart; i < actualEnd; i += stride) {
                    accum.Add (Objects [i]);
                }
            } else {

                if (defaultStart) {
                    actualStart = Objects.Count - 1;
                }

                if (defaultEnd) {
                    actualEnd = 0;
                }

                for (int i = actualStart; i >= actualEnd; i += stride) {
                    accum.Add (Objects [i]);
                }
            }

            return new IodineList (accum);
        }

        public override IodineObject GetIndex (VirtualMachine vm, IodineObject key)
        {
            IodineInteger index = key as IodineInteger;
            if (index == null) {
                vm.RaiseException (new IodineTypeException ("Int"));
                return null;
            }

            if (index.Value < Objects.Count)
                return Objects [(int)index.Value];
            vm.RaiseException (new IodineIndexException ());
            return null;
        }

        public override void SetIndex (VirtualMachine vm, IodineObject key, IodineObject value)
        {
            IodineInteger index = key as IodineInteger;
            if (index == null) {
                vm.RaiseException (new IodineTypeException ("Int"));
                return;
            }

            if (index.Value < Objects.Count)
                this.Objects [(int)index.Value] = value;
            else
                vm.RaiseException (new IodineIndexException ());
        }

        public override IodineObject Add (VirtualMachine vm, IodineObject right)
        {
            IodineList list = new IodineList (Objects.ToArray ());
            right.IterReset (vm);
            while (right.IterMoveNext (vm)) {
                IodineObject o = right.IterGetCurrent (vm);
                list.Add (o);
            }
            return list;
        }

        public override IodineObject Equals (VirtualMachine vm, IodineObject right)
        {
            IodineList listVal = right as IodineList;
            if (listVal == null) {
                vm.RaiseException (new IodineTypeException ("List"));
                return null;
            }
            return IodineBool.Create (Compare (this, listVal));
        }

        public override IodineObject GetIterator (VirtualMachine vm)
        {
            return new ListIterator (Objects);
        }

        public override IodineObject Represent (VirtualMachine vm)
        {
            string repr = String.Join (", ", Objects.Select (p => p.Represent (vm).ToString ()));
            return new IodineString (String.Format ("[{0}]", repr));
        }

        public void Add (IodineObject obj)
        {
            Objects.Add (obj);
        }

        private bool Compare (IodineList list1, IodineList list2)
        {
            if (list1.Objects.Count != list2.Objects.Count) {
                return false;
            }

            for (int i = 0; i < list1.Objects.Count; i++) {
                if (list1.Objects [i].GetHashCode () != list2.Objects [i].GetHashCode ()) {
                    return false;
                }
            }
            return true;
        }

        /**
         * Iodine Method: List.append (self, *items)
         * Description: Appends each item to the list
         */
        private IodineObject Add (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            if (arguments.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            IodineList list = self as IodineList;
            foreach (IodineObject obj in arguments) {
                list.Add (obj);
            }
            return null;
        }

        /**
         * Iodine Method: List.appendRange (self, item)
         * Description: Iterates through item, appending each item to the list
         */
        private IodineObject AddRange (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            if (arguments.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            IodineObject collection = arguments [0].GetIterator (vm);
            collection.IterReset (vm);
            while (collection.IterMoveNext (vm)) {
                IodineObject o = collection.IterGetCurrent (vm);
                Add (o);
            }
            return null;
        }

        /**
         * Iodine Method: List.prepend (self, *items)
         * Description: Prepends each item to the list
         */
        private IodineObject Prepend (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            if (arguments.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            Objects.Insert (0, arguments [0]);

            return null;
        }

        /**
         * Iodine Method: List.discard (self, item)
         * Description: Removes item from the list
         */
        private IodineObject Discard (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            if (arguments.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineObject key = arguments [0];
            if (Objects.Contains (key)) {
                Objects.Remove (key);
                return IodineBool.True;
            }

            return IodineBool.False;
        }

        /**
         * Iodine Method: List.remove (self, item)
         * Description: Removes item from the list
         */
        private IodineObject Remove (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            if (arguments.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            IodineObject key = arguments [0];
            if (Objects.Contains (key))
                Objects.Remove (key);
            else
                vm.RaiseException (new IodineKeyNotFound ());
            return null;
        }

        /**
         * Iodine Method: List.removeAt (self, index)
         * Description: Removes the item at index
         */
        private IodineObject RemoveAt (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            if (arguments.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            IodineInteger index = arguments [0] as IodineInteger;
            if (index != null)
                Objects.RemoveAt ((int)index.Value);
            else
                vm.RaiseException (new IodineTypeException ("Int"));
            return null;
        }

        /**
         * Iodine Method: List.contains (self, value)
         * Description: Appends each item to the list
         */
        private IodineObject Contains (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            if (arguments.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            IodineObject key = arguments [0];
            int hashCode = key.GetHashCode ();
            bool found = false;
            foreach (IodineObject obj in Objects) {
                if (obj.GetHashCode () == hashCode) {
                    found = true;
                }
            }

            return IodineBool.Create (found);
        }

        /**
         * Iodine Method: List.splice (self, start, [end])
         * Description: Returns a sublist starting at start, and optionally ending at end
         */
        private IodineObject Splice (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            if (arguments.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            int start = 0;
            int end = Objects.Count;

            IodineInteger startInt = arguments [0] as IodineInteger;
            if (startInt == null) {
                vm.RaiseException (new IodineTypeException ("Int"));
                return null;
            }
            start = (int)startInt.Value;

            if (arguments.Length >= 2) {
                IodineInteger endInt = arguments [1] as IodineInteger;
                if (endInt == null) {
                    vm.RaiseException (new IodineTypeException ("Int"));
                    return null;
                }
                end = (int)endInt.Value;
            }

            if (start < 0)
                start = this.Objects.Count - start;
            if (end < 0)
                end = this.Objects.Count - end;

            IodineList retList = new IodineList (new IodineObject[]{ });

            for (int i = start; i < end; i++) {
                if (i < 0 || i > this.Objects.Count) {
                    vm.RaiseException (new IodineIndexException ());
                    return null;
                }
                retList.Add (Objects [i]);
            }

            return retList;
        }

        /**
         * Iodine Method: List.clear (self)
         * Description: Clears all items in this list
         */
        private IodineObject Clear (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
        {
            Objects.Clear ();
            return null;
        }

        /**
         * Iodine Method: List.index (self, i)
         * Description: Returns the index of item i
         */
        private IodineObject Index (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineObject item = args [0];

            if (!Objects.Contains (item)) {
                vm.RaiseException (new IodineKeyNotFound ());
                return null;
            }
            return new IodineInteger (Objects.IndexOf (item));
        }

        /**
         * Iodine Method: List.rindex (self, i)
         * Description: Returns the index of item i
         */
        private IodineObject RightIndex (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineObject item = args [0];

            if (!Objects.Contains (item)) {
                vm.RaiseException (new IodineKeyNotFound ());
                return null;
            }
            return new IodineInteger (Objects.LastIndexOf (item));
        } 


        /**
         * Iodine Method: List.find (self, i)
         * Description: Returns the index of item i
         */
        private IodineObject Find (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineObject item = args [0];

            if (!Objects.Contains (item)) {
                return new IodineInteger (-1);
            }
            return new IodineInteger (Objects.IndexOf (item));
        }

        /**
         * Iodine Method: List.rfind (self, i)
         * Description: Returns the index of item i
         */
        private IodineObject RightFind (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineObject item = args [0];

            if (!Objects.Contains (item)) {
                return new IodineInteger (-1);
            }
            return new IodineInteger (Objects.LastIndexOf (item));
        } 

        public override int GetHashCode ()
        {
            int accum = 17;
            unchecked {
                foreach (IodineObject obj in Objects) {
                    if (obj != null) {
                        accum += 529 * obj.GetHashCode ();
                    }
                }
            }
            return accum;
        }
    }
}
