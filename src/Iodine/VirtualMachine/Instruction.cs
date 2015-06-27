using System;

namespace Iodine
{
	public struct Instruction
	{
		public Opcode OperationCode;
		public int Argument;

		public Instruction (Opcode opcode)
		{
			this.OperationCode = opcode;
			this.Argument = 0;
		}

		public Instruction (Opcode opcode, int arg)
		{
			this.OperationCode = opcode;
			this.Argument = arg;
		}
	}
}

