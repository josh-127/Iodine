using System;

namespace Iodine
{
	public interface IBytecodeOptimization
	{
		void PerformOptimization (IodineMethod method);
	}
}

