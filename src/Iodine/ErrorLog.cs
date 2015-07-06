using System;
using System.Collections;
using System.Collections.Generic;

namespace Iodine
{
	public class Error
	{
		public string Text {
			private set;
			get;
		}

		public ErrorType EType {
			private set;
			get;
		}

		public Location Location {
			private set;
			get;
		}

		public Error (ErrorType etype, Location location, string text)
		{
			this.EType = etype;
			this.Text = text;
			this.Location = location;
		}
	}

	public class ErrorLog : IEnumerable <Error>
	{
		private List<Error> errors = new List<Error> ();

		public int ErrorCount
		{
			private set;
			get;
		}

		public int WarningCount
		{
			private set;
			get;
		}

		public IList<Error> Errors
		{
			get
			{
				return this.errors;
			}
		}

		public void AddError (ErrorType etype, Location location, string format, params object[] args)
		{
			this.errors.Add (new Error (etype, location, String.Format (format, args)));
			ErrorCount++;
		}

		public IEnumerator <Error> GetEnumerator ()
		{
			foreach (Error error in this.errors) {
				yield return error;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}

