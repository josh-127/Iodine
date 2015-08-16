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
	public class IodineBool : IodineObject
	{
		public static readonly IodineTypeDefinition TypeDefinition = new BoolTypeDef ();

		public static readonly IodineBool True = new IodineBool (true);
		public static readonly IodineBool False = new IodineBool (false);


		class BoolTypeDef : IodineTypeDefinition
		{
			public BoolTypeDef () 
				: base ("Bool")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
				}
				return new IodineBool (Boolean.Parse (args[0].ToString ()));
			}
		}

		public bool Value {
			private set;
			get;
		}

		public IodineBool (bool val)
			: base (TypeDefinition)
		{
			this.Value = val;
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			IodineBool boolVal = rvalue as IodineBool;
			switch (binop) {
			case BinaryOperation.Equals:
				return new IodineBool (boolVal.Value == Value);
			case BinaryOperation.NotEquals:
				return new IodineBool (boolVal.Value == Value);
			case BinaryOperation.BoolAnd:
				return new IodineBool (boolVal.Value && Value);
			case BinaryOperation.BoolOr:
				return new IodineBool (boolVal.Value || Value);
			}
			return base.PerformBinaryOperation (vm, binop, rvalue);
		}

		public override IodineObject PerformUnaryOperation (VirtualMachine vm, UnaryOperation op)
		{
			switch (op) {
			case UnaryOperation.BoolNot:
				return new IodineBool (!this.Value);
			}
			return null;
		}

		public override bool IsTrue ()
		{
			return this.Value;
		}

		public override string ToString ()
		{
			return this.Value.ToString ();
		}

		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}
	}
}

