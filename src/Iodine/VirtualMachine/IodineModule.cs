using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Iodine
{
	[AttributeUsage(AttributeTargets.Class)]
	public class IodineExtensionAttribute : System.Attribute 
	{
		public string Name
		{
			private set;
			get;
		}

		public IodineExtensionAttribute (string moduleName)
		{
			this.Name = moduleName;
		}
	}

	public class IodineModule : IodineObject
	{
		private static readonly IodineTypeDefinition ModuleTypeDef = new IodineTypeDefinition ("Module");

		public string Name
		{
			set;
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
			: base (ModuleTypeDef)
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
				Lexer lexer = new Lexer (errorLog, File.ReadAllText (file));
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
				module.Name = Path.GetFileNameWithoutExtension (file);
				if (errorLog.ErrorCount > 0) return null;
				return module;
			} else {
				errorLog.AddError (ErrorType.ParserError, "Could not find module {0}", file);
				return null;
			}
		}

		public static IodineModule LoadModule (ErrorLog errLog, string path)
		{
			if (FindExtension (path) != null) {
				return LoadExtensionModule (Path.GetFileNameWithoutExtension (path), 
					FindExtension (path));
			} else if (FindModule (path) != null) {
				return CompileModule (errLog, FindModule (path));
			}
			return null;
		}

		private static IodineModule LoadExtensionModule (string module, string dll) 
		{
			Assembly extension = Assembly.Load (AssemblyName.GetAssemblyName (dll));

			foreach (Type type in extension.GetTypes ()) {
				IodineExtensionAttribute attr = type.GetCustomAttribute <IodineExtensionAttribute> ();

				if (attr != null) {
					if (attr.Name == module) {
						return (IodineModule)type.GetConstructor (new Type[] {}).Invoke (new object[]{});
					}
				}
			}
			return null;
		}

		private static string FindModule (string name)
		{
			if (File.Exists (name)) {
				return name;
			}
			if (File.Exists (name + ".id")) {
				return name + ".id";
			}

			string exePath = Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location) + "/modules";
		
			foreach (string file in Directory.GetFiles (exePath)) {
				string fname = Path.GetFileName (file);
				if (fname == name || fname == name + ".id") {
					return file;
				}
			}

			return null;
		}

		private static string FindExtension (string name)
		{
			if (File.Exists (name) && name.EndsWith (".dll")) {
				return name;
			}
			if (File.Exists (name + ".dll")) {
				return name + ".dll";
			}

			string exePath = Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location) + "/extensions";

			foreach (string file in Directory.GetFiles (exePath)) {
				string fname = Path.GetFileName (file);
				if (fname == name || fname == name + ".dll") {
					return file;
				}
			}

			return null;
		}
	}
}

