using System;

namespace Iodine
{
	public class IodineEngine
	{
		private IodineModule defaultModule;

		public VirtualMachine VirtualMachine {
			private set;
			get;
		}

		public IodineEngine ()
		{
			this.VirtualMachine = new VirtualMachine ();
			this.defaultModule = new IodineModule ("__main__");
		}

		public IodineObject this [string name] {
			get {
				if (this.VirtualMachine.Globals.ContainsKey (name)) {
					return this.VirtualMachine.Globals [name];
				} else if (this.defaultModule.HasAttribute (name)) {
					return this.defaultModule.GetAttribute (name);
				}
				return null;
			}
		}

		public IodineObject DoString (string source)
		{
			ErrorLog errorLog = new ErrorLog ();
			Lexer lex = new Lexer (errorLog, source);
			if (errorLog.ErrorCount > 0) throw new SyntaxException (errorLog);
			Parser parser = new Parser (lex.Scan ());
			if (errorLog.ErrorCount > 0) throw new SyntaxException (errorLog);
			Ast root = parser.Parse ();
			if (errorLog.ErrorCount > 0) throw new SyntaxException (errorLog);
			SemanticAnalyser analyser = new SemanticAnalyser (errorLog);
			SymbolTable symTab = analyser.Analyse (root);
			if (errorLog.ErrorCount > 0) throw new SyntaxException (errorLog);
			IodineCompiler compiler = new IodineCompiler (errorLog, symTab, "");
			compiler.CompileAst (this.defaultModule, root);
			if (errorLog.ErrorCount > 0) throw new SyntaxException (errorLog);

			return this.VirtualMachine.InvokeMethod (this.defaultModule.Initializer, null, new IodineObject[] 
				{});

		}

		public IodineObject DoFile (string file)
		{
			return DoString (System.IO.File.ReadAllText (file));
		}
	}

	public class SyntaxException : Exception
	{
		public ErrorLog ErrorLog {
			private set;
			get;
		}

		public SyntaxException (ErrorLog errLog)
			: base ("Syntax Error")
		{
			this.ErrorLog = errLog;
		}
	}
}

