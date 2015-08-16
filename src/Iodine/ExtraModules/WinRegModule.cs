#if COMPILE_EXTRAS

using System;
using Microsoft.Win32;
using Iodine.Runtime;

namespace Iodine.Modules.Extras
{
	[IodineBuiltinModule ("winreg")]
	internal class WinRegModule : IodineModule
	{
		class IodineRegistryKeyHandle : IodineObject
		{
			private static new readonly RegistryKeyHandleTypeDef TypeDef = new RegistryKeyHandleTypeDef ();

			class RegistryKeyHandleTypeDef : IodineTypeDefinition
			{
				public RegistryKeyHandleTypeDef ()
					: base ("RegistryKeyHandle")
				{

				}
			}

			public RegistryKey Key {
				private set;
				get;
			}

			public IodineRegistryKeyHandle (RegistryKey original)
				: base (TypeDef)
			{
				this.Key = original;
				this.Attributes ["setValue"] = new InternalMethodCallback (setValue, this);
				this.Attributes ["getValue"] = new InternalMethodCallback (getValue, this);
			}

			private IodineObject setValue (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				string name = args [0].ToString ();
				IodineObject ioval = args [1];
				object val = null;
				IodineTypeConverter.Instance.ConvertToPrimative (ioval, out val);
				Key.SetValue (name, val);
				return null;
			}

			private IodineObject getValue (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				string name = args [0].ToString ();
				IodineObject ioval = args [1];
				object val = Key.GetValue (name);
				IodineTypeConverter.Instance.ConvertFromPrimative (val, out ioval);
				return ioval;
			}
		}

		class IodineRegistryHive : IodineObject
		{
			private static new readonly RegistryHiveTypeDef TypeDef = new RegistryHiveTypeDef ();

			class RegistryHiveTypeDef : IodineTypeDefinition
			{
				public RegistryHiveTypeDef ()
					: base ("RegistryHive")
				{

				}
			}

			public RegistryHive Hive {
				private set;
				get;
			}

			public IodineRegistryHive (RegistryHive original)
				: base (TypeDef)
			{
				this.Hive = original;
			}
		}

		public WinRegModule ()
			: base ("winreg")
		{
			this.Attributes ["HKEY_CLASSES_ROOT"] = new IodineRegistryHive (RegistryHive.ClassesRoot); 
			this.Attributes ["HKEY_CURRENT_CONFIG"] = new IodineRegistryHive (RegistryHive.CurrentConfig); 
			this.Attributes ["HKEY_CURRENT_USER"] = new IodineRegistryHive (RegistryHive.CurrentUser); 
			this.Attributes ["HKEY_LOCAL_MACHINE"] = new IodineRegistryHive (RegistryHive.LocalMachine); 
			this.Attributes ["HKEY_USERS"] = new IodineRegistryHive (RegistryHive.Users);
			this.Attributes ["ClassesRoot"] = new IodineRegistryKeyHandle (Registry.ClassesRoot);
			this.Attributes ["CurrentConfig"] = new IodineRegistryKeyHandle (Registry.CurrentConfig);
			this.Attributes ["CurrentUser"] = new IodineRegistryKeyHandle (Registry.CurrentUser);
			this.Attributes ["LocalMachine"] = new IodineRegistryKeyHandle (Registry.LocalMachine);
		}

	}
}

#endif