using System;

namespace Iodine
{
	public class IodineTypeDefinition : IodineObject
	{
		private static IodineTypeDefinition TypeDefTypeDef = new IodineTypeDefinition ("TypeDef");
		public string Name {
			private set;
			get;
		}

		public IodineTypeDefinition (string name)
			: base (TypeDefTypeDef)
		{
			this.Name = name;
			this.attributes ["name"] = new IodineString (name);
		}

		public virtual void Inherit (VirtualMachine vm, IodineObject self, IodineObject[] arguments)
		{
			IodineObject obj = new IodineObject (this);
			foreach (string attr in this.attributes.Keys) {
				if (!self.HasAttribute (attr))
					self.SetAttribute (attr, this.attributes[attr]);
				obj.SetAttribute (attr, this.attributes[attr]);
			}
			self.SetAttribute ("__super__", obj);
			self.Base = obj;
		}

		public override string ToString ()
		{
			return this.Name;
		}
	}
}

