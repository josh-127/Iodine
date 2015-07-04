using System;

namespace Iodine
{
	public class IodineException : IodineObject
	{
		private static readonly IodineTypeDefinition ExceptionTypeDef = new IodineTypeDefinition ("Exception");

		public string Message {
			private set;
			get;
		}

		public IodineException (string format, params object[] args)
			: base (ExceptionTypeDef)
		{
			this.Message = String.Format (format, args);
			this.SetAttribute ("message", new IodineString (this.Message));
		}
	}

	public class IodineTypeException : IodineException
	{
		public IodineTypeException (string expectedType)
			: base ("Expected type '{0}'", expectedType) 
		{

		}
	}

	public class IodineIndexException : IodineException
	{
		public IodineIndexException ()
			: base ("Index out of range!") 
		{

		}
	}

	public class IodineKeyNotFound : IodineException
	{
		public IodineKeyNotFound ()
			: base ("Key not found in HashMap!") 
		{

		}
	}

	public class IodineAttributeNotFoundException : IodineException
	{
		public IodineAttributeNotFoundException (string expectedType)
			: base ("Attribute '{0}' not found!", expectedType) 
		{

		}
	}

	public class IodineInternalErrorException : IodineException
	{
		public IodineInternalErrorException (Exception ex)
			: base ("Internal exception: {0}\n Inner Exception: ",
				ex.Message, ex.InnerException == null ?  "" : ex.InnerException.Message) 
		{
		}
	}

	public class IodineArgumentException : IodineException
	{
		public IodineArgumentException (int argCount)
			: base ("Expected {0} or more arguments!", argCount) 
		{

		}
	}

	public class IodineIOException : IodineException
	{
		public IodineIOException (string expectedType)
			: base ("Attribute '{0}' not found!", expectedType) 
		{

		}
	}

	public class IodineSyntaxException : IodineException
	{
		public IodineSyntaxException (ErrorLog errorLog)
			: base ("Syntax error") 
		{
		}
	}

	public class UnhandledIodineExceptionException : Exception
	{
		public IodineException OriginalException
		{
			private set;
			get;
		}

		public StackFrame Frame
		{
			private set;
			get;
		}

		public UnhandledIodineExceptionException (StackFrame frame, IodineException original)
		{
			this.OriginalException = original;
			this.Frame = frame;
		}

		public void PrintStack ()
		{
			StackFrame top = this.Frame;
			Console.WriteLine ("Stack trace:");
			Console.WriteLine ("------------");
			while (top != null) {
				if (top is NativeStackFrame) {
					NativeStackFrame frame = top as NativeStackFrame;

					Console.WriteLine (" at {0} <internal method>", frame.NativeMethod.Callback.Method.Name);
				} else {
					Console.WriteLine (" at {0} (Module: {1})", top.Method.Name, top.Module.Name);
				}
				top = top.Parent;
			}
		}
	}
}

