using System;

namespace Iodine.Compiler
{
	public class InstructionOptimization : IBytecodeOptimization
	{
		public void PerformOptimization (IodineMethod method)
		{
			while (performOptimiziation (method) > 0)
				;
		}

		private int performOptimiziation (IodineMethod method)
		{
			int removed = 0;
			Instruction[] oldInstructions = method.Body.ToArray ();
			Instruction[] newInstructions = new Instruction[method.Body.Count];
			int next = 0;
			Instruction last = new Instruction ();
			for (int i = 0; i < method.Body.Count; i++) {
				Instruction curr = oldInstructions [i];
				if (i != 0 && curr.OperationCode == Opcode.Pop) {
					if (last.OperationCode == Opcode.LoadLocal || last.OperationCode == Opcode.LoadGlobal
					    || last.OperationCode == Opcode.LoadNull) {
						oldInstructions [i] = new Instruction (curr.Location, Opcode.Nop, 0);
						oldInstructions [i - 1] = new Instruction (curr.Location, Opcode.Nop, 0);
						removed++;
					}
				} else if (curr.OperationCode == Opcode.Jump && curr.Argument == i + 1) {
					oldInstructions [i] = new Instruction (curr.Location, Opcode.Nop, 0);
					removed++;
				}
				last = curr;
			}
			for (int i = 0; i < oldInstructions.Length; i++) {
				Instruction curr = oldInstructions [i];
				if (curr.OperationCode == Opcode.Nop) {
					shiftLabels (next, newInstructions);
					shiftLabels (next, oldInstructions);
				} else {
					newInstructions [next++] = curr;
				}
			}
			method.Body.Clear ();
			method.Body.AddRange (newInstructions);
			return removed;
		}

		private void shiftLabels (int start, Instruction[] instructions)
		{
			for (int i = 0; i < instructions.Length; i++) {
				Instruction ins = instructions [i];
				if (ins.OperationCode == Opcode.Jump || ins.OperationCode == Opcode.JumpIfFalse ||
				    ins.OperationCode == Opcode.JumpIfTrue ||
				    ins.OperationCode == Opcode.PushExceptionHandler) {

					if (ins.Argument > start) {
						instructions [i] = new Instruction (ins.Location, ins.OperationCode,
							ins.Argument - 1);
					}
				}

			}
		}
	}
}

