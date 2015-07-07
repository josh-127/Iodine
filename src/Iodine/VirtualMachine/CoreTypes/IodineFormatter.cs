using System;
using System.Text;

namespace Iodine
{
	public class IodineFormatter : IodineObject
	{
		public static IodineTypeDefinition FormatterTypeDef = new IodineTypeDefinition ("Formatter");
			 
		public IodineFormatter ()
			: base (FormatterTypeDef)
		{
		}

		public string Format (VirtualMachine vm, string format, IodineObject[] args)
		{
			StringBuilder accum = new StringBuilder ();
			int nextArg = 0;
			int pos = 0;
			while (pos < format.Length) {
				if (format[pos] == '{') {
					string substr = format.Substring (pos + 1);
					if (substr.IndexOf ('}') == -1) {
						return null;
					}
					substr = substr.Substring (0, substr.IndexOf ('}'));
					pos += substr.Length + 2;
					if (substr.Length == 0) {
						accum.Append (args[nextArg++].ToString ());
					} else {
						int index = 0;
						string indexStr = "";
						string specifier = "";

						if (substr.IndexOf (':') == -1) {
							indexStr = substr;
						} else {
							indexStr = substr.Substring (0, substr.IndexOf (":"));
							specifier = substr.Substring (substr.IndexOf (":") + 1);
						}

						if (indexStr == "") {
							index = nextArg++;
						} else if (!Int32.TryParse (indexStr, out index)) {
							return null;
						}

						accum.Append (formatObj (args[index], specifier));

					}
				} else {
					accum.Append (format [pos++]);
				}
			}
			return accum.ToString ();
		}

		private string formatObj (IodineObject obj, string specifier)
		{
			if (specifier.Length == 0) {
				return obj.ToString ();
			}
			char type = specifier[0];
			string args = specifier.Substring (1);
			switch (char.ToLower (type)) {
			case 'd': {
					IodineInteger intObj = obj as IodineInteger;
					int pad = args.Length == 0 ? 0 : int.Parse (args);
					if (intObj == null) return null;
					return intObj.Value.ToString (type.ToString () + pad);
				}
			case 'x': {
					IodineInteger intObj = obj as IodineInteger;
					int pad = args.Length == 0 ? 0 : int.Parse (args);
					if (intObj == null) return null;
					return intObj.Value.ToString (type.ToString () + pad);
				}
			default:
				return null;
			}
		}
	}
}

