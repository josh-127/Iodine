using System;

namespace Iodine
{
	public interface IMatcher
	{
		bool IsMatchImpl (InputStream instream);
		Token ScanToken (ErrorLog errLog, InputStream instream);
	}
}

