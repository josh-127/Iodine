using System;
using System.Collections.Generic;

namespace Iodine.Compiler
{
	public class ControlFlowOptimization : IBytecodeOptimization
	{
		class ReachableRegion
		{
			public int Start {
				private set;
				get;
			}

			public int End {
				private set;
				get;
			}

			public int Size {
				get {
					return this.End - this.Start;
				}
			}

			public ReachableRegion (int start, int end)
			{
				this.Start = start;
				this.End = end;
			}
		}

		public void PerformOptimization (IodineMethod method)
		{
			List <ReachableRegion> regions = new List<ReachableRegion> ();
			int reachableSize = 0;
			findRegion (method, regions, 0);
			foreach (ReachableRegion region in regions) {
				reachableSize += region.Size + 1;
			}
			Instruction[] oldInstructions = method.Body.ToArray ();
			Instruction[] newInstructions = new Instruction[method.Body.Count];
			int next = 0;
			for (int i = 0; i < method.Body.Count; i++) {
				if (isReachable (regions, i)) {
					newInstructions [next++] = oldInstructions [i];
				} else {
					shiftLabels (next, oldInstructions);
					shiftLabels (next, newInstructions);
				}
			}
			method.Body.Clear ();
			method.Body.AddRange (newInstructions);
		}

		private void findRegion (IodineMethod method, List<ReachableRegion> regions, int start)
		{
			if (isReachable (regions, start)) {
				return;
			}

			for (int i = start; i < method.Body.Count; i++) {
				Instruction ins = method.Body [i];

				if (ins.OperationCode == Opcode.Jump) {
					regions.Add (new ReachableRegion (start, i));
					findRegion (method, regions, ins.Argument);
					return;
				} else if (ins.OperationCode == Opcode.JumpIfTrue ||
				           ins.OperationCode == Opcode.JumpIfFalse ||
				           ins.OperationCode == Opcode.PushExceptionHandler) {
					regions.Add (new ReachableRegion (start, i));
					findRegion (method, regions, i + 1);
					findRegion (method, regions, ins.Argument);
					return;
				} else if (ins.OperationCode == Opcode.Return) {
					regions.Add (new ReachableRegion (start, i));
					return;
				}
			}
			regions.Add (new ReachableRegion (start, method.Body.Count));
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

		private bool isReachable (List<ReachableRegion> regions, int addr)
		{
			foreach (ReachableRegion region in regions) {
				if (region.Start <= addr && addr <= region.End) {
					return true;
				}
			}
			return false;
		}
	}
}

