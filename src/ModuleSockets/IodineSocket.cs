using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Iodine;

namespace ModuleSockets
{
	public class IodineSocket : IodineObject
	{
		private static IodineTypeDefinition SocketTypeDef = new IodineTypeDefinition ("Socket");

		public Socket Socket {
			private set;
			get;
		}

		private System.IO.Stream stream;
		private string host;

		private static bool ValidateServerCertificate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None)
				return true;
			throw new Exception ("Invalid certificate: " + sslPolicyErrors.ToString ());
		}

		public IodineSocket (Socket sock)
			: base (SocketTypeDef)
		{
			this.Socket = sock;
			this.SetAttribute ("connect", new InternalMethodCallback (connect ,this));
			this.SetAttribute ("connectSsl", new InternalMethodCallback (connectSsl, this));
			this.SetAttribute ("send", new InternalMethodCallback (send ,this));
			this.SetAttribute ("bind", new InternalMethodCallback (bind, this));
			this.SetAttribute ("accept", new InternalMethodCallback (accept, this));
			this.SetAttribute ("acceptSsl", new InternalMethodCallback (acceptSsl, this));
			this.SetAttribute ("listen", new InternalMethodCallback (listen, this));
			this.SetAttribute ("receive", new InternalMethodCallback (receive ,this));
			this.SetAttribute ("receiveRaw", new InternalMethodCallback (receiveRaw, this));
			this.SetAttribute ("getBytesAvailable", new InternalMethodCallback (getBytesAvailable ,this));
			this.SetAttribute ("readLine", new InternalMethodCallback (readLine ,this));
			this.SetAttribute ("getStream", new InternalMethodCallback (getStream ,this));
			this.SetAttribute ("close", new InternalMethodCallback (close, this));
			this.SetAttribute ("setHost", new InternalMethodCallback (setHost, this));
			this.SetAttribute ("connected", new InternalMethodCallback (connected, this));
			this.host = string.Empty;
		}

		public IodineSocket (SocketType sockType, ProtocolType protoType)
			: this (new Socket (sockType, protoType))
		{
		}
		
		private IodineObject connected (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			try {
				var result = !((this.Socket.Poll(1000, SelectMode.SelectRead)
					&& (this.Socket.Available == 0)) || !this.Socket.Connected);
				return new IodineBool (result);
			}
			catch (Exception e) {
				vm.RaiseException (e.Message);
				return null;
			}
			return null;
		}

		private IodineObject setHost (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString hostObj = args [0] as IodineString;
			this.host = hostObj.ToString ();
			return null;
		}

		private IodineObject close (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			this.Socket.Shutdown (SocketShutdown.Both);
			this.Socket.Close ();
			return null;
		}

		private IodineObject bind (VirtualMachine vm, IodineObject self, IodineObject[] args)
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
				this.Socket.Bind (new IPEndPoint (ipAddr, port));
			} catch {
				vm.RaiseException ("Could not bind to socket!");
				return null;
			}
			return null;
		}

		private IodineObject listen (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineInteger backlogObj = args[0] as IodineInteger;
			try {
				int backlog = (int)backlogObj.Value;
				this.Socket.Listen (backlog);
			} catch {
				vm.RaiseException ("Could not listen to socket!");
				return null;
			}
			return null;
		}

		private IodineSocket accept (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineSocket sock = new IodineSocket (this.Socket.Accept ());
			sock.stream = new NetworkStream (sock.Socket);
			return sock;
		}

		private IodineSocket acceptSsl (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineSocket sock = new IodineSocket (this.Socket.Accept ());
			sock.stream = new SslStream (new NetworkStream (this.Socket), false, ValidateServerCertificate, null);
			// I have no idea what I'm doing lol
			((SslStream)sock.stream).AuthenticateAsClient (this.host);
			return sock;
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

		private IodineObject connectSsl (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineString ipAddrStr = args [0] as IodineString;
			IodineInteger portObj = args [1] as IodineInteger;
			IPAddress ipAddr;
			int port = (int)portObj.Value;
			if (!IPAddress.TryParse (ipAddrStr.ToString (), out ipAddr)) {
				vm.RaiseException ("Invalid IP address!");
				return null;
			}

			try {
				this.Socket.Connect (ipAddr, port);
			} catch {
				vm.RaiseException ("Could not connect to socket!");
				return null;
			}

			try {
				this.stream = new SslStream (new NetworkStream (this.Socket), false, ValidateServerCertificate, null);
			} catch (Exception e) {
				vm.RaiseException (e.Message);
				return null;
			}

			try {
				((SslStream)this.stream).AuthenticateAsClient (this.host);
			} catch (Exception e) {
				vm.RaiseException (e.Message);
				return null;
			}

			return null;
		}

		private IodineObject getStream (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineStream (this.stream, true, true);
		}

		private IodineObject getBytesAvailable (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineInteger (this.Socket.Available);
		}

		private IodineObject send (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			foreach (IodineObject obj in args) {
				if (obj is IodineInteger) {
					this.stream.WriteByte ((byte)((IodineInteger)obj).Value);
					this.stream.Flush ();
				} else if (obj is IodineString) {
					var buf = Encoding.UTF8.GetBytes (obj.ToString ());
					this.stream.Write (buf, 0, buf.Length);
					this.stream.Flush ();
				}
			}
			return null;
		}

		private IodineByteArray receiveRaw (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineInteger n = args [0] as IodineInteger;
			byte[] buf = new byte[n.Value];
			for (int i = 0; i < n.Value; i++)
				buf [i] = (byte)stream.ReadByte ();
			return new IodineByteArray (buf);
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
