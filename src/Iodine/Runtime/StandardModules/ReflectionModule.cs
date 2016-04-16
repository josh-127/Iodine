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
using System.Security.Cryptography;
using Iodine.Compiler;

namespace Iodine.Runtime
{
	[IodineBuiltinModule ("reflection")]
	public class ReflectionModule : IodineModule
	{
		class IodineInstruction : IodineObject
		{
			public static readonly IodineTypeDefinition TypeDefinition = new IodineTypeDefinition ("Instruction");

			public readonly Instruction Instruction;

			private IodineMethod parentMethod;

			public IodineInstruction (IodineMethod method, Instruction instruction)
				: base (TypeDefinition)
			{
				Instruction = instruction;
				parentMethod = method;
				SetAttribute ("opcode", new IodineInteger ((long)instruction.OperationCode));
				SetAttribute ("immediate", new IodineInteger (instruction.Argument));
				SetAttribute ("line", new IodineInteger (instruction.Location.Line));
				SetAttribute ("col", new IodineInteger (instruction.Location.Column));
				SetAttribute ("file", new IodineString (instruction.Location.File));

				switch (instruction.OperationCode) {
				case Opcode.LoadConst:
				case Opcode.StoreGlobal:
				case Opcode.LoadGlobal:
				case Opcode.StoreAttribute:
				case Opcode.LoadAttribute:
				case Opcode.LoadAttributeOrNull:

					SetAttribute ("immediateRef", method.Module.ConstantPool[instruction.Argument]);
					break;
				default:

					SetAttribute ("immediateRef", IodineNull.Instance);
					break;
				}


			}

			public override string ToString ()
			{
				Instruction ins = this.Instruction;
				switch (this.Instruction.OperationCode) {
				case Opcode.BinOp:
					return ((BinaryOperation)ins.Argument).ToString ();
				case Opcode.UnaryOp:
					return ((UnaryOperation)ins.Argument).ToString ();
				case Opcode.LoadConst:
				case Opcode.Invoke:
				case Opcode.BuildList:
				case Opcode.LoadLocal:
				case Opcode.StoreLocal:
				case Opcode.Jump:
				case Opcode.JumpIfTrue:
				case Opcode.JumpIfFalse:
					return String.Format ("{0} {1}", ins.OperationCode, ins.Argument);
				case Opcode.StoreAttribute:
				case Opcode.LoadAttribute:
				case Opcode.LoadGlobal:
				case Opcode.StoreGlobal:
					return String.Format ("{0} {1} ({2})", ins.OperationCode, ins.Argument, 
						parentMethod.Module.ConstantPool[ins.Argument].ToString ());
				default:
					return ins.OperationCode.ToString ();
				}
			}
		}
		public ReflectionModule ()
			: base ("reflection")
		{
			SetAttribute ("getBytecode", new BuiltinMethodCallback (GetBytecode, this));
			SetAttribute ("hasAttribute", new BuiltinMethodCallback (HasAttribute, this));
			SetAttribute ("setAttribute", new BuiltinMethodCallback (SetAttribute, this));
			SetAttribute ("getAttributes", new BuiltinMethodCallback (GetAttributes, this));
			SetAttribute ("getInterfaces", new BuiltinMethodCallback (GetInterfaces, this));
			SetAttribute ("loadModule", new BuiltinMethodCallback (LoadModule, this));
			SetAttribute ("compileModule", new BuiltinMethodCallback (CompileModule, this));
			SetAttribute ("isClass", new BuiltinMethodCallback (IsClass, this));
			SetAttribute ("isMethod", new BuiltinMethodCallback (IsMethod, this));
			SetAttribute ("isFunction", new BuiltinMethodCallback (IsFunction, this));
			SetAttribute ("isGeneratorMethod", new BuiltinMethodCallback (IsGeneratorMethod, this));
			SetAttribute ("isModule", new BuiltinMethodCallback (IsModule, this));
			SetAttribute ("isBuiltIn", new BuiltinMethodCallback (IsBuiltin, this));
			SetAttribute ("isProperty", new BuiltinMethodCallback (IsProperty, this));
		}

		/**
		 * Iodine Function: hasAttribute (obj, attr)
		 * Description: Returns true if obj contains attribute attr
		 */
		private IodineObject HasAttribute (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}
			IodineObject o1 = args [0];
			IodineString str = args [1] as IodineString;
			if (str == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}
			return IodineBool.Create (o1.HasAttribute (str.Value));
		}

		/**
		 * Iodine Function: getAttributes (obj)
		 * Description: Returns a hashmap containing all attributes found in obj
		 */
		private IodineObject GetAttributes (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 1) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineObject o1 = args [0];
			IodineHashMap map = new IodineHashMap ();
			foreach (string key in o1.Attributes.Keys) {
				map.Set (new IodineString (key), o1.Attributes [key]);
			}
			return map;
		}

		/**
		 * Iodine Function: getInterfaces (obj)
		 * Description: Returns a list of all interfaces obj implements
		 */
		private IodineObject GetInterfaces (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 1) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineObject o1 = args [0];
			IodineList list = new IodineList (o1.Interfaces.ToArray ());
			return list;
		}

		/**
		 * Iodine Function: setAttribute (obj, key, value)
		 * Description: Sets obj.key to value
		 */
		private IodineObject SetAttribute (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 3) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}
			IodineObject o1 = args [0];
			IodineString str = args [1] as IodineString;
			if (str == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}
			o1.SetAttribute (str.Value, args [2]);
			return null;
		}

		private IodineObject LoadModule (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString pathStr = args [0] as IodineString;
			IodineModule module = vm.Context.LoadModule (pathStr.Value);
			module.Initializer.Invoke (vm, new IodineObject[] { });
			return module;
		}

		private IodineObject CompileModule (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString source = args [0] as IodineString;
			SourceUnit unit = SourceUnit.CreateFromSource (source.Value);
			return unit.Compile (vm.Context);
		}

		/**
		 * Iodine Function: getBytecode (item)
		 * Description: Returns a list of instructions from an iodine method
		 */
		private IodineObject GetBytecode (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineMethod method = args [0] as IodineMethod;

			if (method == null && args [0] is IodineClosure) {
				method = ((IodineClosure)args [0]).Target;
			}

			IodineList ret = new IodineList (new IodineObject[] { });

			foreach (Instruction ins in method.Body) {
				ret.Add (new IodineInstruction (method, ins));
			}
			return ret;
		}

		/**
		 * Iodine Function: isMethod (item)
		 * Description: Returns true if item is an iodine method
		 */
		private IodineObject IsMethod (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length == 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineObject method = args [0];

			bool isMethod = method is IodineMethod || method is IodineBoundMethod;

			return IodineBool.Create (isMethod);
		}

		/**
		 * Iodine Function: isGeneratorMethod (item)
		 * Description: Returns true if item is an iodine generator
		 */
		private IodineObject IsGeneratorMethod (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length == 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineObject generator = args [0];

			bool isGenerator = generator is IodineGenerator;

			return IodineBool.Create (isGenerator);
		}

		/**
		 * Iodine Function: isFunction (item)
		 * Description: Returns true if item is an iodine function
		 */
		private IodineObject IsFunction (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length == 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineObject function = args [0];
			bool isFunction = function is IodineMethod || function is IodineBoundMethod || function is IodineClosure || function is IodineGenerator;

			return IodineBool.Create (isFunction);
		}

		/**
		 * Iodine Function: isBuiltin (item)
		 * Description: Returns true if item is a built in Iodine function
		 */
		private IodineObject IsBuiltin (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length == 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineObject builtin = args [0];

			bool isBuiltin = builtin is BuiltinMethodCallback;

			return IodineBool.Create (isBuiltin);
		}

		/**
		 * Iodine Function: isClass (item)
		 * Description: Returns true if item is an iodine class
		 */
		private IodineObject IsClass (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length == 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineObject clazz = args [0];

			bool isClass = clazz is IodineClass;

			return IodineBool.Create (isClass);
		}

		/**
		 * Iodine Function: isType (item)
		 * Description: Returns true if item is an iodine type
		 */
		private IodineObject IsType (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length == 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			IodineObject type = args [0];

			bool isType = type is IodineTypeDefinition;

			return IodineBool.Create (isType);
		}

		/**
		 * Iodine Function: isModule (item)
		 * Description: Returns true if item is an Iodine module
		 */
		private IodineObject IsModule (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length == 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineObject module = args [0];

			bool isModule = module is IodineModule;

			return IodineBool.Create (isModule);
		}

		/**
		 * Iodine Function: isProperty (item)
		 * Description: Returns true if item is an Iodine property
		 */
		private IodineObject IsProperty (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length == 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineObject property = args [0];

			bool isProperty = property is IIodineProperty;

			return IodineBool.Create (isProperty);
		}
	}
}

