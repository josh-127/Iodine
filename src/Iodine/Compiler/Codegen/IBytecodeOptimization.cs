using System;
using Iodine.Runtime;

namespace Iodine.Compiler
{
	public interface IBytecodeOptimization
	{
		void PerformOptimization (IodineMethod method);
	}
}

