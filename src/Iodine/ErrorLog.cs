using System;
using System.Collections;
using System.Collections.Generic;

namespace Iodine
{
	public class Error
	{
		public string Text
		{
			private set;
			get;
		}

		public ErrorType EType
		{
			private set;
			get;
		}

		public Error (ErrorType etype, string text)
		{
			this.EType = etype;
			this.Text = text;
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

		public void AddError (ErrorType etype, string format, params object[] args)
		{
			this.errors.Add (new Error (etype, String.Format (format, args)));
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

