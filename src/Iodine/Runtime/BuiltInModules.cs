using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Iodine.Runtime
{
	public static class BuiltInModules
	{
		public static readonly Dictionary<string, IodineModule> Modules = new Dictionary<string, IodineModule> ();

		static BuiltInModules ()
		{
			var modules = Assembly.GetExecutingAssembly ().GetTypes ()
				.Where (p => p.IsSubclassOf (typeof(IodineModule)));
			
			foreach (Type type in modules) {
				IodineBuiltinModule attr = type.GetCustomAttribute <IodineBuiltinModule> ();
				if (attr != null) {
					Modules.Add (attr.Name, (IodineModule)Activator.CreateInstance (type));
				}
			}
		}
	}
}

