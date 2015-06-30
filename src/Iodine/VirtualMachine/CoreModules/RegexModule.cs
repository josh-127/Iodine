using System;
using System.Text.RegularExpressions;

namespace Iodine
{
	public class RegexModule : IodineModule
	{
		class IodineRegex : IodineObject
		{
			public static readonly IodineTypeDefinition RegexTypeDef = new IodineTypeDefinition ("Regex");

			public Regex Value
			{
				private set;
				get;
			}

			public IodineRegex (Regex val)
				: base (RegexTypeDef)
			{
				this.Value = val;
				this.SetAttribute ("match", new InternalMethodCallback (match, this));
				this.SetAttribute ("isMatch", new InternalMethodCallback (isMatch, this));
			}

			private IodineObject match (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}
				IodineString expr = args[0] as IodineString;

				if (expr == null) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				return new IodineMatch (this.Value.Match (expr.ToString ()));
			}

			private IodineObject isMatch (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				if (args.Length <= 0) {
					vm.RaiseException (new IodineArgumentException (1));
					return null;
				}
				IodineString expr = args[0] as IodineString;

				if (expr == null) {
					vm.RaiseException (new IodineTypeException ("Str"));
					return null;
				}

				return new IodineBool (this.Value.IsMatch (expr.ToString ()));
			}
		}

		class IodineMatch : IodineObject
		{
			public static readonly IodineTypeDefinition MatchTypeDef = new IodineTypeDefinition ("Match");

			public Match Value
			{
				private set;
				get;
			}

			private Match iterMatch;
			private Match iterRet;

			public IodineMatch (Match val)
				: base (MatchTypeDef) {
				this.Value = val;
				this.SetAttribute ("value", new IodineString (val.Value));
				this.SetAttribute ("success", new IodineBool (val.Success));
				this.SetAttribute ("getNextMatch", new InternalMethodCallback (getNextMatch, this));
			}

			public override IodineObject IterGetNext (VirtualMachine vm)
			{
				return new IodineMatch (this.iterRet);
			}

			public override bool IterMoveNext (VirtualMachine vm)
			{
				this.iterRet = this.iterMatch;
				this.iterMatch = this.iterMatch.NextMatch ();
				if (this.iterRet.Success) {
					return true;
				}
				return false;
			}

			public override void IterReset (VirtualMachine vm)
			{
				this.iterMatch = this.Value;
			}

			private IodineObject getNextMatch (VirtualMachine vm, IodineObject self, IodineObject[] EventArgs)
			{
				return new IodineMatch (this.Value.NextMatch ());
			}
		}

		public RegexModule ()
			: base ("regex")
		{
			this.SetAttribute ("compile", new InternalMethodCallback (compile, this));
		}

		private IodineObject compile (VirtualMachine vm, IodineObject self, IodineObject[] args) 
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineString expr = args[0] as IodineString;

			if (expr == null) {
				vm.RaiseException (new IodineTypeException ("Str"));
				return null;
			}

			return new IodineRegex (new Regex (expr.ToString ()));
		}


	}
}

