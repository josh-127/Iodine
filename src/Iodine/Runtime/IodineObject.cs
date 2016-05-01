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
using System.Collections.Generic;
using Iodine.Util;
using Iodine.Compiler;

namespace Iodine.Runtime
{
    /// <summary>
    /// Base class for all Iodine objects. Everything in Iodine extends this, as
    /// everything in Iodine is an object
    /// </summary>
    public class IodineObject
    {
        public static readonly IodineTypeDefinition ObjectTypeDef = new IodineTypeDefinition ("Object");

        public readonly AttributeDictionary Attributes = new AttributeDictionary ();

        /// <summary>
        /// Gets or sets the base class
        /// </summary>
        /// <value>The base.</value>
        public IodineObject Base { set; get; }

        /// <summary>
        /// A list of contracts this object implements
        /// </summary>
        public readonly List<IodineContract> Interfaces = new List<IodineContract> ();

        public IodineTypeDefinition TypeDef {
            private set;
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Iodine.Runtime.IodineObject"/> class.
        /// </summary>
        /// <param name="typeDef">Type of this object.</param>
        public IodineObject (IodineTypeDefinition typeDef)
        {
            SetType (typeDef);
        }

        protected IodineObject ()
        {
        }

        /// <summary>
        /// Modifies the type of this object
        /// </summary>
        /// <param name="typeDef">The new type.</param>
        public void SetType (IodineTypeDefinition typeDef)
        {
            TypeDef = typeDef;
            Attributes ["__type__"] = typeDef;
            if (typeDef != null) {
                typeDef.BindAttributes (this);
            }
        }

        public bool HasAttribute (string name)
        {
            bool res = Attributes.ContainsKey (name);
            if (!res && Base != null)
                return Base.HasAttribute (name);
            return res;
        }

        public virtual void SetAttribute (VirtualMachine vm, string name, IodineObject value)
        {
            if (Base != null && !Attributes.ContainsKey (name)) {
                if (Base.HasAttribute (name)) {
                    Base.SetAttribute (vm, name, value);
                    return;
                }
            }
            SetAttribute (name, value);
        }

        public void SetAttribute (string name, IodineObject value)
        {
            if (value is IodineMethod) {
                IodineMethod method = (IodineMethod)value;
                if (method.InstanceMethod) {
                    Attributes [name] = new IodineBoundMethod (this, method);
                } else {
                    Attributes [name] = value;
                }
            } else if (value is BuiltinMethodCallback) {
                BuiltinMethodCallback callback = (BuiltinMethodCallback)value;
                callback.Self = this;
                Attributes [name] = value;
            } else if (value is IodineBoundMethod) {
                IodineBoundMethod wrapper = (IodineBoundMethod)value;
                Attributes [name] = new IodineBoundMethod (this, wrapper.Method);
            } else if (value is IodineProperty) {
                IodineProperty property = (IodineProperty)value;
                Attributes [name] = new IodineProperty (property.Getter, property.Setter, this); 
            } else {
                Attributes [name] = value;
            }
        }

        public IodineObject GetAttribute (string name)
        {
            IodineObject ret;
            Attributes.TryGetValue (name, out ret);

            bool hasAttribute = Attributes.TryGetValue (name, out ret) ||
                Base != null &&
                Base.Attributes.TryGetValue (name, out ret);

            if (hasAttribute) {
                return ret;
            }
            return null;
        }

        public virtual IodineObject GetAttribute (VirtualMachine vm, string name)
        {
            if (Attributes.ContainsKey (name))
                return Attributes [name];
            else if (Base != null && Base.HasAttribute (name))
                return Base.GetAttribute (name);
            vm.RaiseException (new IodineAttributeNotFoundException (name));
            return null;
        }

        /// <summary>
        /// Determines whether this instance is callable.
        /// </summary>
        /// <returns><c>true</c> if this instance is callable; otherwise, <c>false</c>.</returns>
        public virtual bool IsCallable ()
        {
            return Attributes.ContainsKey ("__invoke__") && Attributes ["__invoke__"].IsCallable ();
        }

        /// <summary>
        /// Returns a human friendly representation of this object
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="vm">Vm.</param>
        public virtual IodineObject ToString (VirtualMachine vm)
        {
            if (Attributes.ContainsKey ("__str__")) {
                return Attributes ["__str__"].Invoke (vm, new IodineObject[] { });
            }
            return new IodineString (ToString ());
        }

        /// <summary>
        /// Returns a string represention of this object. This representation *should* be valid
        /// Iodine code and pastable into an Iodine REPL. 
        /// </summary>
        /// <param name="vm">Vm.</param>
        public virtual IodineObject Represent (VirtualMachine vm)
        {
            if (Attributes.ContainsKey ("__repr__")) {
                return Attributes ["__repr__"].Invoke (vm, new IodineObject[] { });
            }
            return ToString (vm);
        }

        public virtual IodineObject Slice (VirtualMachine vm, IodineSlice slice)
        {

            if (Attributes.ContainsKey ("__getItem__")) {
                return Attributes ["__getItem__"].Invoke (vm, new IodineObject[] { slice });
            } else {

                return null;
            }

        }

        public virtual void SetIndex (VirtualMachine vm, IodineObject key, IodineObject value)
        {
            if (Attributes.ContainsKey ("__setItem__")) {
                Attributes ["__setItem__"].Invoke (vm, new IodineObject[] {
                    key,
                    value
                });
            } else {
                vm.RaiseException (new IodineNotSupportedException ("__setItem__ not implemented!"));
            }
        }

        public virtual IodineObject GetIndex (VirtualMachine vm, IodineObject key)
        {
            if (Attributes.ContainsKey ("__getItem__")) {
                return Attributes ["__getItem__"].Invoke (vm, new IodineObject[] { key });
            }
            return null;
        }

        public virtual bool Equals (IodineObject obj)
        {
            return Equals ((object)obj);
        }

        /// <summary>
        /// Dispatches the proper overload for the specified binary operator
        /// </summary>
        /// <returns>The result of invoking the overload for Binop.</returns>
        /// <param name="vm">Vm.</param>
        /// <param name="binop">Binary Operation.</param>
        /// <param name="rvalue">Right hand value.</param>
        public IodineObject PerformBinaryOperation (
            VirtualMachine vm,
            BinaryOperation binop,
            IodineObject rvalue)
        {
            switch (binop) {
            case BinaryOperation.Add:
                return Add (vm, rvalue);
            case BinaryOperation.Sub:
                return Sub (vm, rvalue);
            case BinaryOperation.Mul:
                return Mul (vm, rvalue);
            case BinaryOperation.Div:
                return Div (vm, rvalue);
            case BinaryOperation.And:
                return And (vm, rvalue);
            case BinaryOperation.Xor:
                return Xor (vm, rvalue);
            case BinaryOperation.Or:
                return Or (vm, rvalue);
            case BinaryOperation.Mod:
                return Mod (vm, rvalue);
            case BinaryOperation.Equals:
                return Equals (vm, rvalue);
            case BinaryOperation.NotEquals:
                return NotEquals (vm, rvalue);
            case BinaryOperation.RightShift:
                return RightShift (vm, rvalue);
            case BinaryOperation.LeftShift:
                return LeftShift (vm, rvalue);
            case BinaryOperation.LessThan:
                return LessThan (vm, rvalue);
            case BinaryOperation.GreaterThan:
                return GreaterThan (vm, rvalue);
            case BinaryOperation.LessThanOrEqu:
                return LessThanOrEqual (vm, rvalue);
            case BinaryOperation.GreaterThanOrEqu:
                return GreaterThanOrEqual (vm, rvalue);
            case BinaryOperation.BoolAnd:
                return LogicalAnd (vm, rvalue);
            case BinaryOperation.BoolOr:
                return LogicalOr (vm, rvalue);
            case BinaryOperation.HalfRange:
                return HalfRange (vm, rvalue);
            case BinaryOperation.ClosedRange:
                return ClosedRange (vm, rvalue);
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        /// <summary>
        /// Dispatches the overload for a specified unary operation
        /// </summary>
        /// <returns>The unary operation.</returns>
        /// <param name="vm">Vm.</param>
        /// <param name="op">Operand.</param>
        public virtual IodineObject PerformUnaryOperation (VirtualMachine vm, UnaryOperation op)
        {
            switch (op) {
            case UnaryOperation.Negate:
                return Negate (vm);
            case UnaryOperation.Not:
                return Not (vm);
            case UnaryOperation.BoolNot:
                return LogicalNot (vm);
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested unary operator has not been implemented"
            ));
            return null;
        }

        /// <summary>
        /// Calls this object as a method (Overloading the call operator)
        /// </summary>
        /// <param name="vm">Vm.</param>
        /// <param name="arguments">Arguments.</param>
        public virtual IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
        {
            if (HasAttribute ("__invoke__")) {
                return GetAttribute ("__invoke__").Invoke (vm, arguments);
            }
            vm.RaiseException (new IodineNotSupportedException (
                "Object does not support invocation")
            );
            return null;
        }

        /// <summary>
        /// Determines whether this instance is evaluates as true.
        /// </summary>
        /// <returns><c>true</c> if this instance evaluates as true; otherwise, <c>false</c>.</returns>
        public virtual bool IsTrue ()
        {
            return true;
        }

        /// <summary>
        /// Retrieves the length (Size) of this object. By default, this simply
        /// attempts to call self.__len__ ();
        /// </summary>
        /// <param name="vm">Vm.</param>
        public virtual IodineObject Len (VirtualMachine vm)
        {
            if (Attributes.ContainsKey ("__len__")) {
                return GetAttribute (vm, "__len__").Invoke (vm, new IodineObject[] { });
            }
            vm.RaiseException (new IodineAttributeNotFoundException ("__len__"));
            return null;
        }

        /// <summary>
        /// Called when entering a with statement
        /// </summary>
        /// <param name="vm">Vm.</param>
        public virtual void Enter (VirtualMachine vm)
        {
            if (Attributes.ContainsKey ("__enter__")) {
                GetAttribute (vm, "__enter__").Invoke (vm, new IodineObject[] { });
            }
        }

        /// <summary>
        /// Called when leaving a with statement
        /// </summary>
        /// <param name="vm">Vm.</param>
        public virtual void Exit (VirtualMachine vm)
        {
            if (Attributes.ContainsKey ("__exit__")) {
                GetAttribute (vm, "__exit__").Invoke (vm, new IodineObject[] { });
            }
        }

        #region Unary Operator Stubs

        /*
         * - unary prefix operator
         */
        public virtual IodineObject Negate (VirtualMachine vm)
        {
            if (Attributes.ContainsKey ("__negate__")) {
                return GetAttribute (vm, "__negate__").Invoke (vm, new IodineObject[] { });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested unary operator has not been implemented")
            );
            return null;
        }

        /*
         * ~ unary prefix operator
         */
        public virtual IodineObject Not (VirtualMachine vm)
        {
            if (Attributes.ContainsKey ("__invert__")) {
                return GetAttribute (vm, "__invert__").Invoke (vm, new IodineObject[] { });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested unary operator has not been implemented")
            );
            return null;
        }

       /*
        * ! unary prefix operator
        */
        public virtual IodineObject LogicalNot (VirtualMachine vm)
        {
            if (Attributes.ContainsKey ("__not__")) {
                return GetAttribute (vm, "__not__").Invoke (vm, new IodineObject[] { });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested unary operator has not been implemented")
            );
            return null;
        }

        #endregion

        #region Binary Operator Stubs

        public virtual IodineObject Add (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__add__")) {
                return GetAttribute (vm, "__add__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject Sub (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__sub__")) {
                return GetAttribute (vm, "__sub__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject Div (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__div__")) {
                return GetAttribute (vm, "__div__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject Mod (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__mod__")) {
                return GetAttribute (vm, "__mod__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject Mul (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__mul__")) {
                return GetAttribute (vm, "__mul__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject And (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__and__")) {
                return GetAttribute (vm, "__and__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject Xor (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__xor__")) {
                return GetAttribute (vm, "__xor__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject Or (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__or__")) {
                return GetAttribute (vm, "__or__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject Equals (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__equals__")) {
                return GetAttribute (vm, "__equals__").Invoke (vm, new IodineObject[] { left });
            }
            return IodineBool.Create (this == left);
        }

        public virtual IodineObject NotEquals (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__notequals__")) {
                return GetAttribute (vm, "__notequals__").Invoke (vm, new IodineObject[] { left });
            }
            return IodineBool.Create (this != left);
        }

        public virtual IodineObject RightShift (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__rightshift__")) {
                return GetAttribute (vm, "__rightshift__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject LeftShift (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__leftshit__")) {
                return GetAttribute (vm, "__leftshit__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject LessThan (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__lt__")) {
                return GetAttribute (vm, "__lt__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject GreaterThan (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__gt__")) {
                return GetAttribute (vm, "__gt__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }


        public virtual IodineObject LessThanOrEqual (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__lte__")) {
                return GetAttribute (vm, "__lte__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject GreaterThanOrEqual (VirtualMachine vm, IodineObject left)
        {
            if (Attributes.ContainsKey ("__gte__")) {
                return GetAttribute (vm, "__gte__").Invoke (vm, new IodineObject[] { left });
            }
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject LogicalAnd (VirtualMachine vm, IodineObject left)
        {
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject LogicalOr (VirtualMachine vm, IodineObject left)
        {
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject ClosedRange (VirtualMachine vm, IodineObject right)
        {
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        public virtual IodineObject HalfRange (VirtualMachine vm, IodineObject right)
        {
            vm.RaiseException (new IodineNotSupportedException (
                "The requested binary operator has not been implemented")
            );
            return null;
        }

        #endregion

        public virtual IodineObject GetIterator (VirtualMachine vm)
        {
            if (!Attributes.ContainsKey ("__iter__")) {
                vm.RaiseException (new IodineNotSupportedException ("__iter__ has not been implemented"));
                return null;
            }
            return GetAttribute ("__iter__").Invoke (vm, new IodineObject[]{ });
        }

        public virtual IodineObject IterGetCurrent (VirtualMachine vm)
        {
            if (!Attributes.ContainsKey ("__iterGetCurrent__")) {
                vm.RaiseException (new IodineNotSupportedException ("__iterGetCurrent__ has not been implemented"));
                return null;
            }
            return GetAttribute ("__iterGetCurrent__").Invoke (vm, new IodineObject[]{ });
        }

        public virtual bool IterMoveNext (VirtualMachine vm)
        {
            if (!Attributes.ContainsKey ("__iterMoveNext__")) {
                vm.RaiseException (new IodineNotSupportedException ("__iterMoveNext__ has not been implemented"));
                return false;
            }
            return GetAttribute ("__iterMoveNext__").Invoke (vm, new IodineObject[]{ }).IsTrue ();
        }

        public virtual void IterReset (VirtualMachine vm)
        {
            if (!Attributes.ContainsKey ("__iterReset__")) {
                vm.RaiseException (new IodineNotSupportedException ("__iterReset__ has not been implemented"));
                return;
            }
            GetAttribute ("__iterReset__").Invoke (vm, new IodineObject[]{ });
        }

        public bool InstanceOf (IodineTypeDefinition def)
        {
            if (def is IodineTrait) {

                IodineTrait trait = def as IodineTrait;

                return trait.HasTrait (this);
            }

            foreach (IodineContract contract in this.Interfaces) {
                if (contract == def) {
                    return true;
                }
            }
            IodineObject i = this;
            while (i != null) {
                if (i.TypeDef == def) {
                    return true;
                }
                i = i.Base;
            }
            return false;
        }

        public override int GetHashCode ()
        {
            int accum = 17;
            unchecked {
                foreach (IodineObject obj in Attributes.Values) {
                    if (obj != null) {
                        accum += 529 * obj.GetHashCode ();
                    }
                }
            }
            return accum;
        }

        public override string ToString ()
        {
            return TypeDef.Name;
        }
    }
}

