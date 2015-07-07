using System;

namespace Iodine
{
	public class DateTimeModule : IodineModule
	{
		public class IodineTimeStamp : IodineObject
		{
			public readonly static IodineTypeDefinition TimeStampTypeDef = new IodineTypeDefinition ("TimeStamp");

			public DateTime Value {
				private set;
				get;
			}

			public IodineTimeStamp (DateTime val)
				: base (TimeStampTypeDef)
			{
				this.Value = val;
				this.SetAttribute ("millisecond", new IodineInteger (val.Millisecond));
				this.SetAttribute ("minute", new IodineInteger (val.Minute));
				this.SetAttribute ("hour", new IodineInteger (val.Hour));
				this.SetAttribute ("day", new IodineInteger (val.Day));
				this.SetAttribute ("month", new IodineInteger (val.Month));
				this.SetAttribute ("year", new IodineInteger (val.Year));
			}
		}

		public DateTimeModule ()
			: base ("datetime")
		{
			this.SetAttribute ("now", new InternalMethodCallback (now, this));
		}

		private static IodineObject now (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			return new IodineTimeStamp (DateTime.Now);
		}
	}

}

