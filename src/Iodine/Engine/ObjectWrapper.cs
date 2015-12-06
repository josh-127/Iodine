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
using System.Reflection;
using Iodine.Runtime;

namespace Iodine.Engine
{
	class ObjectWrapper : IodineObject
	{
		private Type type;
		private object self;
		private TypeRegistry typeRegistry;

		public ObjectWrapper (TypeRegistry registry, ClassWrapper clazz, object self)
			: base (clazz)
		{
			typeRegistry = registry;
			this.self = self;
		}

		public static ObjectWrapper CreateFromObject (TypeRegistry registry, ClassWrapper clazz, object obj)
		{
			Type type = obj.GetType ();
			ObjectWrapper wrapper = new ObjectWrapper (registry, clazz, obj);
			foreach (MemberInfo info in type.GetMembers (BindingFlags.Instance | BindingFlags.Public)) {
				switch (info.MemberType) {
				case MemberTypes.Method:
					wrapper.SetAttribute (info.Name, MethodWrapper.Create (registry, (MethodInfo)info,
						obj));
					break;
				case MemberTypes.Field:
					wrapper.SetAttribute (info.Name, FieldWrapper.Create (registry, (FieldInfo)info,
						obj));
					break;
				case MemberTypes.Property:
					wrapper.SetAttribute (info.Name, PropertyWrapper.Create (registry, (PropertyInfo)info,
						obj));
					break;
				}
			}
			return wrapper;
		}
	}
}

