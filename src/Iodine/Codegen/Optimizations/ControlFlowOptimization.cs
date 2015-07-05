using System;
using System.Collections.Generic;

namespace Iodine
{
	public class ControlFlowOptimization
	{
		class ReachableRegion
		{
			public int Start
			{
				private set;
				get;
			}

			public int End
			{
				private set;
				get;
			}

			public int Size
			{
				get
				{
					return this.End - this.Start;
				}
			}

			public ReachableRegion (int start, int end)
			{
				this.Start = start;
				this.End = end;
			}
		}

		private List <ReachableRegion> regions = new List<ReachableRegion> ();
		private IodineMethod method;


		public ControlFlowOptimization (IodineMethod method)
		{
			this.method = method;
		}

		public void Optimize ()
		{
			int reachableSize = 0;
			FindRegion (0);
			foreach (ReachableRegion region in this.regions) {
				reachableSize += region.Size +1;
			}
			Instruction[] oldInstructions = method.Body.ToArray ();
			Instruction[] newInstructions = new Instruction[method.Body.Count];
			int next = 0;
			for (int i = 0; i < method.Body.Count; i++) {
				if (isReachable (i)) {
					newInstructions[next++] = oldInstructions[i];
				} else {
					newInstructions[next++] = new Instruction (Opcode.Nop);
					shiftLabels (next, oldInstructions);
					shiftLabels (next, newInstructions);
				}
			}
			this.method.Body.Clear ();
			this.method.Body.AddRange (newInstructions);
		}

		public void FindRegion (int start)
		{
			if (isReachable (start)) {
				return;
			}
			for (int i = start; i < method.Body.Count; i++) {
				Instruction ins = method.Body[i];

				if (ins.OperationCode == Opcode.Jump) {
					this.regions.Add ( new ReachableRegion (start, i));
					FindRegion (ins.Argument);
					return;
				} else if (ins.OperationCode == Opcode.JumpIfTrue || ins.OperationCode == Opcode.JumpIfFalse) {
					this.regions.Add ( new ReachableRegion (start, i));
					FindRegion (i + 1);
					FindRegion (ins.Argument);
					return;
				}
			}
			this.regions.Add (new ReachableRegion (start, method.Body.Count));
		}

		private void shiftLabels (int start, Instruction[] instructions) 
		{
			for (int i = 0; i < instructions.Length; i++) {
				Instruction ins = instructions[i];
				if (ins.OperationCode == Opcode.Jump || ins.OperationCode == Opcode.JumpIfFalse ||
					ins.OperationCode == Opcode.JumpIfTrue || ins.OperationCode == Opcode.PushExceptionHandler) {
					if (ins.Argument >= start) {
						instructions[i] = new Instruction (ins.OperationCode, ins.Argument - 1);
					}
				}

			}
		}

		private bool isReachable (int addr)
		{
			foreach (ReachableRegion region in this.regions) {
				if (region.Start <= addr && addr <= region.End) {
					return true;
				}
			}
			return false;
		}
	}
}

