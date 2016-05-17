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
    [BuiltinDocString (
        "Provides functions for inspecting live objects."
    )]
    [IodineBuiltinModule ("inspect")]
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
                if (instruction.Location != null) {
                    SetAttribute ("line", new IodineInteger (instruction.Location.Line));
                    SetAttribute ("col", new IodineInteger (instruction.Location.Column));
                    SetAttribute ("file", new IodineString (instruction.Location.File ?? ""));
                } else {

                    SetAttribute ("line", new IodineInteger (0));
                    SetAttribute ("col", new IodineInteger (0));
                    SetAttribute ("file", new IodineString (""));
                }
                switch (instruction.OperationCode) {
                case Opcode.LoadConst:
                case Opcode.StoreGlobal:
                case Opcode.LoadGlobal:
                case Opcode.StoreAttribute:
                case Opcode.LoadAttribute:
                case Opcode.LoadAttributeOrNull:
                    SetAttribute ("immediateref", method.Module.ConstantPool [instruction.Argument]);
                    break;
                default:
                    SetAttribute ("immediateref", IodineNull.Instance);
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
                        parentMethod.Module.ConstantPool [ins.Argument].ToString ());
                default:
                    return ins.OperationCode.ToString ();
                }
            }
        }

        public ReflectionModule ()
            : base ("inspect")
        {
            SetAttribute ("getbytecode", new BuiltinMethodCallback (GetBytecode, this));
            SetAttribute ("hasattribute", new BuiltinMethodCallback (HasAttribute, this));
            SetAttribute ("setattribute", new BuiltinMethodCallback (SetAttribute, this));
            SetAttribute ("getattribute", new BuiltinMethodCallback (GetAttribute, this));
            SetAttribute ("getattributes", new BuiltinMethodCallback (GetAttributes, this));
            SetAttribute ("getmembers", new BuiltinMethodCallback (GetAttributes, this));
            SetAttribute ("getcontracts", new BuiltinMethodCallback (GetInterfaces, this));
            // kept for compatibility
            SetAttribute ("getinterfaces", new BuiltinMethodCallback (GetInterfaces, this));
            SetAttribute ("loadModule", new BuiltinMethodCallback (LoadModule, this));
            SetAttribute ("isclass", new BuiltinMethodCallback (IsClass, this));
            SetAttribute ("istype", new BuiltinMethodCallback (IsType, this));
            SetAttribute ("ismethod", new BuiltinMethodCallback (IsMethod, this));
            SetAttribute ("isfunction", new BuiltinMethodCallback (IsFunction, this));
            SetAttribute ("isgeneratormethod", new BuiltinMethodCallback (IsGeneratorMethod, this));
            SetAttribute ("ismodule", new BuiltinMethodCallback (IsModule, this));
            SetAttribute ("isbuiltin", new BuiltinMethodCallback (IsBuiltin, this));
            SetAttribute ("isproperty", new BuiltinMethodCallback (IsProperty, this));
        }

        /**
         * Iodine Function: hasAttribute (obj, attr)
         * Description: Returns true if obj contains attribute attr
         */
        [BuiltinDocString (
            "Checks whether or not an object has a specific attribute",
            "@param obj The object",
            "@param attr : Str the attribute",
            "@returns Bool"
        )]
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

        [BuiltinDocString (
            "Gets a specific attribute of an object",
            "@param obj The object",
            "@param attr : Str the attribute",
            "@returns Object"
        )]
        private IodineObject GetAttribute (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 2) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            IodineObject o1 = args [0];
            IodineString str = args [1] as IodineString;
            if (str == null) {
                vm.RaiseException (new IodineTypeException ("Str"));
                return null;
            }
            if (!o1.HasAttribute (str.Value)) {
                return null;
            }
            return o1.GetAttribute (str.Value);
        }

        /**
         * Iodine Function: getAttributes (obj)
         * Description: Returns a hashmap containing all attributes found in obj
         */
        [BuiltinDocString (
            "Gets all attributes of an object",
            "@param obj The object",
            "@returns Dict"
        )]
        private IodineObject GetAttributes (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            IodineObject o1 = args [0];
            IodineObject func = null;

            if (args.Length > 1) {
                func = args [1];
            }

            IodineDictionary map = new IodineDictionary ();
            foreach (string key in o1.Attributes.Keys) {
                IodineObject value = o1.Attributes [key];

                if (func == null || func.Invoke (vm, new IodineObject[] { value }) == IodineBool.True) {
                    map.Set (new IodineString (key), o1.Attributes [key]);
                }
            }
            return map;
        }

        /**
         * Iodine Function: getInterfaces (obj)
         * Description: Returns a list of all interfaces obj implements
         */
        [BuiltinDocString (
            "Gets all contracts of an object",
            "@param obj The object",
            "@returns List"
        )]
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
        [BuiltinDocString (
            "Sets a specific attribute of an object",
            "@param obj The object",
            "@param attr : Str the attribute",
            "@param value The value"
        )]
        private IodineObject SetAttribute (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 3) {
                vm.RaiseException (new IodineArgumentException (3));
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

        [BuiltinDocString (
            "Loads a module",
            "@param path : Str The module",
            "@returns Module"
        )]
        private IodineObject LoadModule (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 1) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }
            IodineString pathStr = args [0] as IodineString;
            IodineModule module = vm.Context.LoadModule (pathStr.Value);
            module.Initializer.Invoke (vm, new IodineObject[] { });
            return module;
        }

        [BuiltinDocString (
            "Compiles source code into a module",
            "@param source : Str The source code",
            "@returns Module"
        )]
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
        [BuiltinDocString (
            "Decompiles a function to get its bytecode",
            "@param callable The function",
            "@returns List"
        )]
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
        [BuiltinDocString (
            "Checks if an object is a method",
            "@param obj The object",
            "@returns Bool"
        )]
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
        [BuiltinDocString (
            "Checks if an object is a generator",
            "@param obj The object",
            "@returns Bool"
        )]
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
        [BuiltinDocString (
            "Checks if an object is a method, function or closure",
            "@param obj The object",
            "@returns Bool"
        )]
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
        [BuiltinDocString (
            "Checks if an object is a builtin method",
            "@param obj The object",
            "@returns Bool"
        )]
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
        [BuiltinDocString (
            "Checks if an object is a class",
            "@param obj The object",
            "@returns Bool"
        )]
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
        [BuiltinDocString (
            "Checks if an object is a type",
            "@param obj The object",
            "@returns Bool"
        )]
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
        [BuiltinDocString (
            "Checks if an object is a module",
            "@param obj The object",
            "@returns Bool"
        )]
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
        [BuiltinDocString (
            "Checks if an object is a method property",
            "@param obj The object",
            "@returns Bool"
        )]
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

