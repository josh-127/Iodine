using System;

namespace Iodine
{
	public class IodineTypeDefinition : IodineObject
	{
		private static IodineTypeDefinition TypeDefTypeDef = new IodineTypeDefinition ("TypeDef");
		public string Name
		{
			private set;
			get;
		}

		public IodineTypeDefinition (string name)
			: base (TypeDefTypeDef)
		{
			this.Name = name;
			this.attributes["name"] = new IodineString (name);
		}

		public virtual void Inherit (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
		}
	}
}

