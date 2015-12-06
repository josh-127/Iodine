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
	/*
	 * TODO: Make this work again
	 */
	public sealed class IodineEngine
	{
		private TypeRegistry typeRegistry = new TypeRegistry ();
		private IodineModule defaultModule;
		private IodineContext context = new IodineContext ();

		public IodineEngine (IodineContext context)
		{
			/*
			defaultModule = new IodineModule ("__main__");
			*/
		}

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
		}

		public void RegisterClass<T> (string name)
			where T : class
		{
			Type type = typeof(T);
			ClassWrapper wrapper = ClassWrapper.CreateFromType (typeRegistry, type, name);
			typeRegistry.AddTypeMapping (type, wrapper, null);
			context.VirtualMachine.Globals [name] = wrapper;
		}

		public dynamic DoString (string source)
		{
			SourceUnit line = SourceUnit.CreateFromSource (source);
			context.Invoke (line.Compile (context), new IodineObject[] { });
			return null;
		}

		public dynamic DoFile (string file)
		{
			IodineModule main = new IodineModule (Path.GetFileNameWithoutExtension (file));
			DoString (main, File.ReadAllText (file));
			return new IodineDynamicObject (main, context.VirtualMachine, typeRegistry);
		}

		private dynamic DoString (IodineModule module, string source)
		{
			return null;
		}

		private dynamic GetMember (string name)
		{
			IodineObject obj = null;
			if (context.VirtualMachine.Globals.ContainsKey (name)) {
				obj = context.VirtualMachine.Globals [name];
			} else if (this.defaultModule.HasAttribute (name)) {
				obj = defaultModule.GetAttribute (name);
			}
			return IodineDynamicObject.Create (obj, context.VirtualMachine, typeRegistry);
		}

		private void SetMember (string name, dynamic value)
		{
			IodineObject obj = typeRegistry.ConvertToIodineObject ((object)value);
			if (defaultModule.HasAttribute (name)) {
				defaultModule.SetAttribute (name, obj);
			} else {
				context.VirtualMachine.Globals [name] = obj;
			}
		}
	}
}

