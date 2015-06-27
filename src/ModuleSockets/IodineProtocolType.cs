using System;
using System.Net;
using System.Net.Sockets;
using Iodine;

namespace ModuleSockets
{
	public class IodineProtocolType : IodineObject
	{
		private static IodineTypeDefinition SocketProtocalTypeTypeDef = new IodineTypeDefinition ("Socket");

		public ProtocolType Type
		{
			private set;
			get;
		}

		public IodineProtocolType (ProtocolType protoType)
			: base (SocketProtocalTypeTypeDef)
		{
			this.Type = protoType;
		}


	}
}

