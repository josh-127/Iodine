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
			this.SetAttribute ("loadModule", new InternalMethodCallback (loadModule, this));
			this.SetAttribute ("MethodBuilder", new InternalMethodCallback (loadModule, this));
			this.SetAttribute ("Opcode", IodineOpcode.OpcodeTypeDef);
		}

		private IodineObject loadModule (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString pathStr = args[0] as IodineString;
			return IodineModule.LoadModule (new ErrorLog (), pathStr.Value);
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

