﻿using System;
using System.Net;
using Iodine;

namespace ModuleWebClient
{
	public class IodineWebClient : IodineObject
	{
		private static IodineTypeDefinition WebClientTypeDef =
			new IodineTypeDefinition ("WebClient");

		private WebClient client;

		public IodineWebClient () 
			: base (WebClientTypeDef)
		{
			this.SetAttribute ("downloadString", new InternalMethodCallback (downloadString, this));
			this.SetAttribute ("downloadRaw", new InternalMethodCallback (downloadRaw, this));
			this.SetAttribute ("downloadFile", new InternalMethodCallback (downloadFile, this));
			WebProxy proxy = new WebProxy ();
			this.client = new WebClient ();
			this.client.Proxy = proxy;
		}

		private IodineObject downloadString (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
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

		private IodineObject downloadRaw (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
			IodineString uri = args [0] as IodineString;
			byte[] data;
			try {
				data = this.client.DownloadData (uri.ToString ());
			} catch (Exception e) {
				vm.RaiseException (e.Message);
				return null;
			}
			return new IodineByteArray (data);
		}

		private IodineObject downloadFile (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
			IodineString uri = args [0] as IodineString;
			IodineString file = args [1] as IodineString;
		
			try {
				this.client.DownloadFile (uri.ToString (), file.ToString ());
			} catch (Exception e) {
				vm.RaiseException (e.Message);
			}
			return null;
		
		}
	}
}

