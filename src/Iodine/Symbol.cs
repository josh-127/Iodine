using System;

namespace Iodine.Compiler
{
	public enum SymbolType
	{
		Local,
		Global
	}

	public class Symbol
	{
		public string Name {
			private set;
			get;
		}

		public int Index {
			private set;
			get;
		}

		public SymbolType Type {
			private set;
			get;
		}

		public Symbol (SymbolType type, string name, int index)
		{
			this.Name = name;
			this.Index = index;
			this.Type = type;
		}
	}
}

