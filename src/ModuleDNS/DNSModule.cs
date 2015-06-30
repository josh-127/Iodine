using System;
using System.Net;
using System.Net.Sockets;
using Iodine;

namespace ModuleDNS
{

	[IodineExtensionAttribute ("dns")]
	public class DNSModule : IodineModule
	{
		public DNSModule ()
			: base ("dns")
		{
			this.SetAttribute ("getHostEntry", new InternalMethodCallback (getHostEntry ,this));
		}

		private IodineObject getHostEntry (VirtualMachine vm, IodineObject self, IodineObject[] args) 
		{
			IodineString domain = args[0] as IodineString;
			return new IodineHostEntry (Dns.GetHostEntry (domain.Value));
		}

	}
}

