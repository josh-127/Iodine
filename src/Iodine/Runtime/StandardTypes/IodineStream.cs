﻿/**
  * Copyright (c) 2015, GruntTheDivine All rights reserved.

  * Redistribution and use in source and binary forms, with or without modification,
  * are permitted provided that the following conditions are met:
  * 
  *  * Redistributions of source code must retain the above copyright notice, this list
  *    of conditions and the following disclaimer.
  * 
  *  * Redistributions in binary form must reproduce the above copyright notice, this
  *    list of conditions and the following disclaimer in the documentation and/or
  *    other materials provided with the distribution.

  * Neither the name of the copyright holder nor the names of its contributors may be
  * used to endorse or promote products derived from this software without specific
  * prior written permission.
  * 
  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
  * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
  * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
  * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
  * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
  * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
  * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
  * CONTRACT ,STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
  * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
  * DAMAGE.
**/

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Iodine.Runtime
{
	// TODO: Implement binary mode
	public class IodineStream : IodineObject
	{
		private static readonly IodineTypeDefinition TypeDefinition = new IodineTypeDefinition ("File");

		public bool Closed { set; get; }

		public Stream File { private set; get; }

		public bool CanRead { private set; get; }

		public bool CanWrite { private set; get; }

		public bool BinaryMode { private set; get; }

		public IodineStream (Stream file, bool canWrite, bool canRead)
			: base (TypeDefinition)
		{
			File = file;
			CanRead = canRead;
			CanWrite = canWrite;
			SetAttribute ("write", new BuiltinMethodCallback (Write, this));
			SetAttribute ("read", new BuiltinMethodCallback (Read, this));
			SetAttribute ("close", new BuiltinMethodCallback (Close, this));
			SetAttribute ("flush", new BuiltinMethodCallback (Flush, this));
			SetAttribute ("readAllText", new BuiltinMethodCallback (ReadAll, this));
		}


		public override IodineObject Len (VirtualMachine vm)
		{
			return new IodineInteger (File.Length);
		}

		public override void Exit (VirtualMachine vm)
		{
			if (!Closed) {
				File.Close ();
				File.Dispose ();
			}
		}

		/**
		 * Iodine Method: Stream.write (self, *args);
		 * Description: Writes each item in *args to the underlying stream
		 */
		private IodineObject Write (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (Closed) { 
				vm.RaiseException (new IodineIOException ("Stream has been closed!"));
				return null;
			}

			if (!CanWrite) {
				vm.RaiseException (new IodineIOException ("Can not write to stream!"));
				return null;
			}

			foreach (IodineObject obj in args) {
				if (obj is IodineString) {
					Write (obj.ToString ());
				} else if (obj is IodineBytes) {
					IodineBytes arr = obj as IodineBytes;
					File.Write (arr.Value, 0, arr.Value.Length);
				} else if (obj is IodineInteger) {
					IodineInteger intVal = obj as IodineInteger;
					Write ((byte)intVal.Value);
				} else if (obj is IodineByteArray) {
					IodineByteArray arr = obj as IodineByteArray;
					File.Write (arr.Array, 0, arr.Array.Length);
				} else {
					vm.RaiseException (new IodineTypeException (""));
				}
			}
			return null;
		}

		/**
		 * Iodine Method: Stream.read (self, [n]);
		 * Description: Reads n bytes from the underlying steam, or until the next line if n is not specified
		 */
		private IodineObject Read (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (Closed) { 
				vm.RaiseException ("Stream has been closed!");
				return null;
			}

			if (!CanRead) {
				vm.RaiseException ("Stream is not open for reading!");
				return null;
			}

			if (args.Length > 0) {
				IodineInteger intv = args [0] as IodineInteger;

				if (intv == null) {
					vm.RaiseException (new IodineTypeException ("Int"));
					return null;
				}

				byte[] buf = new byte[(int)intv.Value];
				File.Read (buf, 0, buf.Length);
				return new IodineString (Encoding.UTF8.GetString (buf));
			}

			return new IodineString (ReadLine ());
		}

		/**
		 * Iodine Method: Stream.tell (self);
		 * Description: Returns the current position of the underlying stream
		 */
		private IodineObject Tell (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (this.Closed) { 
				vm.RaiseException ("Stream has been closed!");
			}
			return new IodineInteger (File.Position);
		}

		/**
		 * Iodine Method: Stream.close (self);
		 * Description: Closes the underlying stream
		 */
		private IodineObject Close (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (this.Closed) { 
				vm.RaiseException ("Stream has been closed!");
			}
			File.Close ();
			return null;
		}

		/**
		 * Iodine Method: Stream.flush (self);
		 * Description: Flushes the underlying stream
		 */
		private IodineObject Flush (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (this.Closed) { 
				vm.RaiseException ("Stream has been closed!");
			}
			File.Flush ();
			return null;
		}


		/**
		 * Iodine Method: Stream.readAll (self);
		 * Description: Reads the entire file
		 */
		private IodineObject ReadAll (VirtualMachine vm, IodineObject self, IodineObject[] args)
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

		private void Write (string str)
		{
			foreach (char c in str) {
				File.WriteByte ((byte)c);
			}
		}

		public void Write (byte b)
		{
			File.WriteByte (b);
		}

		public string ReadLine ()
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
