using System;
using System.IO;
using Iodine;

namespace ModuleReflection
{
	[IodineExtensionAttribute ("reflection")]
	public class ReflectionModule : IodineModule
	{
		public ReflectionModule ()
			: base ("reflection")
		{
			this.SetAttribute ("getBytecode", new InternalMethodCallback (getBytecode, this));
			this.SetAttribute ("hasAttribute", new InternalMethodCallback (hasAttribute, this));
			this.SetAttribute ("setAttribute", new InternalMethodCallback (setAttribute, this));
			this.SetAttribute ("getAttributes", new InternalMethodCallback (getAttributes, this));
			this.SetAttribute ("loadModule", new InternalMethodCallback (loadModule, this));
			this.SetAttribute ("compileModule", new InternalMethodCallback (compileModule, this));
			this.SetAttribute ("MethodBuilder", new InternalMethodCallback (loadModule, this));
			this.SetAttribute ("Opcode", IodineOpcode.OpcodeTypeDef);
		}

		private IodineObject hasAttribute (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 2) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}
			IodineObject o1 = args[0];
			IodineString str = args[1] as IodineString;
			if (str != null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}
			return new IodineBool (o1.HasAttribute (str.Value));
		}

		private IodineObject getAttributes (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 1) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineObject o1 = args[0];
			IodineMap map = new IodineMap ();
			foreach (string key in o1.Attributes.Keys) {
				map.Set (new IodineString (key), o1.Attributes[key]);
			}
			return map;
		}

		private IodineObject setAttribute (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length < 3) {
				vm.RaiseException (new IodineArgumentException (2));
				return null;
			}
			IodineObject o1 = args[0];
			IodineString str = args[1] as IodineString;
			if (str == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}
			o1.SetAttribute (str.Value, args[2]);
			return null;
		}

		private IodineObject loadModule (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString pathStr = args[0] as IodineString;
			return IodineModule.LoadModule (new ErrorLog (), pathStr.Value);
		}

		private IodineObject compileModule (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString pathStr = args[0] as IodineString;
			return IodineModule.CompileModuleFromSource (new ErrorLog (), pathStr.Value);
		}

		private IodineObject getBytecode (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineMethod method = args[0] as IodineMethod;

			IodineList ret = new IodineList (new IodineObject[] {});

			foreach (Instruction ins in method.Body) {
				ret.Add (new IodineInstruction (method, ins));
			}

			return ret;
		}

		private IodineObject methodBuilder (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString name = args[0] as IodineString;
			//IodineMethod method = new IodineMethod (
			//IodineMethodBuilder methBuilder = new IodineMethodBuilder (
			return null;
		}
	}
}

