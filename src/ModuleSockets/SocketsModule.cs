using System;
using System.Net;
using System.Net.Sockets;
using Iodine;

namespace ModuleSockets
{

	[IodineExtensionAttribute ("socket")]
	public class SocketModule : IodineModule
	{
		public SocketModule ()
			: base ("socket")
		{
			this.SetAttribute ("SOCK_DGRAM", new IodineSocketType (SocketType.Dgram));
			this.SetAttribute ("SOCK_RAW", new IodineSocketType (SocketType.Raw));
			this.SetAttribute ("SOCK_RDM", new IodineSocketType (SocketType.Rdm));
			this.SetAttribute ("SOCK_SEQPACKET", new IodineSocketType (SocketType.Seqpacket));
			this.SetAttribute ("SOCK_STREAM", new IodineSocketType (SocketType.Stream));
			this.SetAttribute ("PROTO_TCP", new IodineProtocolType (ProtocolType.Tcp));
			this.SetAttribute ("PROTO_IP", new IodineProtocolType (ProtocolType.IP));
			this.SetAttribute ("PROTO_IPV4", new IodineProtocolType (ProtocolType.IPv4));
			this.SetAttribute ("PROTO_IPV6", new IodineProtocolType (ProtocolType.IPv6));
			this.SetAttribute ("PROTO_UDP", new IodineProtocolType (ProtocolType.Udp));
			this.SetAttribute ("socket", new InternalMethodCallback (socket, this));
		}

		private IodineObject socket (VirtualMachine vm, IodineObject self, IodineObject[] args) 
		{
			IodineSocketType sockType = args[0] as IodineSocketType;
			IodineProtocolType protoType = args[1] as IodineProtocolType;
			return new IodineSocket (sockType.Type, protoType.Type);
		}

	}
}

