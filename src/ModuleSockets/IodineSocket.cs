using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Iodine;

namespace ModuleSockets
{
	public class IodineSocket : IodineObject
	{
		private static IodineTypeDefinition SocketTypeDef = new IodineTypeDefinition ("Socket");

		public Socket Socket
		{
			private set;
			get;
		}
		private NetworkStream stream;

		public IodineSocket (SocketType sockType, ProtocolType protoType)
			: base (SocketTypeDef)
		{
			this.Socket = new Socket (sockType, protoType);
			this.SetAttribute ("connect", new InternalMethodCallback (connect ,this));
			this.SetAttribute ("send", new InternalMethodCallback (send ,this));
			this.SetAttribute ("receive", new InternalMethodCallback (receive ,this));
			this.SetAttribute ("getBytesAvailable", new InternalMethodCallback (getBytesAvailable ,this));
			this.SetAttribute ("readLine", new InternalMethodCallback (readLine ,this));
		}

		private IodineObject connect (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString ipAddrStr = args[0] as IodineString;
			IodineInteger portObj = args[1] as IodineInteger;
			IPAddress ipAddr;
			int port = (int)portObj.Value;
			if (!IPAddress.TryParse (ipAddrStr.ToString (), out ipAddr)) {
				vm.RaiseException ("Invalid IP address!");
				return null;
			} 

			try {
				this.Socket.Connect (ipAddr, port);
				this.stream = new NetworkStream (this.Socket);
			} catch {
				vm.RaiseException ("Could not connect to socket!");
				return null;
			}
			return null;
		}

		private IodineObject getBytesAvailable (VirtualMachine vm, IodineObject self, IodineObject[] args) 
		{
			return new IodineInteger (this.Socket.Available);
		}

		private IodineObject send (VirtualMachine vm, IodineObject self, IodineObject[] args) 
		{
			foreach (IodineObject obj in args) {
				if (obj is IodineInteger) {
					this.Socket.Send (new byte[] { (byte)((IodineInteger)obj).Value });
				} else if (obj is IodineString) {
					this.Socket.Send (Encoding.UTF8.GetBytes (obj.ToString ()));
				}
			}
			return null;
		}

		private IodineObject receive (VirtualMachine vm, IodineObject self, IodineObject[] args) 
		{
			IodineInteger n = args[0] as IodineInteger;
			StringBuilder accum = new StringBuilder ();
			for (int i = 0; i < n.Value; i++) {
				int b = this.stream.ReadByte ();
				if (b != -1)
					accum.Append ((char)b);
			}
			return new IodineString (accum.ToString ());
		}

		private IodineObject readLine (VirtualMachine vm, IodineObject self, IodineObject[] args) 
		{
			StringBuilder accum = new StringBuilder ();
			int b = this.stream.ReadByte ();
			while (b != -1 && b != '\n') {
				accum.Append ((char)b);
				b = this.stream.ReadByte ();
			}
			return new IodineString (accum.ToString ());
		}
	}
}

