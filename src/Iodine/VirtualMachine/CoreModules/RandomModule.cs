using System;
using System.Security.Cryptography;

namespace Iodine
{
	public class RandomModule : IodineModule
	{
		private static Random rand = new Random ();
		private static RNGCryptoServiceProvider secureRand = new RNGCryptoServiceProvider ();

		public RandomModule ()
			: base ("random")
		{
			this.SetAttribute ("choice", new InternalMethodCallback (choice, this));
			this.SetAttribute ("cryptoString", new InternalMethodCallback (cryptoString, this));
		}

		private IodineObject cryptoString (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineInteger count = args[0] as IodineInteger;
			byte[] buf = new byte[(int)count.Value];
			secureRand.GetBytes (buf);
			return new IodineString (Convert.ToBase64String (buf).Substring (0, (int)count.Value));
		}

		private IodineObject choice (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineObject collection = args[0];
			int count = 0;
			collection.IterReset (vm);
			while (collection.IterMoveNext (vm)) {
				collection.IterGetNext (vm);
				count++;
			}

			int choice = rand.Next (0, count);
			count = 0;

			collection.IterReset (vm);
			while (collection.IterMoveNext (vm)) {
				IodineObject o = collection.IterGetNext (vm);
				if (count == choice)
					return o;
				count++;
			}

			return null;
		}
	}
}

