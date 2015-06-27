using System;
using System.IO;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineModule : IodineObject
	{
		public string Name
		{
			private set;
			get;
		}

		public IList<IodineObject> ConstantPool
		{
			get
			{
				return this.constantPool;
			}
		}

		public IList<string> Imports
		{
			private set;
			get;
		}

		private List<IodineObject> constantPool = new List<IodineObject> ();

		public IodineModule (string name)
		{
			this.Name = name;
			this.Imports = new List<string> ();
		}

		public void AddMethod (IodineMethod method)
		{
			this.attributes[method.Name] = method;
		}

		public int DefineConstant (IodineObject obj)
		{
			constantPool.Add (obj);
			return this.constantPool.Count - 1;
		}

		public static IodineModule CompileModule (ErrorLog errorLog, string file)
		{
			if (FindModule (file) != null) {
				Lexer lexer = new Lexer (errorLog, File.ReadAllText (FindModule (file)));
				TokenStream tokenStream = lexer.Scan ();
				if (errorLog.ErrorCount > 0) return null;
				Parser parser = new Parser (tokenStream);
				Ast root = parser.Parse ();
				if (errorLog.ErrorCount > 0) return null;
				SemanticAnalyser analyser = new SemanticAnalyser (errorLog);
				SymbolTable symbolTable = analyser.Analyse (root);
				if (errorLog.ErrorCount > 0) return null;
				IodineCompiler compiler = new IodineCompiler (errorLog, symbolTable);
				IodineModule module = compiler.CompileAst (root);
				if (errorLog.ErrorCount > 0) return null;
				return module;
			} else {
				errorLog.AddError (ErrorType.ParserError, "Could not find module {0}", file);
				return null;
			}
		}

		private static string FindModule (string name)
		{
			if (File.Exists (name)) {
				return name;
			}
			if (File.Exists (name + ".id")) {
				return name + ".id";
			}
			return null;
		}
	}
}

