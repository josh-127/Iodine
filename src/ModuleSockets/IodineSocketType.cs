using System;
using System.Net;
using System.Net.Sockets;
using Iodine;

namespace ModuleSockets
{
	public class IodineSocketType : IodineObject
	{
		private static IodineTypeDefinition SocketTypeTypeDef = new IodineTypeDefinition ("Socket");

		public SocketType Type
		{
			private set;
			get;
		}

		public IodineSocketType (SocketType sockType)
			: base (SocketTypeTypeDef)
		{
			this.Type = sockType;
		}


	}
}

