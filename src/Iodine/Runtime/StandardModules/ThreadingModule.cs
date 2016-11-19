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
            class ThreadTypeDefinition : IodineTypeDefinition
            {
                public ThreadTypeDefinition ()
                    : base ("Thread")
                {
                    BindAttributes (this);
                    SetDocumentation (
                        "Creates and controls a thread.",
                        "@param func The function to invoke when this thread is created."
                    );
                }

                public override IodineObject BindAttributes (IodineObject obj)
                {
                    obj.SetAttribute ("start", new BuiltinMethodCallback (Start, obj));
                    obj.SetAttribute ("abort", new BuiltinMethodCallback (Abort, obj));
                    obj.SetAttribute ("alive", new BuiltinMethodCallback (Alive, obj));
                    return obj;
                }

                public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
                {
                    if (args.Length <= 0) {
                        vm.RaiseException (new IodineArgumentException (1));
                        return null;
                    }

                    IodineObject func = args [0];
                    VirtualMachine newVm = new VirtualMachine (vm.Context);

                    Thread t = new Thread (() => {
                        try {
                            func.Invoke (newVm, new IodineObject[] { }); 
                        } catch (UnhandledIodineExceptionException ex) {
                            vm.RaiseException (ex.OriginalException);
                        }
                    });
                    return new IodineThread (t);
                }

                [BuiltinDocString (
                    "Starts the thread."
                )]
                private static IodineObject Start (VirtualMachine vm, IodineObject self, IodineObject[] args)
                {
                    IodineThread thread = self as IodineThread;

                    if (thread == null) {
                        vm.RaiseException (new IodineTypeException (TypeDefinition.Name));
                        return null;
                    }


                    thread.Value.Start ();

                    return null;
                }

                [BuiltinDocString (
                    "Terminates the thread."
                )]
                private static IodineObject Abort (VirtualMachine vm, IodineObject self, IodineObject[] args)
                {
                    IodineThread thread = self as IodineThread;

                    if (thread == null) {
                        vm.RaiseException (new IodineTypeException (TypeDefinition.Name));
                        return null;
                    }

                    thread.Value.Abort ();

                    return null;
                }

                [BuiltinDocString (
                    "Returns true if this thread is alive, false if it is not."
                )]
                private static IodineObject Alive (VirtualMachine vm, IodineObject self, IodineObject[] args)
                {
                    IodineThread thread = self as IodineThread;

                    if (thread == null) {
                        vm.RaiseException (new IodineTypeException (TypeDefinition.Name));
                        return null;
                    }

                    return IodineBool.Create (thread.Value.IsAlive);
                }
            }

            public static readonly IodineTypeDefinition TypeDefinition = new ThreadTypeDefinition ();

            public Thread Value { private set; get; }

            public IodineThread (Thread t)
                : base (TypeDefinition)
            {
                Value = t;
            }
        }

        class IodineLock : IodineObject
        {
            public static readonly IodineTypeDefinition TypeDefinition = new LockTypeDefinition ();

            class LockTypeDefinition : IodineTypeDefinition
            {
                public LockTypeDefinition ()
                    : base ("Lock")
                {
                    BindAttributes (this);
                    SetDocumentation (
                        "Creates and controls a simple spinlock."
                    );
                }

                public override IodineObject BindAttributes (IodineObject obj)
                {
                    obj.SetAttribute ("acquire", new BuiltinMethodCallback (Acquire, obj));
                    obj.SetAttribute ("release", new BuiltinMethodCallback (Release, obj));
                    obj.SetAttribute ("locked", new BuiltinMethodCallback (Locked, obj));
                    return obj;
                }

                public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
                {
                    return new IodineLock ();
                }

                [BuiltinDocString (
                    "Enters the critical section, blocking all threads until release the lock is released."
                )]
                private static IodineObject Acquire (VirtualMachine vm, IodineObject self, IodineObject[] args)
                {
                    IodineLock spinlock = self as IodineLock;

                    if (spinlock == null) {
                        vm.RaiseException (new IodineTypeException (TypeDefinition.Name));
                        return null;
                    }

                    spinlock.Acquire ();
                    return null;
                }

                [BuiltinDocString (
                    "Releases the lock, allowing any threads blocked by this lock to continue."
                )]
                private static IodineObject Release (VirtualMachine vm, IodineObject self, IodineObject[] args)
                {
                    IodineLock spinlock = self as IodineLock;

                    if (spinlock == null) {
                        vm.RaiseException (new IodineTypeException (TypeDefinition.Name));
                        return null;
                    }

                    spinlock.Release ();
                    return null;
                } 

                [BuiltinDocString (
                    "Returns true if a thread has acquired this lock, false if not."
                )]
                private static IodineObject Locked (VirtualMachine vm, IodineObject self, IodineObject[] args)
                {
                    IodineLock spinlock = self as IodineLock;

                    if (spinlock == null) {
                        vm.RaiseException (new IodineTypeException (TypeDefinition.Name));
                        return null;
                    }

                    return IodineBool.Create (spinlock.IsLocked ());
                }
            }

            private volatile bool _lock = false;

            public IodineLock ()
                : base (TypeDefinition)
            {
            }

            public void Acquire ()
            {
                while (_lock)
                    ;
                _lock = true;
            }

            public void Release ()
            {
                _lock = false;
            }

            public bool IsLocked ()
            {
                return _lock;
            }
        }

        class IodineSemaphore : IodineObject
        {
            public static readonly IodineTypeDefinition TypeDefinition = new SemaphoreTypeDefinition ();

            class SemaphoreTypeDefinition : IodineTypeDefinition
            {
                public SemaphoreTypeDefinition ()
                    : base ("Semaphore")
                {
                }

                public override IodineObject Invoke (VirtualMachine vm, IodineObject[] args)
                {
                    if (args.Length == 0) {
                        return new IodineSemaphore (1);
                    }

                    IodineInteger semaphore = args [0] as IodineInteger;

                    if (semaphore == null) {
                        vm.RaiseException (new IodineTypeException ("Integer"));
                        return null;
                    }

                    return new IodineSemaphore ((int)semaphore.Value);
                }
            }

            private volatile int semaphore = 1;

            public IodineSemaphore (int semaphore)
                : base (TypeDefinition)
            {
                this.semaphore = semaphore;
                SetAttribute ("aquire", new BuiltinMethodCallback (Acquire, this));
                SetAttribute ("release", new BuiltinMethodCallback (Release, this));
                SetAttribute ("locked", new BuiltinMethodCallback (IsLocked, this));
            }

            /**
             * Iodine Method: Semaphore.acquire (self)
             * Description: Decrements the semaphore
             */
            private IodineObject Acquire (VirtualMachine vm, IodineObject self, IodineObject[] args)
            {
                semaphore--;
                while (semaphore < 0)
                    ; // Spin
                return null;
            }

            /**
             * Iodine Method: Semaphore.release (self)
             * Description: Increments the semaphore
             */
            private IodineObject Release (VirtualMachine vm, IodineObject self, IodineObject[] args)
            {
                semaphore++;
                return null;
            }

           /**
            * Returns true if the semaphore is less than 0
            */
            private IodineObject IsLocked (VirtualMachine vm, IodineObject self, IodineObject[] args)
            {
                return IodineBool.Create (semaphore < 0);
            }
        }

        public ThreadingModule ()
            : base ("threading")
        {
            SetAttribute ("Thread", IodineThread.TypeDefinition);
            SetAttribute ("Lock", IodineLock.TypeDefinition);
            SetAttribute ("Semaphore", IodineSemaphore.TypeDefinition);
            SetAttribute ("sleep", new BuiltinMethodCallback (Sleep, this));
        }

        [BuiltinDocString (
            "Suspends the current thread for t milliseconds.",
            "@param t How many milliseconds to suspend the thread for"
        )]
        private IodineObject Sleep (VirtualMachine vm, IodineObject self, IodineObject[] args)
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

