/**
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
using Iodine.Compiler;
using Iodine.Compiler.Ast;
using Iodine.Runtime;

namespace Iodine.Engine
{
	public sealed class IodineEngine
	{
		/// <summary>
		/// The Iodine context
		/// </summary>
		public readonly IodineContext Context;

		private TypeRegistry typeRegistry = new TypeRegistry ();

		public dynamic this [string name] {
			get {
				return GetMember (name);
			}
			set {
				SetMember (name, value);
			}
		}

		public IodineEngine ()
		{
			Context = IodineContext.Create ();
		}

		public IodineEngine (IodineContext context)
		{
			Context = context;
		}

		/// <summary>
		/// Registers a class in the global namespace, allowing it to be
		/// instantiated in Iodine 
		/// </summary>
		/// <param name="name">Name of the class.</param>
		/// <typeparam name="T">The class.</typeparam>
		public void RegisterClass<T> (string name)
			where T : class
		{
			Type type = typeof(T);
			ClassWrapper wrapper = ClassWrapper.CreateFromType (typeRegistry, type, name);
			typeRegistry.AddTypeMapping (type, wrapper, null);
			Context.VirtualMachine.Globals [name] = wrapper;
		}

		public void RegisterClass (Type type, string name)
		{
			ClassWrapper wrapper = ClassWrapper.CreateFromType (typeRegistry, type, name);
			typeRegistry.AddTypeMapping (type, wrapper, null);
			Context.VirtualMachine.Globals [name] = wrapper;
		}

		/// <summary>
		/// Executes a string of Iodine source code
		/// </summary>
		/// <returns>The last object evaluated during the execute of the source.</returns>
		/// <param name="source">A string containing valid Iodine code..</param>
		public dynamic DoString (string source)
		{
			SourceUnit line = SourceUnit.CreateFromSource (source);
			Context.Invoke (line.Compile (Context), new IodineObject[] { });
			return null;
		}

		public dynamic DoFile (string file)
		{
			IodineModule main = new IodineModule (Path.GetFileNameWithoutExtension (file));
			DoString (File.ReadAllText (file));
			return new IodineDynamicObject (main, Context.VirtualMachine, typeRegistry);
		}

		private dynamic GetMember (string name)
		{
			IodineObject obj = null;
			if (Context.VirtualMachine.Globals.ContainsKey (name)) {
				obj = Context.VirtualMachine.Globals [name];
			}
			return IodineDynamicObject.Create (obj, Context.VirtualMachine, typeRegistry);
		}

		private void SetMember (string name, dynamic value)
		{
			IodineObject obj = typeRegistry.ConvertToIodineObject ((object)value);
			Context.VirtualMachine.Globals [name] = obj;
		}
	}
}

