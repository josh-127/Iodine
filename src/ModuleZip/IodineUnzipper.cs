using System;
using System.Data;
using Iodine;
using System.Dynamic;
using System.Collections.Generic;
using Internals;

namespace ModuleZip
{
	public class IodineUnzipper : IodineObject
	{
		private static readonly IodineTypeDefinition UnzipperTypeDef = new IodineTypeDefinition ("Unzipper");
		public Unzip Unzipper {
			get;
			private set;
		}
		public IodineUnzipper (Unzip unzipper) : base(UnzipperTypeDef)
		{
			this.Unzipper = unzipper;
			this.SetAttribute ("close", new InternalMethodCallback (close, this));
			this.SetAttribute ("extractToDirectory", new InternalMethodCallback (extractTo, this));
		}

		public IodineObject extractTo (VirtualMachine vm, IodineObject self, IodineObject[] args) {
			var dir = args [0] as IodineString;
			var unzipper = self as IodineUnzipper;

			unzipper.Unzipper.ExtractToDirectory (dir.Value);
			return null;
		}

		public IodineObject close (VirtualMachine vm, IodineObject self, IodineObject[] args) {
			var unzipper = self as IodineUnzipper;
			unzipper.Unzipper.Dispose ();
			return null;
		}
	}
}