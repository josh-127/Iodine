using System;
using System.Collections.Generic;
using System.Dynamic;
using Iodine;
using System.IO.Compression;


namespace ModuleZip
{

	[IodineExtensionAttribute ("ziplib")]
	public class ZipModule : IodineModule
	{
		public ZipModule () : base ("ziplib")
		{
			this.SetAttribute ("unzipToDirectory", new InternalMethodCallback (unzip ,this));
		}

		private IodineObject unzip (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			var archiveName = args [0] as IodineString;
			var targetDir = args [1] as IodineString;
			ZipFile.ExtractToDirectory (archiveName.Value, targetDir.Value);
			return null;
		}

	}
}