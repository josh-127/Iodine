using System;

namespace Iodine
{
	public struct Instruction
	{
		public Location Location {
			set;
			get;
		}

		public Opcode OperationCode {
			private set;
			get;
		}

		public int Argument {
			private set;
			get;
		}

		public Instruction (Location location, Opcode opcode)
		{
			this.OperationCode = opcode;
			this.Argument = 0;
			this.Location = location;
		}

		public Instruction (Location location, Opcode opcode, int arg)
		{
			this.OperationCode = opcode;
			this.Argument = arg;
			this.Location = location;
		}
	}
}

