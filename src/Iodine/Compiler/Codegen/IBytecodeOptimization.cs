using System;

namespace Iodine.Compiler
{
	public interface IBytecodeOptimization
	{
		void PerformOptimization (IodineMethod method);
	}
}

