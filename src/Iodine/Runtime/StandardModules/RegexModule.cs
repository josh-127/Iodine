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
using System.Text.RegularExpressions;

namespace Iodine.Runtime
{
    [IodineBuiltinModule ("regex")]
    public class RegexModule : IodineModule
    {
        class IodineRegex : IodineObject
        {
            public static readonly IodineTypeDefinition RegexTypeDef = new IodineTypeDefinition ("Regex");

            public Regex Value { private set; get; }

            public IodineRegex (Regex val)
                : base (RegexTypeDef)
            {
                this.Value = val;
                SetAttribute ("find", new BuiltinMethodCallback (Find, this));
                SetAttribute ("isMatch", new BuiltinMethodCallback (IsMatch, this));

            }

            /**
			 * Iodine Method: Regex.find (self, pattern)
			 * Description: Finds the first match of pattern
			 */
            private IodineObject Find (VirtualMachine vm, IodineObject self, IodineObject[] args)
            {
                if (args.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }
                IodineString expr = args [0] as IodineString;

                if (expr == null) {
                    vm.RaiseException (new IodineTypeException ("Str"));
                    return null;
                }

                return new IodineMatch (Value.Match (expr.ToString ()));
            }

            /**
			 * Iodine Method: Regex.isMatch (str)
			 * Description: Returns true if str is a match
			 */
            private IodineObject IsMatch (VirtualMachine vm, IodineObject self, IodineObject[] args)
            {
                if (args.Length <= 0) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }
                IodineString expr = args [0] as IodineString;

                if (expr == null) {
                    vm.RaiseException (new IodineTypeException ("Str"));
                    return null;
                }

                return IodineBool.Create (Value.IsMatch (expr.ToString ()));
            }

            /**
			 * Iodine Method: Regex.replace (self, pattern, value)
			 * Description: Replaces all substrings that match pattern with value
			 */
            private IodineObject Replace (VirtualMachine vm, IodineObject self, IodineObject[] args)
            {
                if (args.Length <= 1) {
                    vm.RaiseException (new IodineArgumentException (1));
                    return null;
                }
                IodineString input = args [0] as IodineString;
                IodineString val = args [1] as IodineString;

                if (input == null || val == null) {
                    vm.RaiseException (new IodineTypeException ("Str"));
                    return null;
                }

                Value.Replace (args [0].ToString (), args [1].ToString ());
                return null;
            }
        }

        class IodineMatch : IodineObject
        {
            public static readonly IodineTypeDefinition MatchTypeDef = new IodineTypeDefinition ("Match");

            public Match Value { private set; get; }

            private Match iterMatch;
            private Match iterRet;

            public IodineMatch (Match val)
                : base (MatchTypeDef)
            {
                Value = val;
                SetAttribute ("value", new IodineString (val.Value));
                SetAttribute ("success", IodineBool.Create (val.Success));
                SetAttribute ("getNextMatch", new BuiltinMethodCallback (GetNextMatch, this));
            }

            public override IodineObject GetIterator (VirtualMachine vm)
            {
                return this;
            }

            public override IodineObject IterGetCurrent (VirtualMachine vm)
            {
                return new IodineMatch (this.iterRet);
            }

            public override bool IterMoveNext (VirtualMachine vm)
            {
                iterRet = iterMatch;
                iterMatch = iterMatch.NextMatch ();
                if (iterRet.Success) {
                    return true;
                }
                return false;
            }

            public override void IterReset (VirtualMachine vm)
            {
                this.iterMatch = Value;
            }

            /**
			 * Iodine Method: Match.getNextMatch (self)
			 * Description: Returns the next match
			 */
            private IodineObject GetNextMatch (VirtualMachine vm, IodineObject self, IodineObject[] EventArgs)
            {
                return new IodineMatch (Value.NextMatch ());
            }
        }

        public RegexModule ()
            : base ("regex")
        {
            SetAttribute ("compile", new BuiltinMethodCallback (Compile, this));
            SetAttribute ("find", new BuiltinMethodCallback (Find, this));
            SetAttribute ("isMatch", new BuiltinMethodCallback (IsMatch, this));
        }

        /**
		 * Iodine Function: compile (pattern)
		 * Description: Compiles a regular expression pattern
		 */
        private IodineObject Compile (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            IodineString expr = args [0] as IodineString;

            if (expr == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }

            return new IodineRegex (new Regex (expr.ToString ()));
        }

        /**
		 * Iodine Function: find (str, pattern)
		 * Description: Finds the first match in str
		 */
        private IodineObject Find (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 1) {
                vm.RaiseException (new IodineArgumentException (2));
                return null;
            }
            IodineString data = args [0] as IodineString;
            IodineString pattern = args [1] as IodineString;

            if (pattern == null || data == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }

            return new IodineMatch (Regex.Match (data.ToString (), pattern.ToString ()));
        }

        /**
		 * Iodine Function: isMatch (str, pattern)
		 * Description: Returns true if any values in str match pattern
		 */
        private IodineObject IsMatch (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length <= 1) {
                vm.RaiseException (new IodineArgumentException (2));
                return null;
            }
            IodineString data = args [0] as IodineString;
            IodineString pattern = args [1] as IodineString;

            if (pattern == null || data == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }

            return IodineBool.Create (Regex.IsMatch (data.ToString (), pattern.ToString ()));
        }

    }
}

