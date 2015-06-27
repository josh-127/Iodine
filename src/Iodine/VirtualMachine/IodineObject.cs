using System;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineObject
	{
		public IodineObject Base
		{
			set; get;
		}

		protected Dictionary<string, IodineObject> attributes = new Dictionary<string, IodineObject> ();

		public void SetAttribute (string name, IodineObject value)
		{
			if (value is IodineMethod) {
				IodineMethod method = (IodineMethod)value;
				if (method.InstanceMethod) {
					this.attributes[name] = new IodineInstanceMethodWrapper (this, method);
					return;
				}
			}
			this.attributes[name] = value;
		}

		public IodineObject GetAttribute (string name)
		{
			return this.attributes[name];
		}

		public bool HasAttribute (string name)
		{
			return this.attributes.ContainsKey (name);
		}

		public virtual void SetIndex (IodineObject key, IodineObject value)
		{
			
		}

		public virtual IodineObject GetIndex (IodineObject key)
		{
			return null;
		}

		public virtual IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			IodineObject[] arguments = new IodineObject[] { rvalue };
			switch (binop) {
			case BinaryOperation.Add:
				return GetAttribute ("_add").Invoke (vm, arguments); 
			case BinaryOperation.Sub:
				return GetAttribute ("_sub").Invoke (vm, arguments); 
			case BinaryOperation.Mul:
				return GetAttribute ("_mul").Invoke (vm, arguments); 
			case BinaryOperation.Div:
				return GetAttribute ("_div").Invoke (vm, arguments); 
			case BinaryOperation.And:
				return GetAttribute ("_and").Invoke (vm, arguments); 
			case BinaryOperation.Xor:
				return GetAttribute ("_xor").Invoke (vm, arguments); 
			case BinaryOperation.Or:
				return GetAttribute ("_or").Invoke (vm, arguments); 
			case BinaryOperation.Mod:
				return GetAttribute ("_mod").Invoke (vm, arguments); 
			case BinaryOperation.Equals:
				return GetAttribute ("_equals").Invoke (vm, arguments); 
			case BinaryOperation.NotEquals:
				return GetAttribute ("_notEquals").Invoke (vm, arguments); 
			case BinaryOperation.RightShift:
				return GetAttribute ("_rightShift").Invoke (vm, arguments); 
			case BinaryOperation.LeftShift:
				return GetAttribute ("_leftShift").Invoke (vm, arguments); 
			case BinaryOperation.LessThan:
				return GetAttribute ("_lessThan").Invoke (vm, arguments); 
			case BinaryOperation.GreaterThan:
				return GetAttribute ("_greaterThan").Invoke (vm, arguments); 
			case BinaryOperation.LessThanOrEqu:
				return GetAttribute ("_lessThanOrEqu").Invoke (vm, arguments); 
			case BinaryOperation.GreaterThanOrEqu:
				return GetAttribute ("_greaterThanOrEqu").Invoke (vm, arguments); 
			case BinaryOperation.BoolAnd:
				return GetAttribute ("_boolAnd").Invoke (vm, arguments); 
			case BinaryOperation.BoolOr:
				return GetAttribute ("_boolOr").Invoke (vm, arguments); 
			default:
				return null;
			}

		}

		public virtual IodineObject PerformUnaryOperation (VirtualMachine vm, UnaryOperation op)
		{
			vm.RaiseException ("Object does not support operator '{0}'", op.ToString ());
			return null;
		}

		public virtual IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			vm.RaiseException ("Object does not support invocation!");
			return new IodineObject ();
		}

		public virtual bool IsTrue () {
			return false;
		}

		public virtual IodineObject IterGetNext (VirtualMachine vm)
		{
			if (!HasAttribute ("_iterGetNext")) {
				vm.RaiseException ("_iterGetNext not implemented!");
				return null;
			}
			return GetAttribute("_iterGetNext").Invoke (vm, new IodineObject[]{});
		}

		public virtual bool IterMoveNext (VirtualMachine vm) 
		{
			if (!HasAttribute ("_iterMoveNext")) {
				vm.RaiseException ("_iterMoveNext not implemented!");
				return false;
			}
			return GetAttribute("_iterMoveNext").Invoke (vm, new IodineObject[]{}).IsTrue ();
		}

		public virtual void IterReset (VirtualMachine vm)
		{
			if (!HasAttribute ("_iterReset")) {
				vm.RaiseException ("_iterReset not implemented!");
				return;
			}
			GetAttribute("_iterReset").Invoke (vm, new IodineObject[]{});
		}

		public virtual void PrintTest ()
		{
		}

		public override int GetHashCode ()
		{
			int accum = 17;
			unchecked
			{
				foreach (IodineObject obj in this.attributes.Values) {
					accum += 23 * obj.GetHashCode ();
				}
			}
			return accum;
		}
	}
}

