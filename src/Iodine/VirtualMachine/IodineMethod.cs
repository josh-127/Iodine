using System;
using System.IO;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineLabel
	{
		public int _Position;
		public int _LabelID ;

		public IodineLabel (int labelID)
		{
			this._LabelID = labelID;
			this._Position = 0;
		}
	}

	public class IodineInstanceMethodWrapper : IodineObject
	{
		private static readonly IodineTypeDefinition InstanceTypeDef = new IodineTypeDefinition ("InstanceMethod");

		public IodineMethod Method {
			private set;
			get;
		}

		private IodineObject self;

		public IodineInstanceMethodWrapper (IodineObject self, IodineMethod method)
			: base (InstanceTypeDef)
		{
			this.Method = method;
			this.self = self;
		}

		public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			return vm.InvokeMethod (Method, self, arguments);
		}
	}

	public class IodineMethod : IodineObject
	{
		private static readonly IodineTypeDefinition MethodTypeDef = new IodineTypeDefinition ("Method");
		private static int nextLabelID = 0;

		private Dictionary<int, IodineLabel> labelReferences = new Dictionary<int, IodineLabel> ();
		protected List<Instruction> instructions = new List<Instruction> ();
		private IodineMethod parent = null;

		public List<Instruction> Body {
			get {
				return this.instructions;
			}
		}

		public string Name {
			private set;
			get;
		}

		public Dictionary <string, int> Parameters {
			private set;
			get;
		}

		public int ParameterCount {
			private set;
			get;
		}

		public int LocalCount {
			private set;
			get;
		}

		public bool Variadic {
			set;
			get;
		}

		public IodineModule Module {
			private set;
			get;
		}

		public bool InstanceMethod {
			private set;
			get;
		}

		public IodineMethod (IodineModule module, string name, bool isInstance, int parameterCount,
			int localCount) : base (MethodTypeDef)
		{
			this.Name = name;
			this.ParameterCount = parameterCount;
			this.Module = module;
			this.LocalCount = localCount;
			this.InstanceMethod = isInstance;
			this.Parameters = new Dictionary<string, int> ();
		}

		public IodineMethod (IodineMethod parent, IodineModule module, string name, bool isInstance, int parameterCount,
			int localCount) : this (module, name, isInstance, parameterCount, localCount)
		{
			this.parent = parent;
		}

		public void EmitInstruction (Opcode opcode)
		{
			this.instructions.Add (new Instruction (new Location (0, 0, ""), opcode));
		}

		public void EmitInstruction (Opcode opcode, int arg)
		{
			this.instructions.Add (new Instruction (new Location (0, 0, ""), opcode, arg));
		}

		public void EmitInstruction (Opcode opcode, IodineLabel label)
		{
			this.labelReferences[this.instructions.Count] = label;
			this.instructions.Add (new Instruction (new Location (0, 0, ""), opcode, 0));
		}

		public void EmitInstruction (Location loc, Opcode opcode)
		{
			this.instructions.Add (new Instruction (loc, opcode));
		}

		public void EmitInstruction (Location loc, Opcode opcode, int arg)
		{
			this.instructions.Add (new Instruction (loc, opcode, arg));
		}

		public void EmitInstruction (Location loc, Opcode opcode, IodineLabel label)
		{
			this.labelReferences[this.instructions.Count] = label;
			this.instructions.Add (new Instruction (loc, opcode, 0));
		}

		public int CreateTemporary ()
		{
			if (this.parent != null) 
				this.parent.CreateTemporary ();
			return this.LocalCount++;
		}

		public IodineLabel CreateLabel ()
		{
			return new IodineLabel (nextLabelID++);
		}

		public void MarkLabelPosition (IodineLabel label)
		{
			label._Position = this.instructions.Count;
		}

		public void FinalizeLabels ()
		{
			foreach (int position in this.labelReferences.Keys) {
				this.instructions[position] = new Instruction (this.instructions[position].Location,
					this.instructions[position].OperationCode, this.labelReferences[position]._Position);
			}
		}

		public override IodineObject Invoke (VirtualMachine vm, IodineObject[] arguments)
		{
			return vm.InvokeMethod (this, null, arguments);
		}
	}
}

