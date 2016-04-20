// /**
//   * Copyright (c) 2015, GruntTheDivine All rights reserved.
//
//   * Redistribution and use in source and binary forms, with or without modification,
//   * are permitted provided that the following conditions are met:
//   * 
//   *  * Redistributions of source code must retain the above copyright notice, this list
//   *    of conditions and the following disclaimer.
//   * 
//   *  * Redistributions in binary form must reproduce the above copyright notice, this
//   *    list of conditions and the following disclaimer in the documentation and/or
//   *    other materials provided with the distribution.
//
//   * Neither the name of the copyright holder nor the names of its contributors may be
//   * used to endorse or promote products derived from this software without specific
//   * prior written permission.
//   * 
//   * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
//   * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
//   * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
//   * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
//   * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
//   * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
//   * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
//   * CONTRACT ,STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
//   * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
//   * DAMAGE.
// /**
using System;
using System.Collections.Generic;
using Iodine.Runtime;

namespace Iodine.Util
{
    /// <summary>
    /// Dictionary specifically for storing and retrieving IodineObjects using IodineObjects
    /// as keys
    /// </summary>
    public class ObjectDictionary
    {
        /// <summary>
        /// Dictionary entry, implemented using a doubly linked list
        /// </summary>
        class DictionaryEntry 
        {
            public IodineObject Key {
                set;
                get;
            }

            public IodineObject Value {
                set;
                get;
            }

            public DictionaryEntry _NextEntry;
            public DictionaryEntry _PrevEntry;

            public DictionaryEntry (IodineObject key, IodineObject value)
            {
                Key = key;
                Value = value;
            }
        }

        private int _count;
        private DictionaryEntry _head = null;
        private DictionaryEntry _tail = null;

        public int Count {
            get { 
                return _count;
            }
        }

        public void Clear ()
        {
            _head = null;
            _tail = null;
            _count = 0;
        }

        public IodineObject Get (int index)
        {
            DictionaryEntry top = _head;

            for (int i = 0; i != index && top != null; i++) {
                top = top._NextEntry;
            }

            if (top != null) {
                return top.Key;
            }
            return null;

        }

        public IodineObject Get (IodineObject key)
        {
            DictionaryEntry entry = GetEntry (key);

            if (entry != null) {
                return entry.Value;
            }

            return null;
        }

        public void Remove (IodineObject key)
        {
            DictionaryEntry entry = GetEntry (key);

            if (entry != null) {
                if (entry._PrevEntry != null) {
                    entry._PrevEntry._NextEntry = entry._NextEntry;
                }

                if (_tail == entry) {
                    _tail = entry._PrevEntry;
                }

                if (_head == entry) {
                    _head = entry._NextEntry;
                }
                _count--;
            }
        }

        public void Set (IodineObject key, IodineObject value)
        {
            if (ContainsKey (key)) {
                GetEntry (key).Value = value;
                return;
            }

            if (_head == null) {
                _head = new DictionaryEntry (key, value);
                _tail = _head;
            } else {
                DictionaryEntry prev = _tail;
                _tail._NextEntry = new DictionaryEntry (key, value);
                _tail = _tail._NextEntry;
                _tail._PrevEntry = prev;
            }
            _count++;
        }

        public bool ContainsKey (IodineObject key)
        {
            DictionaryEntry i = _head;

            while (i != null) {
                if (i.Key.Equals (key)) {
                    return true;
                }
                i = i._NextEntry;
            }
            return false;
        }

        public IEnumerable<IodineObject> GetKeys ()
        {
            List<IodineObject> keys = new List<IodineObject> ();

            DictionaryEntry i = _head;

            while (i != null) {
                keys.Add (i.Key);
                i = i._NextEntry;
            }

            return keys;
        }

        public IEnumerable<IodineObject> GetValues ()
        {
            List<IodineObject> values = new List<IodineObject> ();

            DictionaryEntry i = _head;

            while (i != null) {
                values.Add (i.Value);
                i = i._NextEntry;
            }

            return values;
        }

        private DictionaryEntry GetEntry (IodineObject key)
        {
            DictionaryEntry i = _head;

            while (i != null) {
                if (i.Key.Equals (key)) {
                    return i;
                }
                i = i._NextEntry;
            }
            return null;
        }
    }
}

