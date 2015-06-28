using System;

namespace Iodine
{
	public class RandomModule : IodineModule
	{
		private static Random rand = new Random ();
		public RandomModule ()
			: base ("random")
		{
			this.SetAttribute ("choice", new InternalMethodCallback (choice, this));
		}

		private IodineObject choice (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			IodineObject collection = args[0];
			int count = 0;
			collection.IterReset (vm);
			while (collection.IterMoveNext (vm)) {
				IodineObject o = collection.IterGetNext (vm);
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

