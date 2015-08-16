using System;

namespace Iodine.Runtime
{
	public class IodineEnum : IodineTypeDefinition
	{
		private int nextVal = 0;
		public IodineEnum (string name)
			: base (name)
		{
		}

		public void AddItem (string name)
		{
			this.SetAttribute (name, new IodineInteger (nextVal++));
		}

		public void AddItem (string name, int val)
		{
			this.SetAttribute (name, new IodineInteger (val));
		}
	}
}

