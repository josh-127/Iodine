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
using System.Reflection;
using System.Collections.Generic;
using Iodine.Runtime;

namespace Iodine.Compiler
{
    /// <summary>
    /// Cached Iodine bytecode file
    /// </summary>
    internal class BytecodeFile
    {
        enum DataType
        {
            CodeObject = 0x00,
            NameObject = 0x01,
            StringObject = 0x02,
            IntObject = 0x03,
            FloatObject = 0x04,
            BoolObject = 0x05,
            NullObject = 0x06,
            BigIntObject = 0x07
        }

        const byte MAGIC_0 = 0x49;
        const byte MAGIC_1 = 0x4F;
        const byte MAGIC_2 = 0x57;
        const byte MAGIC_3 = 0x49;
        const byte MAGIC_4 = 0x5A;

        private BinaryWriter binaryWriter;
        private BinaryReader binaryReader;
        private string fileName;

        public BytecodeFile (FileStream stream, string originalFile)
        {
            fileName = originalFile;
            binaryWriter = new BinaryWriter (stream);
            binaryReader = new BinaryReader (stream);
        }
            
        public bool TryReadModule (ref IodineModule module)
        {
            if (!ReadHeader ()) {
                return false;
            }

            string name = binaryReader.ReadString ();

            ModuleBuilder builder = new ModuleBuilder (name, fileName);

            binaryReader.ReadByte ();

            ReadCodeObject (builder.Initializer);

            int constantCount = binaryReader.ReadInt32 ();

            for (int i = 0; i < constantCount; i++) {
                builder.DefineConstant (ReadConstant ());
            }

            module = builder;
            return true;
        }

        private bool ReadHeader ()
        {
            byte mag0 = binaryReader.ReadByte ();
            byte mag1 = binaryReader.ReadByte ();
            byte mag2 = binaryReader.ReadByte ();
            byte mag3 = binaryReader.ReadByte ();
            byte mag4 = binaryReader.ReadByte ();

            if (mag0 != MAGIC_0 ||
                mag1 != MAGIC_1 ||
                mag2 != MAGIC_2 ||
                mag3 != MAGIC_3 ||
                mag4 != MAGIC_4) {
                return false;
            }

            int versionMajor = binaryReader.ReadByte ();
            int versionMinor = binaryReader.ReadByte ();
            int versionBuild = binaryReader.ReadByte ();

            Version version = Assembly.GetExecutingAssembly ().GetName ().Version;

            if (versionMajor != version.Major ||
                versionMinor != version.Minor ||
                versionBuild != version.Build) {
                return false;
            }

            long timestamp = binaryReader.ReadInt64 ();

            DateTime lastModified = File.GetLastWriteTime (fileName);

            if (timestamp < GetUnixTime (lastModified)) {
                return false;
            }
            return true;
        }

        public void WriteModule (ModuleBuilder builder)
        {
            binaryWriter.Write (MAGIC_0);
            binaryWriter.Write (MAGIC_1);
            binaryWriter.Write (MAGIC_2);
            binaryWriter.Write (MAGIC_3);
            binaryWriter.Write (MAGIC_4);

            Version version = Assembly.GetExecutingAssembly ().GetName ().Version;

            binaryWriter.Write ((byte)version.Major);
            binaryWriter.Write ((byte)version.Minor);
            binaryWriter.Write ((byte)version.Build);

            binaryWriter.Write (GetUnixTime (DateTime.Now));

            binaryWriter.Write (builder.Name);

            WriteCodeObject (builder.Initializer);

            binaryWriter.Write (builder.ConstantPool.Count);

            foreach (IodineObject obj in builder.ConstantPool) {
                WriteConstant (obj);
            }
        }

        private void WriteConstant (IodineObject obj)
        {
            var lookup = new Dictionary<Type, Action> ()
            {
                { typeof (IodineNull), () =>    WriteNull () },  
                { typeof (IodineBool), () =>    WriteBool (obj as IodineBool) },
                { typeof (IodineName), () =>    WriteName (obj as IodineName) }, 
                { typeof (IodineInteger), () => WriteInt (obj as IodineInteger) }, 
                { typeof (IodineFloat), () =>   WriteFloat (obj as IodineFloat) }, 
                { typeof (IodineBigInt), () =>  WriteBigInt (obj as IodineBigInt) },
                { typeof (IodineString), () =>  WriteString (obj as IodineString) }, 
                { typeof (CodeBuilder), () =>   WriteCodeObject (obj as CodeBuilder) }, 
            };

            lookup [obj.GetType ()] ();
        }

        private void WriteCodeObject (CodeBuilder codeObject)
        {
            binaryWriter.Write ((byte)DataType.CodeObject);
            binaryWriter.Write (codeObject.Instructions.Length);

            for (int i = 0; i < codeObject.Instructions.Length; i++) {
                WriteInstruction (codeObject.Instructions [i]);
            }
        }

        private void WriteInstruction (Instruction ins)
        {
            binaryWriter.Write ((byte)ins.OperationCode);

            binaryWriter.Write (ins.Argument);

            if (ins.Location == null) {
                binaryWriter.Write (-1);
            } else {
                binaryWriter.Write (ins.Location.Line);
            }
        }

        private void WriteName (IodineName name)
        {
            binaryWriter.Write ((byte)DataType.NameObject);
            binaryWriter.Write (name.Value);
        }

        private void WriteString (IodineString str)
        {
            binaryWriter.Write ((byte)DataType.StringObject);
            binaryWriter.Write (str.Value);
        }

        private void WriteInt (IodineInteger integer)
        {
            binaryWriter.Write ((byte)DataType.IntObject);
            binaryWriter.Write (integer.Value);
        }

        private void WriteFloat (IodineFloat realnum)
        {
            binaryWriter.Write ((byte)DataType.FloatObject);
            binaryWriter.Write (realnum.Value);
        }

        private void WriteBool (IodineBool boolean)
        {
            binaryWriter.Write ((byte)DataType.BoolObject);
            binaryWriter.Write (boolean.Value);
        }

        private void WriteBigInt (IodineBigInt bigint)
        {
            binaryWriter.Write ((byte)DataType.BigIntObject);
            byte[] bytes = bigint.Value.ToByteArray ();
            binaryWriter.Write (bytes.Length);
            binaryWriter.Write (bytes);
        }

        private void WriteNull ()
        {
            binaryWriter.Write ((byte)DataType.NullObject);
        }

        private IodineObject ReadConstant ()
        {
            DataType type = (DataType)binaryReader.ReadByte ();

            switch (type) {
            case DataType.NameObject:
                return ReadName ();
            case DataType.StringObject:
                return ReadString ();
            case DataType.IntObject:
                return ReadInt ();
            case DataType.FloatObject:
                return ReadFloat ();
            case DataType.CodeObject:
                CodeBuilder codeObj = new CodeBuilder ();
                return ReadCodeObject (codeObj);
            case DataType.BoolObject:
                return ReadBool ();
            case DataType.NullObject:
                return IodineNull.Instance;
            case DataType.BigIntObject:
                return ReadBigInt ();
            }
            return null;
        }

        private CodeBuilder ReadCodeObject (CodeBuilder codeObject)
        {
            int instructionCount = binaryReader.ReadInt32 ();

            for (int i = 0; i < instructionCount; i++) {
                ReadInstruction (codeObject);
            }

            codeObject.Finalize ();

            return codeObject;
        }

        private void ReadInstruction (CodeBuilder codeObject)
        {
            Opcode opcode = (Opcode)binaryReader.ReadByte ();
            int argument = binaryReader.ReadInt32 ();
            int line = binaryReader.ReadInt32 ();

            SourceLocation location = line == -1 
                ? null
                : new SourceLocation (line, 0, fileName);

            codeObject.EmitInstruction (
                location,
                opcode, 
                argument
            ); 
        }

        public IodineObject ReadName ()
        {
            return new IodineName (binaryReader.ReadString ());
        }

        public IodineObject ReadString ()
        {
            return new IodineString (binaryReader.ReadString ());
        }

        public IodineObject ReadInt ()
        {
            return new IodineInteger (binaryReader.ReadInt64 ());
        }

        public IodineObject ReadFloat ()
        {
            return new IodineFloat (binaryReader.ReadDouble ());
        }

        public IodineObject ReadBool ()
        {
            return IodineBool.Create (binaryReader.ReadBoolean ());
        }

        public IodineObject ReadBigInt ()
        {
            int size = binaryReader.ReadInt32 ();
            byte[] bytes = binaryReader.ReadBytes (size);

            return new IodineBigInt (new System.Numerics.BigInteger (bytes));
        }

        static long GetUnixTime (DateTime time)
        {
            return (long)(time.Subtract (new DateTime (1970, 1, 1))).TotalSeconds;
        }
    }
}

