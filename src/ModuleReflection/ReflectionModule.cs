using System;
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
	}
}

