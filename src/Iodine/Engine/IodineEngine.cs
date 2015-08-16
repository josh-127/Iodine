using System;
using System.IO;

namespace Iodine
{
	public class IodineEngine
	{
		private IodineModule defaultModule;
		private StackFrame stackFrame;

		public VirtualMachine VirtualMachine {
			private set;
			get;
		}

		public IodineEngine ()
		{
			this.VirtualMachine = new VirtualMachine ();
			this.defaultModule = new IodineModule ("__main__");
			this.stackFrame = new StackFrame (this.defaultModule.Initializer, null, null, 1024);
		}

		public dynamic this [string name] {
			get {
				IodineObject obj = null;
				if (this.VirtualMachine.Globals.ContainsKey (name)) {
					obj = this.VirtualMachine.Globals [name];
				} else if (this.defaultModule.HasAttribute (name)) {
					obj = this.defaultModule.GetAttribute (name);
				}
				Object ret = null;
				if (!IodineTypeConverter.Instance.ConvertToPrimative (obj, out ret)) {
					ret = IodineTypeConverter.Instance.CreateDynamicObject (this, obj);
				}
				return ret;
			}
			set {
				IodineObject obj;
				IodineTypeConverter.Instance.ConvertFromPrimative (value, out obj);
				if (this.defaultModule.HasAttribute (name)) {
					this.defaultModule.SetAttribute (name, obj);
				} else {
					this.VirtualMachine.Globals [name] = obj;
				}
			}
		}

		public dynamic DoString (string source)
		{
			return doString (defaultModule, source);
		}

		public dynamic DoFile (string file)
		{
			IodineModule main = new IodineModule (Path.GetFileNameWithoutExtension (file));
			doString (main, File.ReadAllText (file));
			return new IodineDynamicObject (main, VirtualMachine);
		}

		private dynamic doString (IodineModule module, string source)
		{
			ErrorLog errorLog = new ErrorLog ();
			Lexer lex = new Lexer (errorLog, source);
			if (errorLog.ErrorCount > 0)
				throw new SyntaxException (errorLog);
			Parser parser = new Parser (lex.Scan ());
			if (errorLog.ErrorCount > 0)
				throw new SyntaxException (errorLog);
			Ast root = parser.Parse ();
			if (errorLog.ErrorCount > 0)
				throw new SyntaxException (errorLog);
			SemanticAnalyser analyser = new SemanticAnalyser (errorLog);
			SymbolTable symTab = analyser.Analyse (root);
			if (errorLog.ErrorCount > 0)
				throw new SyntaxException (errorLog);
			IodineCompiler compiler = new IodineCompiler (errorLog, symTab, "");
			module.Initializer.Body.Clear ();
			compiler.CompileAst (module, root);
			if (errorLog.ErrorCount > 0)
				throw new SyntaxException (errorLog);

			IodineObject result = this.VirtualMachine.InvokeMethod (module.Initializer,
				                      null, new IodineObject[] { });
			object ret = null;
			if (!IodineTypeConverter.Instance.ConvertToPrimative (result, out ret)) {
				ret = IodineTypeConverter.Instance.CreateDynamicObject (this, result);
			}
			return ret;
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

