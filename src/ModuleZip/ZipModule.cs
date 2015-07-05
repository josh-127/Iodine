using System;
using System.Collections.Generic;
using System.Dynamic;
using Iodine;
using Internals;

namespace ModuleZip
{

	[IodineExtensionAttribute ("ziplib")]
	public class ZipModule : IodineModule
	{
		public ZipModule () : base ("ziplib")
		{
			this.SetAttribute ("unzip", new InternalMethodCallback (unzip ,this));
		}

		private IodineObject unzip (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			var archiveName = args [0] as IodineString;
			var unzipper = new Unzip (archiveName.Value);
			return new IodineUnzipper(unzipper);
		}

	}
}