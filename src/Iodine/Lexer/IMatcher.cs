using System;

namespace Iodine.Compiler
{
	public interface IMatcher
	{
		bool IsMatchImpl (InputStream instream);
		Token ScanToken (ErrorLog errLog, InputStream instream);
	}
}

