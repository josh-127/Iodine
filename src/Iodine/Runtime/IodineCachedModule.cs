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
using System.Collections.Generic;

namespace Iodine.Runtime
{
	public static class IodineCachedModule
	{
		const byte BytecodeMajorVersion = 1;
		const byte BytecodeMinorVersion = 3;
		const byte Mag1 = 0x43;
		const byte Mag2 = 0x41;
		const byte Mag3 = 0x42;
		const byte Mag4 = 0x3C;
		const byte Mag5 = 0x33;

		enum IodineItemType
		{
			Method = 0,
			List = 1,
			Tuple = 2,
			Class = 3,
			Enum = 4,
			String = 5,
			Int = 6,
			Float = 7,
			Bool = 8,
			Name = 9,
			Null = 10,
			Inteface = 11,
		}

		public static void SaveModule (string path, IodineModule original)
		{
			using (BinaryWriter bw = new BinaryWriter (new FileStream (path, FileMode.Create))) {
				bw.Write (Mag1);
				bw.Write (Mag2);
				bw.Write (Mag3);
				bw.Write (Mag4);
				bw.Write (Mag5);
				bw.Write (BytecodeMajorVersion);
				bw.Write (BytecodeMinorVersion);
				bw.Write (original.Name);
				bw.Write (original.ConstantPool.Count);
				foreach (IodineObject constant in original.ConstantPool) {
					WriteObject (bw, constant);
				}
				WriteObject (bw, original.Initializer);
				bw.Write (original.Attributes.Count);
				foreach (string key in original.Attributes.Keys) {
					bw.Write (key);
					WriteObject (bw, original.Attributes [key]);
				}
			}
		}

		public static void WriteObject (BinaryWriter bw, IodineObject obj)
		{
			if (obj is IodineString) {
				bw.Write ((byte)IodineItemType.String);
				bw.Write (obj != null ? obj.ToString () : "");
			} else if (obj is IodineName) {
				bw.Write ((byte)IodineItemType.Name);
				bw.Write (obj != null ? obj.ToString () : "");
			} else if (obj is IodineInteger) {
				bw.Write ((byte)IodineItemType.Int);
				bw.Write (((IodineInteger)obj).Value);
			} else if (obj is IodineFloat) {
				bw.Write ((byte)IodineItemType.Float);
				bw.Write (((IodineFloat)obj).Value);
			} else if (obj is IodineBool) {
				bw.Write ((byte)IodineItemType.Float);
				bw.Write (((IodineBool)obj).Value);
			} else if (obj is IodineMethod) {
				bw.Write ((byte)IodineItemType.Method);
				WriteMethod (bw, obj as IodineMethod);
			} else if (obj is IodineEnum) {
				bw.Write ((byte)IodineItemType.Enum);
				WriteEnum (bw, obj as IodineEnum);
			} else if (obj is IodineClass) {
				bw.Write ((byte)IodineItemType.Class);
				WriteClass (bw, obj as IodineClass);
			} else if (obj is IodineInterface) {
				bw.Write ((byte)IodineItemType.Inteface);
				WriteInterface (bw, obj as IodineInterface);
			} else if (obj is IodineTuple) {
				bw.Write ((byte)IodineItemType.Tuple);
				WriteTuple (bw, obj as IodineTuple);
			} else if (obj is IodineList) {
				bw.Write ((byte)IodineItemType.List);
				WriteList (bw, obj as IodineList);
			} else if (obj is IodineTypeDefinition) {
				bw.Write ((byte)IodineItemType.Null);
			} else {
				throw new Exception (obj.ToString ());
			}
		}

		private static void WriteEnum (BinaryWriter bw, IodineEnum ienum)
		{
			bw.Write (ienum.Name);
			bw.Write (ienum.Attributes.Count);
			foreach (string key in ienum.Attributes.Keys) {
				bw.Write (key);
				WriteObject (bw, ienum.GetAttribute (key));
			}
		}

		private static void WriteClass (BinaryWriter bw, IodineClass clazz)
		{
			bw.Write (clazz.Name);
			WriteObject (bw, clazz.Constructor);
			bw.Write (clazz.InstanceMethods.Count);
			foreach (IodineMethod meth in clazz.InstanceMethods) {
				WriteObject (bw, meth);
			}
			bw.Write (clazz.Attributes.Count);
			foreach (string key in clazz.Attributes.Keys) {
				bw.Write (key);
				WriteObject (bw, clazz.Attributes [key]);
			}
		}

		private static void WriteInterface (BinaryWriter bw, IodineInterface contract)
		{
			bw.Write (contract.Name);
			bw.Write (contract.RequiredMethods.Count);
			foreach (IodineMethod meth in contract.RequiredMethods) {
				WriteObject (bw, meth);
			}
		}

		private static void WriteMethod (BinaryWriter bw, IodineMethod method)
		{
			if (method.Name != null) {
				bw.Write (method.Name);
			} else {
				bw.Write ("");
			}
			bw.Write (method.Variadic);
			bw.Write (method.InstanceMethod);
			bw.Write (method.Parameters.Count);
			foreach (string key in method.Parameters.Keys) {
				bw.Write (key);
				bw.Write (method.Parameters [key]);
			}
			bw.Write (method.LocalCount);
			bw.Write (method.Body.Count);
			foreach (Instruction ins in method.Body) {
				bw.Write ((byte)ins.OperationCode);
				bw.Write (ins.Argument);
			}
		}

		private static void WriteTuple (BinaryWriter bw, IodineTuple tuple)
		{
			bw.Write (tuple.Objects.Length);
			foreach (IodineObject obj in tuple.Objects) {
				WriteObject (bw, obj);
			}
		}

		private static void WriteList (BinaryWriter bw, IodineList list)
		{
			bw.Write (list.Objects.Count);
			foreach (IodineObject obj in list.Objects) {
				WriteObject (bw, obj);
			}
		}

		public static IodineModule Load (string path)
		{
			using (BinaryReader br = new BinaryReader (new FileStream (path, FileMode.Open))) {
				byte mag1 = br.ReadByte ();
				byte mag2 = br.ReadByte ();
				byte mag3 = br.ReadByte ();
				byte mag4 = br.ReadByte ();
				byte mag5 = br.ReadByte ();
				byte major = br.ReadByte ();
				byte minor = br.ReadByte ();

				if (mag1 == Mag1 && mag2 == Mag2 && mag3 == Mag3 && mag4 == Mag4 && mag5 == Mag5) {
					if (major != BytecodeMajorVersion || minor != BytecodeMinorVersion) {
						return null;
					}
					string name = br.ReadString ();
					int constantCount = br.ReadInt32 ();
					IodineModule ret = new IodineModule (name);
					for (int i = 0; i < constantCount; i++) {
						ret.DefineConstant (ReadObject (ret, br));
					}
					ret.Initializer = (IodineMethod)ReadObject (ret, br);
					int attrCount = br.ReadInt32 ();
					for (int i = 0; i < attrCount; i++) {
						string attrName = br.ReadString ();
						IodineObject val = ReadObject (ret, br);
						ret.SetAttribute (attrName, val);
					}

					return ret;
				} else {
					return null;
				}
			}
		}

		private static IodineObject ReadObject (IodineModule module, BinaryReader br)
		{
			IodineItemType itemType = (IodineItemType)br.ReadByte ();
			switch (itemType) {
			case IodineItemType.Bool:
				return new IodineBool (br.ReadBoolean ());
			case IodineItemType.Int:
				return new IodineInteger (br.ReadInt64 ());
			case IodineItemType.Float:
				return new IodineFloat (br.ReadDouble ());
			case IodineItemType.String:
				return new IodineString (br.ReadString ());
			case IodineItemType.Name:
				return new IodineName (br.ReadString ());
			case IodineItemType.Class:
				return ReadClass (module, br);
			case IodineItemType.Inteface:
				return ReadInterface (module, br);
			case IodineItemType.Enum:
				return ReadEnum (module, br);
			case IodineItemType.Method:
				return ReadMethod (module, br);
			case IodineItemType.List:
				return ReadList (module, br);
			case IodineItemType.Tuple:
				return ReadTuple (module, br);
			}
			return null;
		}

		private static IodineObject ReadEnum (IodineModule module, BinaryReader br)
		{
			string name = br.ReadString ();
			int items = br.ReadInt32 ();
			IodineEnum ienum = new IodineEnum (name);
			for (int i = 0; i < items; i++) {
				string item = br.ReadString ();
				IodineObject obj = ReadObject (module, br);
				if (obj != null) {
					ienum.SetAttribute (item, obj);
				}
			}
			return ienum;
		}

		private static IodineObject ReadClass (IodineModule module, BinaryReader br)
		{
			string name = br.ReadString ();
			IodineClass clazz = new IodineClass (name, (IodineMethod)ReadObject (module, br));
			int instanceMethods = br.ReadInt32 ();
			for (int i = 0; i < instanceMethods; i++) {
				clazz.AddInstanceMethod (ReadObject (module, br) as IodineMethod);
			}
			int items = br.ReadInt32 ();
			for (int i = 0; i < items; i++) {
				string item = br.ReadString ();
				IodineObject val = ReadObject (module, br);
				clazz.SetAttribute (item, val);
			}
			return clazz;
		}

		private static IodineObject ReadInterface (IodineModule module, BinaryReader br)
		{
			string name = br.ReadString ();
			IodineInterface contract = new IodineInterface (name);
			int methods = br.ReadInt32 ();
			for (int i = 0; i < methods; i++) {
				contract.AddMethod (ReadObject (module, br) as IodineMethod);
			}
			return contract;
		}

		private static IodineObject ReadMethod (IodineModule module, BinaryReader br)
		{
			string name = br.ReadString ();
			bool variadic = br.ReadBoolean ();
			bool instance = br.ReadBoolean ();
			int paramCount = br.ReadInt32 ();
			Dictionary<string, int> parameters = new Dictionary<string, int> ();
			for (int i = 0; i < paramCount; i++) {
				string param = br.ReadString ();
				int local = br.ReadInt32 ();
				parameters [param] = local;
			}
			int localCount = br.ReadInt32 ();
			int insCount = br.ReadInt32 ();
			IodineMethod meth = new IodineMethod (module, name, instance, paramCount, localCount);
			meth.Variadic = variadic;
			foreach (string key in parameters.Keys) {
				meth.Parameters [key] = parameters [key];
			}
			for (int i = 0; i < insCount; i++) {
				meth.EmitInstruction ((Opcode)br.ReadByte (), br.ReadInt32 ());
			}
			return meth;
		}

		private static IodineObject ReadTuple (IodineModule module, BinaryReader br)
		{
			int itemCount = br.ReadInt32 ();
			IodineObject[] items = new IodineObject[itemCount];
			for (int i = 0; i < itemCount; i++) {
				items [i] = ReadObject (module, br);
			}
			return new IodineTuple (items);
		}

		private static IodineObject ReadList (IodineModule module, BinaryReader br)
		{
			int itemCount = br.ReadInt32 ();
			IodineObject[] items = new IodineObject[itemCount];
			for (int i = 0; i < itemCount; i++) {
				items [i] = ReadObject (module, br);
			}
			return new IodineList (items);
		}
	}
}

