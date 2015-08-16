/**
  * Copyright (c) 2015, GruntTheDivine All rights reserved.

  * Redistribution and use in source and binary forms, with or without modification,
  * are permitted provided that the following conditions are met:
  * 
  *  * Redistributions of source code must retain the above copyright notice, this list
  *    of conditions and the following disclaimer.
  * 
  *  * Redistributions in binary form must reproduce the above copyright notice, this
  *    list of conditions and the following disclaimer in the documentation and/or
  *    other materials provided with the distribution.

  * Neither the name of the copyright holder nor the names of its contributors may be
  * used to endorse or promote products derived from this software without specific
  * prior written permission.
  * 
  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
  * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
  * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
  * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
  * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
  * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
  * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
  * CONTRACT ,STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
  * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
  * DAMAGE.
**/

using System;
using System.Threading;
using System.Security.Cryptography;

namespace Iodine.Runtime
{
	[IodineBuiltinModule ("threading")]
	public class ThreadingModule : IodineModule
	{
		class IodineThread : IodineObject
		{
			public static readonly IodineTypeDefinition ThreadTypeDef = new IodineTypeDefinition ("Thread");

			public Thread Value {
				private set;
				get;
			}

			public IodineThread (Thread t)
				: base (ThreadTypeDef)
			{
				this.Value = t;
				this.SetAttribute ("start", new InternalMethodCallback (start, this));
			}

			private IodineObject start (VirtualMachine vm, IodineObject self, IodineObject[] args)
			{
				Value.Start ();
				return null;
			}
		}

		public ThreadingModule ()
			: base ("threading")
		{
			this.SetAttribute ("Thread", new InternalMethodCallback (thread, this));
			this.SetAttribute ("sleep", new InternalMethodCallback (sleep, this));
		}

		private IodineObject thread (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
				return null;
			}
			IodineObject func = args [0];

			Thread t = new Thread (() => {
				VirtualMachine newVm = new VirtualMachine (vm.Globals);
				func.Invoke (newVm, new IodineObject[] { });
			});
			return new IodineThread (t);
		}

		private IodineObject sleep (VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			if (args.Length <= 0) {
				vm.RaiseException (new IodineArgumentException (1));
			}
			IodineInteger time = args [0] as IodineInteger;
			System.Threading.Thread.Sleep ((int)time.Value);
			return null;
		}
	}
}

