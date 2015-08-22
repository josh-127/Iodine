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
	[IodineBuiltinModule ("datetime")]
	public class DateTimeModule : IodineModule
	{
		public class IodineTimeStamp : IodineObject
		{
			public readonly static IodineTypeDefinition TimeStampTypeDef = new IodineTypeDefinition ("TimeStamp");

			public DateTime Value {
				private set;
				get;
			}

			public IodineTimeStamp (DateTime val)
				: base (TimeStampTypeDef)
			{
				this.Value = val;
				SetAttribute ("millisecond", new IodineInteger (val.Millisecond));
				SetAttribute ("second", new IodineInteger (val.Second));
				SetAttribute ("minute", new IodineInteger (val.Minute));
				SetAttribute ("hour", new IodineInteger (val.Hour));
				SetAttribute ("day", new IodineInteger (val.Day));
				SetAttribute ("month", new IodineInteger (val.Month));
				SetAttribute ("year", new IodineInteger (val.Year));
			}

			public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
			{
				if (rvalue is IodineTimeStamp) {
					IodineTimeStamp op = rvalue as IodineTimeStamp;
					switch (binop) {
					case BinaryOperation.GreaterThan:
						return new IodineBool (Value.CompareTo (op.Value) > 0);
					case BinaryOperation.LessThan:
						return new IodineBool (Value.CompareTo (op.Value) < 0);
					case BinaryOperation.GreaterThanOrEqu:
						return new IodineBool (Value.CompareTo (op.Value) >= 0);
					case BinaryOperation.LessThanOrEqu:
						return new IodineBool (Value.CompareTo (op.Value) <= 0);
					case BinaryOperation.Equals:
						return new IodineBool (Value.CompareTo (op.Value) == 0);
					}
				}
				return base.PerformBinaryOperation (vm, binop, rvalue);
			}
		}

		public DateTimeModule ()
			: base ("datetime")
		{
			SetAttribute ("now", new InternalMethodCallback (now, this));
		}

		private static IodineObject now (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineTimeStamp (DateTime.Now);
		}
	}

}

