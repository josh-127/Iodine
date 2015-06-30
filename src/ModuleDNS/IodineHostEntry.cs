using System;
using System.Net;
using Iodine;

namespace ModuleDNS
{
	public class IodineHostEntry : IodineObject
	{
		private static readonly IodineTypeDefinition HostEntryTypeDef = new IodineTypeDefinition ("HostEntry");

		public IPHostEntry Entry
		{
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
				addresses[i++] = new IodineString (ip.ToString ());
			}
			this.SetAttribute ("addressList", new IodineTuple (addresses));
		}

	}
}

