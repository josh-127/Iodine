﻿using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineObject
	{
		public static readonly IodineTypeDefinition ObjectTypeDef = new IodineTypeDefinition ("Object"); 
		
		public IodineObject Base {
			set; get;
		}

		public IodineTypeDefinition TypeDef {
			private set;
			get;
		}

		public Dictionary<string, IodineObject> Attributes {
			get {
				return this.attributes;
			}
		}

		protected Dictionary<string, IodineObject> attributes = new Dictionary<string, IodineObject> ();

		public IodineObject (IodineTypeDefinition typeDef)
		{
			this.TypeDef = typeDef;
			this.attributes["typeDef"] = typeDef;
		}

		public bool HasAttribute (string name)
		{
			bool res = this.attributes.ContainsKey (name);
			if (!res && this.Base != null) 
				return this.Base.HasAttribute (name);
			return res;
		}

		public void SetAttribute (string name, IodineObject value)
		{
			if (this.Base != null && !this.attributes.ContainsKey (name)) {
				if (this.Base.HasAttribute (name)) {
					this.Base.SetAttribute (name, value);
					return;
				}
			}

			if (value is IodineMethod) {
				IodineMethod method = (IodineMethod)value;
				if (method.InstanceMethod) {
					this.attributes[name] = new IodineInstanceMethodWrapper (this, method);
					return;
				}
			} else if (value is IodineInstanceMethodWrapper) {
				IodineInstanceMethodWrapper wrapper = (IodineInstanceMethodWrapper)value;
				this.attributes[name] = new IodineInstanceMethodWrapper (this, wrapper.Method);
				return;
			}
			this.attributes[name] = value;
		}


		public IodineObject GetAttribute (string name)
		{
			if (this.attributes.ContainsKey (name))
				return this.attributes[name];
			else if (this.Base != null && this.Base.Attributes.ContainsKey (name))
				return this.Base.GetAttribute (name);
			return null;
		}

		public virtual IodineObject GetAttribute (VirtualMachine vm, string name)
		{
			if (this.attributes.ContainsKey (name))
				return this.attributes[name];
			else if (this.Base != null && this.Base.Attributes.ContainsKey (name))
				return this.Base.GetAttribute (name);
			vm.RaiseException (new IodineAttributeNotFoundException (name));
			return null;
		}

		public virtual void SetIndex (VirtualMachine vm, IodineObject key, IodineObject value)
		{
			
		}

		public virtual IodineObject GetIndex (VirtualMachine vm, IodineObject key)
		{
			return null;
		}

		public virtual IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			IodineObject[] arguments = new IodineObject[] { rvalue };
			switch (binop) {
			case BinaryOperation.Add:
				return GetAttribute (vm, "_add").Invoke (vm, arguments); 
			case BinaryOperation.Sub:
				return GetAttribute (vm, "_sub").Invoke (vm, arguments); 
			case BinaryOperation.Mul:
				return GetAttribute (vm, "_mul").Invoke (vm, arguments); 
			case BinaryOperation.Div:
				return GetAttribute (vm, "_div").Invoke (vm, arguments); 
			case BinaryOperation.And:
				return GetAttribute (vm, "_and").Invoke (vm, arguments); 
			case BinaryOperation.Xor:
				return GetAttribute (vm, "_xor").Invoke (vm, arguments); 
			case BinaryOperation.Or:
				return GetAttribute (vm, "_or").Invoke (vm, arguments); 
			case BinaryOperation.Mod:
				return GetAttribute (vm, "_mod").Invoke (vm, arguments); 
			case BinaryOperation.Equals:
				if (HasAttribute ("_equals"))
					return GetAttribute (vm, "_equals").Invoke (vm, arguments); 
				else
					return new IodineBool (this == rvalue);
			case BinaryOperation.NotEquals:
				if (HasAttribute ("_notEquals"))
					return GetAttribute (vm, "_notEquals").Invoke (vm, arguments); 
				else
					return new IodineBool (this != rvalue);
			case BinaryOperation.RightShift:
				return GetAttribute (vm, "_rightShift").Invoke (vm, arguments); 
			case BinaryOperation.LeftShift:
				return GetAttribute (vm, "_leftShift").Invoke (vm, arguments); 
			case BinaryOperation.LessThan:
				return GetAttribute (vm, "_lessThan").Invoke (vm, arguments); 
			case BinaryOperation.GreaterThan:
				return GetAttribute (vm, "_greaterThan").Invoke (vm, arguments); 
			case BinaryOperation.LessThanOrEqu:
				return GetAttribute (vm, "_lessThanOrEqu").Invoke (vm, arguments); 
			case BinaryOperation.GreaterThanOrEqu:
				return GetAttribute (vm, "_greaterThanOrEqu").Invoke (vm, arguments); 
			case BinaryOperation.BoolAnd:
				return GetAttribute (vm, "_boolAnd").Invoke (vm, arguments); 
			case BinaryOperation.BoolOr:
				return GetAttribute (vm, "_boolOr").Invoke (vm, arguments); 
			default:
				return null;
			}

		}

		public virtual IodineObject PerformUnaryOperation (VirtualMachine vm, UnaryOperation op)
		{
			return null;
		}

		public virtual IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			vm.RaiseException ("Object does not support invocation!");
			return new IodineObject (null);
		}

		public virtual bool IsTrue () {
			return false;
		}

		public virtual IodineObject IterGetNext (VirtualMachine vm)
		{
			return GetAttribute("_iterGetNext").Invoke (vm, new IodineObject[]{});
		}

		public virtual bool IterMoveNext (VirtualMachine vm) 
		{
			return GetAttribute("_iterMoveNext").Invoke (vm, new IodineObject[]{}).IsTrue ();
		}

		public virtual void IterReset (VirtualMachine vm)
		{
			GetAttribute("_iterReset").Invoke (vm, new IodineObject[]{});
		}

		public virtual void PrintTest ()
		{
		}

		public bool InstanceOf (IodineTypeDefinition def)
		{
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
			unchecked
			{
				foreach (IodineObject obj in this.attributes.Values) {
					accum += 529 * obj.GetHashCode ();
				}
			}
			return accum;
		}

		public override string ToString ()
		{
			return this.TypeDef.Name;
		}
	}
}

