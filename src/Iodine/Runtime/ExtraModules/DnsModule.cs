#if COMPILE_EXTRAS

using System;
using System.Net;
using System.Net.Sockets;
using Iodine;

namespace Iodine.Modules.Extras
{
	[IodineBuiltinModule ("dns")]
	internal class DNSModule : IodineModule
	{
		public class IodineHostEntry : IodineObject
		{
			private static readonly IodineTypeDefinition HostEntryTypeDef = new IodineTypeDefinition ("HostEntry");

			public IPHostEntry Entry {
				private set;
				get;
			}

			public IodineHostEntry (IPHostEntry host)
				: base (HostEntryTypeDef)
			{
				this.Entry = host;
				IodineObject[] addresses = new IodineObject[this.Entry.AddressList.Length];
				int i = 0;
				foreach (IPAddress ip in this.Entry.AddressList) {
					addresses [i++] = new IodineString (ip.ToString ());
				}
				this.SetAttribute ("addressList", new IodineTuple (addresses));
			}

		}

		public DNSModule ()
			: base ("dns")
		{
			this.SetAttribute ("getHostEntry", new InternalMethodCallback (getHostEntry, this));
		}

		private IodineObject getHostEntry (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString domain = args [0] as IodineString;
			return new IodineHostEntry (Dns.GetHostEntry (domain.Value));
		}

	}
}


#endif