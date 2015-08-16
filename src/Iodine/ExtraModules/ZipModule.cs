#if COMPILE_EXTRAS

using System;
using System.IO.Compression;
using Iodine.Runtime;

namespace Iodine.Modules.Extras
{
	[IodineBuiltinModule ("ziplib")]
	internal class ZipModule : IodineModule
	{
		public ZipModule () : base ("ziplib")
		{
			this.SetAttribute ("unzipToDirectory", new InternalMethodCallback (unzip, this));
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

#endif