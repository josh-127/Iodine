// /**
//   * Copyright (c) 2015, GruntTheDivine All rights reserved.
//
//   * Redistribution and use in source and binary forms, with or without modification,
//   * are permitted provided that the following conditions are met:
//   * 
//   *  * Redistributions of source code must retain the above copyright notice, this list
//   *    of conditions and the following disclaimer.
//   * 
//   *  * Redistributions in binary form must reproduce the above copyright notice, this
//   *    list of conditions and the following disclaimer in the documentation and/or
//   *    other materials provided with the distribution.
//
//   * Neither the name of the copyright holder nor the names of its contributors may be
//   * used to endorse or promote products derived from this software without specific
//   * prior written permission.
//   * 
//   * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
//   * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
//   * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
//   * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
//   * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
//   * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
//   * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
//   * CONTRACT ,STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
//   * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
//   * DAMAGE.
// /**
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Iodine.Runtime;

namespace Iodine.Engine
{
	class AssemblyWrapper 
	{
		public static void CreateFromAssembly (TypeRegistry registry, Assembly asm)
		{
			var classes = asm.GetExportedTypes ().Where (p => p.IsClass);
			Dictionary<string, IodineModule> modules = new Dictionary<string, IodineModule> ();
			foreach (Type type in classes) {
				Console.WriteLine (type.FullName);
				if (type.Namespace != "") {
					string moduleName = type.Namespace.Contains (".") ? 
						type.Namespace.Substring (type.Namespace.LastIndexOf (".") + 1) :
						type.Namespace;
					IodineModule module = null;
					if (!modules.ContainsKey (type.Namespace)) {
						module = new IodineModule (moduleName);
						modules [type.Namespace] = module;
					} else {
						module = modules [type.Namespace];
					}
					module.SetAttribute (type.Name, ClassWrapper.CreateFromType (registry, type,
						type.Name));
				}
			}
		}
	}
}