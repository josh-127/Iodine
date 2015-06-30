using System;
using System.Security.Cryptography;

namespace Iodine
{
	public class HashModule : IodineModule
	{
		public HashModule ()
			: base ("hash")
		{
			this.SetAttribute ("sha1", new InternalMethodCallback (sha1, this));
			this.SetAttribute ("sha256", new InternalMethodCallback (sha256, this));
			this.SetAttribute ("sha512", new InternalMethodCallback (sha512, this));
		}

		private IodineObject sha256 (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			byte[] bytes = new byte[]{};

			if (args[0] is IodineString) {
				bytes = System.Text.Encoding.UTF8.GetBytes (args[0].ToString ());
			} else if (args[0] is IodineByteArray) {
				bytes = ((IodineByteArray)args[0]).Array;
			}

			SHA256Managed hashstring = new SHA256Managed();
			byte[] hash = hashstring.ComputeHash(bytes);
			return new IodineByteArray (hash);
		}

		private IodineObject sha1 (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			byte[] bytes = new byte[]{};

			if (args[0] is IodineString) {
				bytes = System.Text.Encoding.UTF8.GetBytes (args[0].ToString ());
			} else if (args[0] is IodineByteArray) {
				bytes = ((IodineByteArray)args[0]).Array;
			}

			SHA1Managed hashstring = new SHA1Managed();
			byte[] hash = hashstring.ComputeHash(bytes);
			return new IodineByteArray (hash);
		}

		private IodineObject sha512 (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			byte[] bytes = new byte[]{};

			if (args[0] is IodineString) {
				bytes = System.Text.Encoding.UTF8.GetBytes (args[0].ToString ());
			} else if (args[0] is IodineByteArray) {
				bytes = ((IodineByteArray)args[0]).Array;
			}

			SHA512Managed hashstring = new SHA512Managed();
			byte[] hash = hashstring.ComputeHash(bytes);
			return new IodineByteArray (hash);
		}
	}
}

