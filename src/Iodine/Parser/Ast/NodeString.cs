using System;
using System.Collections.Generic;

namespace Iodine
{
	public class NodeString : AstNode
	{
		public string Value
		{
			private set;
			get;
		}

		public NodeString (string value)
		{
			this.Value = value;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (string str)
		{
			int pos = 0;
			string accum = "";
			List<string> vars = new List<string> ();
			while (pos < str.Length) {
				if (str[pos] == '#' && str.Length != pos + 1 && str[pos + 1] == '{') {
					string substr = str.Substring (pos + 2);
					if (substr.IndexOf ('}') == -1) return null;
					substr = substr.Substring (0, substr.IndexOf ('}'));
					pos += substr.Length + 3;
					vars.Add (substr);
					accum += "{}";

				} else {
					accum += str[pos++];
				}
			}
			NodeString ret = new NodeString (accum);

			foreach (string name in vars) {
				ret.Add (new NodeIdent (name));
			}
			return ret;
		}
	}
}

