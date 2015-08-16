using System;
using System.Net;
using Iodine;

namespace ModuleWebClient
{
	[IodineBuiltinModule ("webclient")]
	public class WebClientModule : IodineModule
	{
		public WebClientModule () : base ("webclient") {
			this.SetAttribute ("WebClient", new InternalMethodCallback (webclient, this));
			this.SetAttribute ("disableCertificateCheck", new InternalMethodCallback (disableCertCheck, this));
		}

		private IodineObject webclient (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineWebClient ();
		}

		private IodineObject disableCertCheck (VirtualMachine vm, IodineObject self, IodineObject[] args) {
			ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true; 
			return null;
		}
	}
}

