using System;
using System.Net;
using Iodine;

namespace ModuleWebClient
{
	[IodineExtension ("webclient")]
	public class WebClientModule : IodineModule
	{
		public WebClientModule () : base ("webclient") {
			this.SetAttribute ("WebClient", new InternalMethodCallback (webclient, this));
		}

		private IodineObject webclient (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineWebClient ();
		}
	}
}

