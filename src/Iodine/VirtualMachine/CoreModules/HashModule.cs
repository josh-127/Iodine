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
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			byte[] bytes = new byte[]{};
			byte[] hash = null;

			SHA256Managed hashstring = new SHA256Managed();

			if (args[0] is IodineString) {
				bytes = System.Text.Encoding.UTF8.GetBytes (args[0].ToString ());
				hash = hashstring.ComputeHash(bytes);
			} else if (args[0] is IodineByteArray) {
				bytes = ((IodineByteArray)args[0]).Array;
				hash = hashstring.ComputeHash(bytes);
			} else if (args[0] is IodineStream) {
				hash = hashstring.ComputeHash(((IodineStream)args[0]).File);
			} else {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			return new IodineByteArray (hash);
		}

		private IodineObject sha1 (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			byte[] bytes = new byte[]{};
			byte[] hash = null;

			SHA1Managed hashstring = new SHA1Managed();

			if (args[0] is IodineString) {
				bytes = System.Text.Encoding.UTF8.GetBytes (args[0].ToString ());
				hash = hashstring.ComputeHash(bytes);
			} else if (args[0] is IodineByteArray) {
				bytes = ((IodineByteArray)args[0]).Array;
				hash = hashstring.ComputeHash(bytes);
			} else if (args[0] is IodineStream) {
				hash = hashstring.ComputeHash(((IodineStream)args[0]).File);
			} else {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			return new IodineByteArray (hash);
		}

		private IodineObject sha512 (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}

			byte[] bytes = new byte[]{};
			byte[] hash = null;

			SHA512Managed hashstring = new SHA512Managed();

			if (args[0] is IodineString) {
				bytes = System.Text.Encoding.UTF8.GetBytes (args[0].ToString ());
				hash = hashstring.ComputeHash(bytes);
			} else if (args[0] is IodineByteArray) {
				bytes = ((IodineByteArray)args[0]).Array;
				hash = hashstring.ComputeHash(bytes);
			} else if (args[0] is IodineStream) {
				hash = hashstring.ComputeHash(((IodineStream)args[0]).File);
			} else {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			return new IodineByteArray (hash);
		}
	}
}

