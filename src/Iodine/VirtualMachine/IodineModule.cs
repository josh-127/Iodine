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
		public static readonly List<string> SearchPaths = new List<string> ();
		public static readonly Dictionary<string, IodineModule> ModuleCache = new Dictionary<string,IodineModule> ();
		private static readonly IodineTypeDefinition ModuleTypeDef = new IodineTypeDefinition ("Module");

		static IodineModule ()
		{
			SearchPaths.Add (Environment.CurrentDirectory);
			SearchPaths.Add (String.Format ("{0}{1}modules", Path.GetDirectoryName (
				Assembly.GetEntryAssembly ().Location), Path.DirectorySeparatorChar));
		}

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

		public IodineMethod Initializer
		{
			private set;
			get;
		}

		private bool initialized = false;
		private List<IodineObject> constantPool = new List<IodineObject> ();

		public IodineModule (string name)
			: base (ModuleTypeDef)
		{
			this.Name = name;
			this.Imports = new List<string> ();
			this.Initializer = new IodineMethod (this, "__init__", false, 0, 0);
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

		public override IodineObject GetAttribute (VirtualMachine vm, string name)
		{
			if (!this.initialized && this.Initializer.Body.Count > 0) {
				this.initialized = true;
				vm.InvokeMethod (this.Initializer, null, new IodineObject[]{});
			}
			return base.GetAttribute (vm, name);
		}

		public static IodineModule CompileModule (ErrorLog errorLog, string file)
		{
			if (ModuleCache.ContainsKey (Path.GetFullPath (file)))
				return ModuleCache [Path.GetFullPath (file)];

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
				IodineCompiler compiler = new IodineCompiler (errorLog, symbolTable, Path.GetFullPath (file));
				IodineModule module = new IodineModule (Path.GetFileNameWithoutExtension (file));
				ModuleCache [file] = module;
				compiler.CompileAst (module, root);
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
				string fullPath = FindModule (path);
				if (SearchPaths.Contains (Path.GetDirectoryName (fullPath)))
					SearchPaths.Add (Path.GetDirectoryName (fullPath));
				return CompileModule (errLog, FindModule (path));
			} else if (BuiltInModules.Modules.ContainsKey (path)) {
				return BuiltInModules.Modules [path];
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

			string rawName = Path.GetFileNameWithoutExtension (name);
			foreach (string dir in SearchPaths) {
				string expectedName = String.Format ("{0}{1}{2}.id", dir, Path.DirectorySeparatorChar,
					rawName);
				if (File.Exists (expectedName)) {
					return expectedName;
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
