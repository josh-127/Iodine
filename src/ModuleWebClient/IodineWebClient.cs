using System;
using System.Net;
using Iodine;

namespace ModuleWebClient
{
	public class IodineWebClient : IodineObject
	{
		private static IodineTypeDefinition WebClientTypeDef =
			new IodineTypeDefinition ("WebClient");

		private WebClient client;

		public IodineWebClient () : base (WebClientTypeDef) {
			this.SetAttribute ("downloadString", new InternalMethodCallback (downloadString, this));
			WebProxy proxy = new WebProxy ();
			this.client = new WebClient ();
			this.client.Proxy = proxy;
		}

		private IodineObject downloadString (VirtualMachine vm, IodineObject self, IodineObject[] args) {
			IodineString uri = args [0] as IodineString;
			string data;
			try {
				data = this.client.DownloadString (uri.ToString ());
			} catch (Exception e) {
				vm.RaiseException (e.Message);
				return null;
			}
			return new IodineString (data);
		}
	}
}

