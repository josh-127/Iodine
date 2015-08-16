/**
  * Copyright (c) 2015, GruntTheDivine All rights reserved.

  * Redistribution and use in source and binary forms, with or without modification,
  * are permitted provided that the following conditions are met:
  * 
  *  * Redistributions of source code must retain the above copyright notice, this list
  *    of conditions and the following disclaimer.
  * 
  *  * Redistributions in binary form must reproduce the above copyright notice, this
  *    list of conditions and the following disclaimer in the documentation and/or
  *    other materials provided with the distribution.

  * Neither the name of the copyright holder nor the names of its contributors may be
  * used to endorse or promote products derived from this software without specific
  * prior written permission.
  * 
  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
  * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
  * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
  * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
  * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
  * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
  * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
  * CONTRACT ,STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
  * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
  * DAMAGE.
**/

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
				this.Attributes ["deleteValue"] = new InternalMethodCallback (deleteValue, this);
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
				IodineObject ioval = null;
				object val = Key.GetValue (name);
				IodineTypeConverter.Instance.ConvertFromPrimative (val, out ioval);
				return ioval;
			}

			private IodineObject deleteValue (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				string name = args [0].ToString ();
				Key.DeleteValue (name);
				return null;
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
			this.Attributes ["setValue"] = new InternalMethodCallback (setValue, null);
			this.Attributes ["getValue"] = new InternalMethodCallback (getValue, null);
		}

		private IodineObject setValue (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			string keyName = args [0].ToString ();
			string name = args [1].ToString ();
			IodineObject ioval = args [2];
			object val = null;
			IodineTypeConverter.Instance.ConvertToPrimative (ioval, out val);
			Registry.SetValue (keyName, name, val);
			return null;
		}

		private IodineObject getValue (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			string keyName = args [0].ToString ();
			string name = args [1].ToString ();
			object val = Registry.GetValue (keyName, name, null);
			IodineObject ioval = null;
			IodineTypeConverter.Instance.ConvertFromPrimative (val, out ioval);
			return ioval;
		}
	}
}

#endif