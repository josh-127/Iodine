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
						if (substr.IndexOf (':') == -1) {
							int index = 0;
							if (Int32.TryParse (substr, out index)) {
								accum.Append (args[index].ToString ());
							} else {
								return null;
							}
						}
					}
				} else {
					accum.Append (format [pos++]);
				}
			}
			return accum.ToString ();
		}
	}
}

