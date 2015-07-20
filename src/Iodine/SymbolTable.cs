using System;
using System.Collections.Generic;

namespace Iodine
{
	public class SymbolTable
	{
		class LocalScope
		{
			public int NextLocal {
				set;
				get;
			}

			public LocalScope ParentScope {
				private set;
				get;
			}

			public LocalScope (LocalScope parentScope) {
				this.ParentScope = parentScope;
				this.NextLocal = 0;
			}

		}

		private int nextGlobalIndex = 0;
		private Scope globalScope = new Scope ();
		private Scope lastScope = null;
		private LocalScope currentLocalScope = null;

		public Scope CurrentScope {
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

		public Scope LeaveScope ()
		{
			Scope old = CurrentScope;
			CurrentScope = old.ParentScope;
			CurrentScope.NextScope = old.NextScope;
			return CurrentScope;
		}

		public void BeginScope (bool isLocalScope = false)
		{
			if (isLocalScope) {
				this.currentLocalScope = new LocalScope (this.currentLocalScope);
			}
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

		public void EndScope (bool isLocalScope = false)
		{
			if (isLocalScope) {
				this.currentLocalScope = this.currentLocalScope.ParentScope;
			}

			CurrentScope = CurrentScope.ParentScope;
		}

		public int AddSymbol (string name)
		{
			if (this.CurrentScope.ParentScope != null) {
				return CurrentScope.AddSymbol (SymbolType.Local, name, currentLocalScope.NextLocal++);
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

