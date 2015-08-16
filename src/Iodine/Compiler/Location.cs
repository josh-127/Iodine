using System;

namespace Iodine
{
	public struct Location
	{
		public int Line {
			set;
			get;
		}

		public int Column {
			set;
			get;
		}

		public string File {
			set;
			get;
		}

		public Location (int line, int column, string file)
			: this ()
		{
			this.Line = line;
			this.Column = column;
			this.File = file;
		}

		public Location IncrementLine ()
		{
			return new Location (this.Line + 1, this.Column, this.File);
		}

		public Location IncrementColumn ()
		{
			return new Location (this.Line, this.Column + 1, this.File);
		}
	}
}

