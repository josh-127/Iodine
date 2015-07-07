using System;
using System.IO;

namespace Iodine
{
	public interface ICachable
	{
		void EncodeInto (BinaryWriter bw);
		void DecodeInto (BinaryReader br);
	}
}

