using System;

namespace Iodine
{
	public class IodineException : IodineObject
	{
		public static readonly IodineTypeDefinition TypeDefinition = new ExceptionTypeDef ();

		class ExceptionTypeDef : IodineTypeDefinition
		{
			public ExceptionTypeDef () 
				: base ("Exception")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
				}
				return new IodineException ("{0}", args[0].ToString ());
			}

		}

		public string Message {
			private set;
			get;
		}

		public Location Location {
			set;
			get;
		}

		public IodineException ()
			: base (TypeDefinition)
		{
		}

		public IodineException (string format, params object[] args)
			: base (TypeDefinition)
		{
			this.Message = String.Format (format, args);
			this.SetAttribute ("message", new IodineString (this.Message));
		}

		public IodineException (IodineTypeDefinition typeDef, string format, params object[] args)
			: base (typeDef)
		{
			this.Message = String.Format (format, args);
			this.SetAttribute ("message", new IodineString (this.Message));
		}
			
	}

	public class IodineTypeException : IodineException
	{
		public static new readonly IodineTypeDefinition TypeDefinition = new TypeExceptionTypeDef ();

		class TypeExceptionTypeDef : IodineTypeDefinition
		{
			public TypeExceptionTypeDef () 
				: base ("TypeException")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
				}
				return new IodineTypeException (args[0].ToString ());
			}
		}

		public IodineTypeException (string expectedType)
			: base (TypeDefinition, "Expected type '{0}'", expectedType) 
		{
			this.Base = new IodineException ();
		}
	}

	public class IodineIndexException : IodineException
	{
		public static new readonly IodineTypeDefinition TypeDefinition = new IndexExceptionTypeDef ();

		class IndexExceptionTypeDef : IodineTypeDefinition
		{
			public IndexExceptionTypeDef () 
				: base ("IndexException")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				return new IodineIndexException ();
			}
		}

		public IodineIndexException ()
			: base (TypeDefinition, "Index out of range!") 
		{
			this.Base = new IodineException ();
		}
	}

	public class IodineKeyNotFound : IodineException
	{
		public static new readonly IodineTypeDefinition TypeDefinition = new KeyNotFoundTypeDef ();

		class KeyNotFoundTypeDef : IodineTypeDefinition
		{
			public KeyNotFoundTypeDef () 
				: base ("KeyNotFoundException")
			{
			}

			public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
			{
				return new IodineKeyNotFound ();
			}
		}

		public IodineKeyNotFound ()
			: base (TypeDefinition, "Key not found!") 
		{
			this.Base = new IodineException ();
		}
	}

	public class IodineAttributeNotFoundException : IodineException
	{
		public static new readonly IodineTypeDefinition TypeDefinition = new AttributeNotFoundExceptionTypeDef ();

		class AttributeNotFoundExceptionTypeDef : IodineTypeDefinition
		{
			public AttributeNotFoundExceptionTypeDef () 
				: base ("AttributeNotFoundException")
			{
			}
		}

		public IodineAttributeNotFoundException (string name)
			: base (TypeDefinition, "Attribute '{0}' not found!", name) 
		{
			this.Base = new IodineException ();
		}
	}

	public class IodineInternalErrorException : IodineException
	{
		public static new readonly IodineTypeDefinition TypeDefinition = new InternalErrorExceptionTypeDef ();

		class InternalErrorExceptionTypeDef : IodineTypeDefinition
		{
			public InternalErrorExceptionTypeDef () 
				: base ("InternalException")
			{
			}
		}

		public IodineInternalErrorException (Exception ex)
			: base (TypeDefinition, "Internal exception: {0}\n Inner Exception: ",
				ex.Message, ex.InnerException == null ?  "" : ex.InnerException.Message) 
		{
			this.Base = new IodineException ();
		}
	}

	public class IodineArgumentException : IodineException
	{
		public static new readonly IodineTypeDefinition TypeDefinition = new ArgumentExceptionTypeDef ();

		class ArgumentExceptionTypeDef : IodineTypeDefinition
		{
			public ArgumentExceptionTypeDef () 
				: base ("ArgumentException")
			{
			}
		}

		public IodineArgumentException (int argCount)
			: base (TypeDefinition, "Expected {0} or more arguments!", argCount) 
		{
			this.Base = new IodineException ();
		}
	}

	public class IodineIOException : IodineException
	{
		public static new readonly IodineTypeDefinition TypeDefinition = new IOExceptionTypeDef ();

		class IOExceptionTypeDef : IodineTypeDefinition
		{
			public IOExceptionTypeDef () 
				: base ("IOException")
			{
			}
		}

		public IodineIOException (string msg)
			: base (TypeDefinition, msg) 
		{
			this.Base = new IodineException ();
		}
	}

	public class IodineSyntaxException : IodineException
	{
		public static new readonly IodineTypeDefinition TypeDefinition = new SyntaxExceptionTypeDef ();

		class SyntaxExceptionTypeDef : IodineTypeDefinition
		{
			public SyntaxExceptionTypeDef () 
				: base ("SynaxErrorException")
			{
			}
		}

		public IodineSyntaxException (ErrorLog errorLog)
			: base (TypeDefinition, "Syntax error") 
		{
			this.Base = new IodineException ();
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
					Console.WriteLine (" at {0} (Module: {1}, Line: {2})", top.Method.Name, top.Module.Name,
						top.Location.Line + 1);
				}
				top = top.Parent;
			}
		}
	}
}

