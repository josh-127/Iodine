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
using Iodine.Compiler;

namespace Iodine.Runtime
{
	public class IodineChar : IodineObject
	{
		public static readonly IodineTypeDefinition TypeDefinition = new CharTypeDef ();

		class CharTypeDef : IodineTypeDefinition
		{
			public CharTypeDef ()
				: base ("Char")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}
				if (args [0] is IodineInteger)
					return new IodineChar ((char)((IodineInteger)args [0]).Value);
				else if (args [0] is IodineString)
					return new IodineChar ((char)args [0].ToString () [0]);
				return null;
			}
		}

		public char Value {
			private set;
			get;
		}

		public IodineChar (char value)
			: base (TypeDefinition)
		{
			Value = value;
			SetAttribute ("isLetter", new InternalMethodCallback (isLetter, this));
			SetAttribute ("isDigit", new InternalMethodCallback (isDigit, this));
			SetAttribute ("isLetterOrDigit", new InternalMethodCallback (isLetterOrDigit, this));
			SetAttribute ("isWhiteSpace", new InternalMethodCallback (isWhiteSpace, this));
			SetAttribute ("isSymbol", new InternalMethodCallback (isSymbol, this));
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			
			IodineChar otherChr = rvalue as IodineChar;
			char otherVal;
			if (otherChr == null) {
				if (rvalue is IodineString) {
					otherVal = rvalue.ToString () [0];
				} else if (rvalue is IodineNull) {
					return base.PerformBinaryOperation (vm, binop, rvalue);
				} else {
					vm.RaiseException ("Right value must be of type char!");
					return null;
				}
			} else {
				otherVal = otherChr.Value;
			}

			switch (binop) {
			case BinaryOperation.Equals:
				return new IodineBool (otherVal == Value);
			case BinaryOperation.NotEquals:
				return new IodineBool (otherVal != Value);
			default:
				return base.PerformBinaryOperation (vm, binop, rvalue);	
			}
		}

		public override string ToString ()
		{
			return Value.ToString ();
		}

		public override int GetHashCode ()
		{
			return Value.ToString ().GetHashCode ();
		}

		private IodineObject isLetter (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineBool (char.IsLetter (Value));
		}

		private IodineObject isDigit (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineBool (char.IsDigit (Value));
		}

		private IodineObject isLetterOrDigit (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineBool (char.IsLetterOrDigit (Value));
		}

		private IodineObject isWhiteSpace (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineBool (char.IsWhiteSpace (Value));
		}

		private IodineObject isSymbol (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineBool (char.IsSymbol (Value));
		}
	}
}

