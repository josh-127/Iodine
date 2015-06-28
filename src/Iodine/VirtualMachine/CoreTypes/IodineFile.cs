using System;
using System.IO;
using System.Text;

namespace Iodine
{
	public class IodineFile : IodineObject
	{
		private static readonly IodineTypeDefinition FileTypeDef = new IodineTypeDefinition ("File"); 

		public bool Closed { set; get;}

		public Stream File
		{
			private set;
			get;
		}

		public bool CanRead
		{
			private set;
			get;
		}

		public bool CanWrite
		{
			private set;
			get;
		}

		public IodineFile (Stream file, bool canWrite, bool canRead)
			: base (FileTypeDef)
		{
			this.File = file;
			this.CanRead = canRead;
			this.CanWrite = canWrite;
			this.SetAttribute ("write", new InternalMethodCallback (write, this));
			this.SetAttribute ("read", new InternalMethodCallback (read, this));
			this.SetAttribute ("readLine", new InternalMethodCallback (readLine, this));
			this.SetAttribute ("tell", new InternalMethodCallback (readLine, this));
			this.SetAttribute ("close", new InternalMethodCallback (close, this));
			this.SetAttribute ("readAllText", new InternalMethodCallback (readAllText, this));
		}

		private IodineObject write (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (this.Closed) { 
				vm.RaiseException (new IodineIOException ("Stream has been closed!"));
				return null;
			}

			if (!this.CanWrite) {
				vm.RaiseException (new IodineIOException ("Can not write to stream!"));
				return null;
			}

			foreach (IodineObject obj in args) {
				if (obj is IodineString) {
					write (obj.ToString ());
				} else if (obj is IodineInteger) {
					IodineInteger intVal = obj as IodineInteger;
					write ((byte)intVal.Value);
				} else {
					vm.RaiseException (new IodineTypeException (""));
				}
			}
			return null;
		}

		private IodineObject read (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (this.Closed) { 
				vm.RaiseException ("Stream has been closed!");
				return null;
			}

			if (!this.CanWrite) {
				vm.RaiseException ("Stream is not open for reading!");
				return null;
			}

			return new IodineInteger (File.ReadByte ());
		}

		private IodineObject readLine (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (this.Closed) { 
				vm.RaiseException ("Stream has been closed!");
				return null;
			}

			if (!this.CanWrite) {
				vm.RaiseException ("Stream is not open for reading!");
				return null;
			}

			return new IodineString (readLine ());
		}

		private IodineObject tell (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (this.Closed) { 
				vm.RaiseException ("Stream has been closed!");
			}
			return new IodineInteger (File.Position);
		}

		private IodineObject close (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (this.Closed) { 
				vm.RaiseException ("Stream has been closed!");
			}
			this.File.Close ();
			return null;
		}

		private IodineObject readAllText (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (this.Closed) { 
				vm.RaiseException ("Stream has been closed!");
			}

			StringBuilder builder = new StringBuilder ();
			int ch = 0;
			while ((ch = File.ReadByte ()) != -1) {
				builder.Append ((char)ch);
			}
			return new IodineString (builder.ToString ());
		}

		private void write (string str) 
		{
			foreach (char c in str) {
				this.File.WriteByte ((byte)c);
			}
		}

		public void write (byte b) 
		{
			this.File.WriteByte (b);
		}

		public string readLine ()
		{
			StringBuilder builder = new StringBuilder ();
			int ch = 0;
			while ((ch = File.ReadByte ()) != '\n' && ch != '\r' && ch != -1) {
				builder.Append ((char)ch);
			}
			return builder.ToString ();
		}
	}
}

