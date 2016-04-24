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
using Iodine.Compiler;

namespace Iodine.Runtime
{
    public class IodineString : IodineObject
    {
        public static readonly IodineTypeDefinition TypeDefinition = new StringTypeDef ();

        class StringTypeDef : IodineTypeDefinition
        {
            public StringTypeDef ()
                : base ("Str")
            {
            }

            public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
            {
                if (args.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                }

                return new IodineString (args [0].ToString ());
            }
        }

        private int iterIndex = 0;

        public string Value { private set; get; }

        public IodineString (string val)
            : base (TypeDefinition)
        {
            Value = val;
            SetAttribute ("lower", new BuiltinMethodCallback (Lower, this));
            SetAttribute ("upper", new BuiltinMethodCallback (Upper, this));
            SetAttribute ("substr", new BuiltinMethodCallback (Substring, this));
            SetAttribute ("index", new BuiltinMethodCallback (IndexOf, this));
            SetAttribute ("rindex", new BuiltinMethodCallback (RightIndex, this));
            SetAttribute ("find", new BuiltinMethodCallback (Find, this));
            SetAttribute ("rfind", new BuiltinMethodCallback (RightFind, this));
            SetAttribute ("contains", new BuiltinMethodCallback (Contains, this));
            SetAttribute ("replace", new BuiltinMethodCallback (Replace, this));
            SetAttribute ("startsWith", new BuiltinMethodCallback (StartsWith, this));
            SetAttribute ("endsWith", new BuiltinMethodCallback (EndsWith, this));
            SetAttribute ("split", new BuiltinMethodCallback (Split, this));
            SetAttribute ("join", new BuiltinMethodCallback (Join, this));
            SetAttribute ("trim", new BuiltinMethodCallback (Trim, this));
            SetAttribute ("format", new BuiltinMethodCallback (Format, this));
            SetAttribute ("isLetter", new BuiltinMethodCallback (IsLetter, this));
            SetAttribute ("isDigit", new BuiltinMethodCallback (IsDigit, this));
            SetAttribute ("isLetterOrDigit", new BuiltinMethodCallback (IsLetterOrDigit, this));
            SetAttribute ("isWhiteSpace", new BuiltinMethodCallback (IsWhiteSpace, this));
            SetAttribute ("isSymbol", new BuiltinMethodCallback (IsSymbol, this));
            SetAttribute ("ljust", new BuiltinMethodCallback (PadRight, this));
            SetAttribute ("rjust", new BuiltinMethodCallback (PadLeft, this));
        }

        public override bool Equals (IodineObject obj)
        {
            IodineString strVal = obj as IodineString;

            if (strVal != null) {
                return strVal.Value == Value;
            }

            return false;
        }

        public override IodineObject Len (VirtualMachine vm)
        {
            return new IodineInteger (Value.Length);
        }

        public override IodineObject Slice (VirtualMachine vm, IodineSlice slice)
        {
            return new IodineString (Substring (
                slice.Start,
                slice.Stop,
                slice.Stride,
                slice.DefaultStart,
                slice.DefaultStop)
            );
        }

        private string Substring (int start, int end, int stride, bool defaultStart, bool defaultEnd)
        {
            int actualStart = start >= 0 ? start : Value.Length - (start + 2);
            int actualEnd = end >= 0 ? end : Value.Length - (end + 2);

            StringBuilder accum = new StringBuilder ();

            if (stride >= 0) {

                if (defaultStart) {
                    actualStart = 0;
                }

                if (defaultEnd) {
                    actualEnd = Value.Length;
                }

                for (int i = actualStart; i < actualEnd; i += stride) {
                    accum.Append (Value [i]);
                }
            } else {
                
                if (defaultStart) {
                    actualStart = Value.Length - 1;
                }

                if (defaultEnd) {
                    actualEnd = 0;
                }

                for (int i = actualStart; i >= actualEnd; i += stride) {
                    accum.Append (Value [i]);
                }
            }

            return accum.ToString ();
        }

        public override IodineObject Add (VirtualMachine vm, IodineObject right)
        {
            IodineString str = right as IodineString;
            if (str == null) {
                vm.RaiseException ("Right hand value must be of type Str!");
                return null;
            }
            return new IodineString (Value + str.Value);
        }

        public override IodineObject Equals (VirtualMachine vm, IodineObject right)
        {
            IodineString str = right as IodineString;
            if (str == null) {
                return base.Equals (vm, right);
            }
            return IodineBool.Create (str.Value == Value);
        }

        public override IodineObject NotEquals (VirtualMachine vm, IodineObject right)
        {
            IodineString str = right as IodineString;
            if (str == null) {
                return base.NotEquals (vm, right);
            }
            return IodineBool.Create (str.Value != Value);
        }

        public override string ToString ()
        {
            return Value;
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
            if (index.Value >= Value.Length) {
                vm.RaiseException (new IodineIndexException ());
                return null;
            }
            return new IodineString (Value [(int)index.Value].ToString ());
        }

        public override IodineObject GetIterator (VirtualMachine vm)
        {
            return this;
        }

        public override IodineObject IterGetCurrent (VirtualMachine vm)
        {
            return new IodineString (Value [iterIndex - 1].ToString ());
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

        public override IodineObject Represent (VirtualMachine vm)
        {
            return new IodineString (String.Format ("\"{0}\"", Value));
        }

        /**
		 * Iodine Method: Str.upper (self);
		 * Description: Returns the uppercase representation of this string
		 */
        private IodineObject Upper (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            return new IodineString (Value.ToUpper ());
        }

        /**
		 * Iodine Method: Str.lower (self);
		 * Description: Returns the lowercase representation of this string
		 */
        private IodineObject Lower (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            return new IodineString (Value.ToLower ());
        }

        /**
		 * Iodine Method: Str.substr (self, index, [length]);
		 * Description: Returns a substring of this string starting from index
		 */
        private IodineObject Substring (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            int start = 0;
            int len = 0;
            IodineInteger startObj = args [0] as IodineInteger;
            if (startObj == null) {
                vm.RaiseException (new IodineTypeException ("Int"));
                return null;
            }
            start = (int)startObj.Value;
            if (args.Length == 1) {
                len = this.Value.Length;
            } else {
                IodineInteger endObj = args [1] as IodineInteger;
                if (endObj == null) {
                    vm.RaiseException (new IodineTypeException ("Int"));
                    return null;
                }
                len = (int)endObj.Value;
            }

            if (start < Value.Length && len <= Value.Length) {
                return new IodineString (Value.Substring (start, len - start));
            }
            vm.RaiseException (new IodineIndexException ());
            return null;
        }

        /**
		 * Iodine Method: Str.index (self, value);
		 * Description: Returns the first positio of value in this string 
		 */
        private IodineObject IndexOf (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineString ch = args [0] as IodineString;

            if (ch == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }

            string val = ch.ToString ();

            if (!Value.Contains (val)) {
                vm.RaiseException (new IodineKeyNotFound ());
                return null;
            }

            return new IodineInteger (Value.IndexOf (val));
        }

        /**
         * Iodine Method: Str.rindex (self, value);
         * Description: Returns the first positio of value in this string 
         */
        private IodineObject RightIndex (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineString ch = args [0] as IodineString;

            if (ch == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }

            string val = ch.ToString ();

            if (!Value.Contains (val)) {
                vm.RaiseException (new IodineKeyNotFound ());
                return null;
            }
            return new IodineInteger (Value.LastIndexOf (val));
        }

        /**
         * Iodine Method: Str.find (self, value);
         * Description: Returns the first positio of value in this string 
         */
        private IodineObject Find (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineString ch = args [0] as IodineString;

            if (ch == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }
            string val = ch.ToString ();

            if (!Value.Contains (val)) {
                return new IodineInteger (-1);
            }

            return new IodineInteger (Value.IndexOf (val));
        }

        /**
         * Iodine Method: Str.rfind (self, value);
         * Description: Returns the first positio of value in this string 
         */
        private IodineObject RightFind (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineString ch = args [0] as IodineString;

            if (ch == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }
            string val = ch.ToString ();

            if (!Value.Contains (val)) {
                return new IodineInteger (-1);
            }
            return new IodineInteger (Value.LastIndexOf (val));
        }

        /**
		 * Iodine Method: Str.contains (self, value);
		 * Description: Returns true if this string contains value 
		 */
        private IodineObject Contains (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            return IodineBool.Create (Value.Contains (args [0].ToString ()));
        }

        /**
		 * Iodine Method: Str.startsWith (self, value);
		 * Description: Returns true if this string starts with value 
		 */
        private IodineObject StartsWith (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            return IodineBool.Create (Value.StartsWith (args [0].ToString ()));
        }

        /**
		 * Iodine Method: Str.endsWith (self, value);
		 * Description: Returns true if this string ends with value
		 */
        private IodineObject EndsWith (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            return IodineBool.Create (Value.EndsWith (args [0].ToString ()));
        }

        /**
		 * Iodine Method: Str.replace (self, oldValue, newValue);
		 * Description: Replaces all occurances of oldValue with newVale
		 */
        private IodineObject Replace (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 2) {
                vm.RaiseException (new IodineArgumentException (2));
                return null;
            }
            IodineString arg1 = args [0] as IodineString;
            IodineString arg2 = args [1] as IodineString;
            if (arg1 == null || arg2 == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }
            return new IodineString (Value.Replace (arg1.Value, arg2.Value));
        }

        /**
		 * Iodine Method: Str.split (self, seperator);
		 * Description: Splits this string by seperator, returning a list of substrings
		 */
        private IodineObject Split (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineString selfStr = self as IodineString;
            IodineString ch = args [0] as IodineString;
            char val;
            if (ch == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;

            }
            val = ch.Value [0];

            IodineList list = new IodineList (new IodineObject[]{ });
            foreach (string str in selfStr.Value.Split (val)) {
                list.Add (new IodineString (str));
            }
            return list;
        }

        /**
		 * Iodine Method: Str.trim (self);
		 * Description: Returns this string with all leading and trailing white spaces removed
		 */
        private IodineObject Trim (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            return new IodineString (Value.Trim ());
        }

        /**
		 * Iodine Method: Str.join (self, *args);
		 * Description: Combines each item in *args using this string as a seperator
		 */
        private IodineObject Join (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            StringBuilder accum = new StringBuilder ();
            IodineObject collection = args [0].GetIterator (vm);
            collection.IterReset (vm);
            string last = "";
            string sep = "";
            while (collection.IterMoveNext (vm)) {
                IodineObject o = collection.IterGetCurrent (vm);
                accum.AppendFormat ("{0}{1}", last, sep);
                last = o.ToString ();
                sep = Value;
            }
            accum.Append (last);
            return new IodineString (accum.ToString ());
        }

        /**
		 * Iodine Method: Str.format (self, *args);
		 * Description: Treats the string as a format specifier, applying it to *args
		 */
        private IodineObject Format (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            string format = this.Value;
            IodineFormatter formatter = new IodineFormatter ();
            return new IodineString (formatter.Format (vm, format, args));
        }

        /**
		 * Iodine Method: Str.isLetter (self);
		 * Description: Returns true if this string is a letter
		 */
        private IodineObject IsLetter (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            bool result = Value.Length == 0 ? false : true;
            for (int i = 0; i < Value.Length; i++) {
                if (!char.IsLetter (Value [i])) {
                    return IodineBool.False;
                }
            }
            return IodineBool.Create (result);
        }

        /**
		 * Iodine Method: Str.isDigit (self);
		 * Description: Returns true if this string is a numerical character
		 */
        private IodineObject IsDigit (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            bool result = Value.Length == 0 ? false : true;
            for (int i = 0; i < Value.Length; i++) {
                if (!char.IsDigit (Value [i])) {
                    return IodineBool.False;
                }
            }
            return IodineBool.Create (result);
        }

        /**
		 * Iodine Method: Str.isLetterOrDigit (self);
		 * Description: Returns true if this string is a letter or a digit
		 */
        private IodineObject IsLetterOrDigit (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            bool result = Value.Length == 0 ? false : true;
            for (int i = 0; i < Value.Length; i++) {
                if (!char.IsLetterOrDigit (Value [i])) {
                    return IodineBool.False;
                }
            }
            return IodineBool.Create (result);
        }

        /**
		 * Iodine Method: Str.isWhiteSpace (self);
		 * Description: Returns true if this string is a whitespace character
		 */
        private IodineObject IsWhiteSpace (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            bool result = Value.Length == 0 ? false : true;
            for (int i = 0; i < Value.Length; i++) {
                if (!char.IsWhiteSpace (Value [i])) {
                    return IodineBool.False;
                }
            }
            return IodineBool.Create (result);
        }

        /**
		 * Iodine Method: Str.isSymbol (self);
		 * Description: Returns true if this string is a symbol
		 */
        private IodineObject IsSymbol (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            bool result = Value.Length == 0 ? false : true;
            for (int i = 0; i < Value.Length; i++) {
                if (!char.IsSymbol (Value [i])) {
                    return IodineBool.False;
                }
            }
            return IodineBool.Create (result);
        }


        /**
		 * Iodine Method: Str.padRight (self, amount [, char]);
		 * Description: Returns true if this string is a symbol
		 */
        private IodineObject PadRight (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            char ch = ' ';

            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineInteger width = args [0] as IodineInteger;

            if (width == null) {
                vm.RaiseException (new IodineTypeException ("Int"));
                return null;
            }

            if (args.Length > 1) {
                IodineString chStr = args [0] as IodineString;

                if (chStr == null) {
                    vm.RaiseException (new IodineTypeException ("Str"));
                    return null;
                }

                ch = chStr.Value [0];
            }

            return new IodineString (Value.PadRight ((int)width.Value, ch));
        }

        /**
		 * Iodine Method: Str.padLeft (self, amount [, char]);
		 * Description: Returns true if this string is a symbol
		 */
        private IodineObject PadLeft (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            char ch = ' ';

            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineInteger width = args [0] as IodineInteger;

            if (width == null) {
                vm.RaiseException (new IodineTypeException ("Int"));
                return null;
            }

            if (args.Length > 1) {
                IodineString chStr = args [0] as IodineString;

                if (chStr == null) {
                    vm.RaiseException (new IodineTypeException ("Str"));
                    return null;
                }

                ch = chStr.Value [0];
            }

            return new IodineString (Value.PadLeft ((int)width.Value, ch));
        }
    }
}

