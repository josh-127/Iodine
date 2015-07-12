using System;
using System.Collections.Generic;

namespace Iodine
{
	public class SymbolTable
	{
		private int nextGlobalIndex = 0;
		private int nextLocalIndex = 0;
		private Scope globalScope = new Scope ();
		private Scope lastScope = null;

		public Scope CurrentScope
		{
			set; get;
		}

		public SymbolTable ()
		{
			CurrentScope = globalScope;
		}

		public Scope NextScope ()
		{
			if (CurrentScope == null) CurrentScope = globalScope;
			CurrentScope = CurrentScope.NextScope;
			return CurrentScope;
		}

		public void BeginScope ()
		{
			Scope newScope = new Scope (CurrentScope);
			if (lastScope != null) {
				lastScope.NextScope = newScope;
			} else {
				globalScope.NextScope = newScope;
			}
			CurrentScope.AddScope (newScope);
			CurrentScope = newScope;
			lastScope = newScope;
		}

		public void EndScope ()
		{
			CurrentScope = CurrentScope.ParentScope;
			if (CurrentScope == globalScope) {
				nextLocalIndex = 0;
			}
		}

		public int AddSymbol (string name)
		{
			if (this.CurrentScope.ParentScope != null) {
				return CurrentScope.AddSymbol (SymbolType.Local, name, nextLocalIndex++);
			} else {
				return CurrentScope.AddSymbol (SymbolType.Global, name, nextGlobalIndex++);
			}
		}

		public bool IsSymbolDefined (string name)
		{
			Scope curr = CurrentScope;
			while (curr != null) {
				Symbol sym;
				if (curr.GetSymbol (name, out sym)) {
					return true;
				}
				curr = curr.ParentScope;
			}
			return false;
		}

		public Symbol GetSymbol (string name)
		{
			Scope curr = CurrentScope;
			while (curr != null) {
				Symbol sym;
				if (curr.GetSymbol (name, out sym)) {
					return sym;
				}
				curr = curr.ParentScope;
			}
			return null;
		}
	}

	public class Scope
	{
		private List<Symbol> symbols = new List<Symbol> ();
		private List<Scope> childScopes = new List<Scope> ();
	
		public Scope ParentScope {
			private set;
			get;
		}

		public Scope NextScope {
			set;
			get;
		}

		public IList<Scope> ChildScopes {
			get {
				return this.childScopes;
			}
		}

		public int SymbolCount {
			get {
				int val = symbols.Count;
				foreach (Scope scope in this.childScopes) {
					val += scope.SymbolCount;
				}
				return val;
			}
		}

		public Scope ()
		{
			this.ParentScope = null;
		}

		public Scope (Scope parent)
		{
			this.ParentScope = parent;
		}

		public int AddSymbol (SymbolType type, string name, int index)
		{
			this.symbols.Add (new Symbol (type, name, index));
			return index;
		}

		public void AddScope (Scope scope)
		{
			this.childScopes.Add (scope);
		}

		public bool GetSymbol (string name, out Symbol symbol)
		{
			foreach (Symbol sym in this.symbols) {
				if (sym.Name == name) {
					symbol = sym;
					return true;
				}
			}
			symbol = null;
			return false;
		}
	}
}

