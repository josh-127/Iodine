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
	public class IodineFloat : IodineObject
	{
		public static readonly IodineTypeDefinition TypeDefinition = new FloatTypeDef ();

		class FloatTypeDef : IodineTypeDefinition
		{
			public FloatTypeDef () 
				: base ("Float")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
				}

				return new IodineFloat (Double.Parse (args[0].ToString ()));
			}
		}

		public double Value {
			private set;
			get;
		}

		public IodineFloat (double val)
			: base (TypeDefinition)
		{
			this.Value = val;
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			IodineFloat floatVal = rvalue as IodineFloat;

			double op2 = 0;
			if (floatVal == null) {
				if (rvalue is IodineInteger) {
					IodineInteger intVal = rvalue as IodineInteger;
					op2 = (double)intVal.Value;
				} else {
					vm.RaiseException (new IodineTypeException ("Float"));
					return null;
				}
			} else {
				op2 = floatVal.Value;
			}

			switch (binop) {
			case BinaryOperation.Add:
				return new IodineFloat (Value + op2);
			case BinaryOperation.Sub:
				return new IodineFloat (Value - op2);
			case BinaryOperation.Mul:
				return new IodineFloat (Value * op2);
			case BinaryOperation.Div:
				return new IodineFloat (Value / op2);
			case BinaryOperation.Mod:
				return new IodineFloat (Value % op2);
			case BinaryOperation.Equals:
				return new IodineBool (Value == op2);
			case BinaryOperation.NotEquals:
				return new IodineBool (Value != op2);
			case BinaryOperation.GreaterThan:
				return new IodineBool (Value > op2);
			case BinaryOperation.GreaterThanOrEqu:
				return new IodineBool (Value >= op2);
			case BinaryOperation.LessThan:
				return new IodineBool (Value < op2);
			case BinaryOperation.LessThanOrEqu:
				return new IodineBool (Value <= op2);
			default:
				return base.PerformBinaryOperation (vm, binop, rvalue);
			}
		}


		public override IodineObject PerformUnaryOperation (VirtualMachine vm, UnaryOperation op)
		{
			switch (op) {
			case UnaryOperation.Negate:
				return new IodineFloat (-this.Value);
			}
			return null;
		}
		public override void PrintTest ()
		{
			Console.WriteLine (this.Value);
		}

		public override string ToString ()
		{
			return Value.ToString ();
		}

		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}
	}
}

