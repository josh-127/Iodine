using System;

namespace Iodine
{
	public class IodineException : IodineObject
	{
		public string Message
		{
			private set;
			get;
		}

		public IodineException (string format, params object[] args)
		{
			this.Message = String.Format (format, args);
			this.SetAttribute ("message", new IodineString (this.Message));
		}
	}

	public class UnhandledIodineExceptionException : Exception
	{
		public IodineException OriginalException
		{
			private set;
			get;
		}

		public UnhandledIodineExceptionException (IodineException original)
		{
			this.OriginalException = original;
		}
	}
}

