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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Iodine.Util;
using Iodine.Compiler;

namespace Iodine.Runtime
{
    // Callback for debugger
    public delegate bool TraceCallback (TraceType type,
        VirtualMachine vm,
        StackFrame frame,
        SourceLocation location
    );

    public enum TraceType
    {
        Line,
        Exception,
        Function
    }

    /// <summary>
    /// Represents an instance of an Iodine virtual machine. Each Iodine thread gets its own
    /// instance of this class
    /// </summary>
    public sealed class VirtualMachine
    {
        public static readonly Dictionary<string, IodineModule> ModuleCache = new Dictionary<string, IodineModule> ();

        public readonly IodineContext Context;

        private int frameCount = 0;
        private int stackSize = 0;

        private TraceCallback traceCallback = null;
        private IodineObject lastObject;
        private IodineObject lastException = null;
        private SourceLocation currentLocation;
        private Instruction instruction;
        private LinkedStack<StackFrame> frames = new LinkedStack<StackFrame> ();
        private ManualResetEvent pauseVirtualMachine = new ManualResetEvent (true);

        public StackFrame Top;

        public VirtualMachine (IodineContext context)
        {
            Context = context;
            context.ResolveModule += (name) => {
                if (BuiltInModules.Modules.ContainsKey (name)) {
                    return BuiltInModules.Modules [name];
                }
                return null;
            };
        }

        /// <summary>
        /// Returns a string representing the current stack tracee
        /// </summary>
        /// <returns>The stack trace.</returns>
        public string GetStackTrace ()
        {
            StringBuilder accum = new StringBuilder ();
            StackFrame top = Top;
            while (top != null) {
                accum.AppendFormat (" at {0} (Module: {1}, Line: {2})\n",
                    top.Method != null ? top.Method.Name : "",
                    top.Module.Name,
                    top.Location.Line + 1
                );

                top = top.Parent;
            }

            return accum.ToString ();
        }

        /// <summary>
        /// Resumes execution
        /// </summary>
        public void ContinueExecution ()
        {
            pauseVirtualMachine.Set ();
        }

        /// <summary>
        /// Executes an Iodine method
        /// </summary>
        /// <returns>Value evaluated on return (null if void).</returns>
        /// <param name="method">Method.</param>
        /// <param name="self">self pointer.</param>
        /// <param name="arguments">Arguments.</param>
        public IodineObject InvokeMethod (IodineMethod method, IodineObject self, IodineObject[] arguments)
        {
            int requiredArgs = method.AcceptsKeywordArgs ? method.ParameterCount - 1 : method.ParameterCount;
            if ((method.Variadic && arguments.Length + 1 < requiredArgs) ||
                (!method.Variadic && arguments.Length < requiredArgs)) {
                RaiseException (new IodineArgumentException (method.ParameterCount));
                return null;
            }

            NewFrame (method, arguments, self);

            return Invoke (method, arguments);
        }

        /// <summary>
        /// Executes an Iodine method using a preallocated stack frame. this is used for 
        /// closures 
        /// </summary>
        /// <returns>The method.</returns>
        /// <param name="method">Method.</param>
        /// <param name="frame">Frame.</param>
        /// <param name="self">Self.</param>
        /// <param name="arguments">Arguments.</param>
        public IodineObject InvokeMethod (IodineMethod method,
            StackFrame frame,
            IodineObject self,
            IodineObject[] arguments)
        {
            int requiredArgs = method.AcceptsKeywordArgs ? method.ParameterCount - 1
                : method.ParameterCount;

            requiredArgs -= method.DefaultValues.Length;

            if ((method.Variadic && arguments.Length + 1 < requiredArgs) ||
                (!method.Variadic && arguments.Length < requiredArgs)) {
                RaiseException (new IodineArgumentException (method.ParameterCount));
                return null;
            }

            NewFrame (frame);

            return Invoke (method, arguments);
        }

        /*
         * Internal implementation of Invoke
         */
        private IodineObject Invoke (IodineMethod method, IodineObject[] arguments)
        {
            if (method.Bytecode.Instructions.Length > 0) {
                currentLocation = method.Bytecode.Instructions [0].Location;
            }

            int insCount = method.Bytecode.Instructions.Length;
            int prevStackSize = stackSize;
            int i = 0;
            lastObject = null;

            /*
             * Store function arguments into their respective local variable slots
             */
            foreach (string param in method.Parameters) {
                if (param == method.VarargsParameter) {
                    // Variable list arguments
                    IodineObject[] tupleItems = new IodineObject[arguments.Length - i];
                    Array.Copy (arguments, i, tupleItems, 0, arguments.Length - i);
                    Top.StoreLocalExplicit (param, new IodineTuple (tupleItems));

                } else if (param == method.KwargsParameter) {
                    /*
                     * At the moment, keyword arguments are passed to the function as an IodineHashMap,
                     */
                    if (i < arguments.Length && arguments [i] is IodineDictionary) {
                        Top.StoreLocalExplicit (param, arguments [i]);
                    } else {
                        Top.StoreLocalExplicit (param, new IodineDictionary ());
                    }
                } else {
                    if (arguments.Length <= i && method.HasDefaultValues) {
                        Top.StoreLocalExplicit (param, method.DefaultValues [i - method.DefaultValuesStartIndex]);
                    } else {
                        Top.StoreLocalExplicit (param, arguments [i++]);
                    }
                }
            }

            StackFrame top = Top;
            top.Module = method.Module;
            if (traceCallback != null) {
                Trace (TraceType.Function, top, currentLocation);
            }

            IodineObject retVal = EvalCode (method.Bytecode);
           
            if (top.Yielded) {
                top.Pop ();
            }

            /*
             * Calls __exit__ on any object used in a with statement
             */
            while (!top.Yielded && top.DisposableObjects.Count > 0) {
                top.DisposableObjects.Pop ().Exit (this);
            }

            stackSize = prevStackSize;

            if (top.AbortExecution) {
                /*
                 * If AbortExecution was set, something went wrong and we most likely just
                 * raised an exception. We'll return right here and let what ever catches 
                 * the exception clean up the stack
                 */
                return retVal;
            }

            EndFrame ();

            return retVal;
        }

        /// <summary>
        /// Evaluates an Iodine code object
        /// </summary>
        /// <returns>The code.</returns>
        /// <param name="bytecode">Bytecode.</param>
        public IodineObject EvalCode (CodeObject bytecode)
        {
            int insCount = bytecode.Instructions.Length;
            StackFrame top = Top;
            top.Location = currentLocation;
            while (top.InstructionPointer < insCount && !top.AbortExecution && !top.Yielded) {
                instruction = bytecode.Instructions [top.InstructionPointer++];
                if (traceCallback != null && 
                    instruction.Location != null &&
                    (top.Location == null || instruction.Location.Line != top.Location.Line)) {
                    Trace (TraceType.Line, top, instruction.Location);
                }
                EvalInstruction ();
                top.Location = currentLocation;
            }
            return lastObject ?? IodineNull.Instance;
        }

        /// <summary>
        /// Raises a generic Iodine exception
        /// </summary>
        /// <param name="message">Format.</param>
        /// <param name="args">Arguments.</param>
        public void RaiseException (string message, params object[] args)
        {
            RaiseException (new IodineException (message, args));
        }

        /// <summary>
        /// Raises an exception, throwing 'ex' as an IodineException object
        /// </summary>
        /// <param name="ex">Exception to raise.</param>
        public void RaiseException (IodineObject ex)
        {
            if (traceCallback != null) {
                traceCallback (TraceType.Exception, this, Top, currentLocation);
            }

            IodineExceptionHandler handler = PopCurrentExceptionHandler ();

            if (handler == null) { // No exception handler
               /*
                * The program has gone haywire and we ARE going to crash, however
                * we must attempt to properly dispose any objects created inside 
                * Iodine's with statement
                */
                StackFrame top = Top;
                while (top != null) {
                    while (top.DisposableObjects.Count > 0) {
                        IodineObject obj = top.DisposableObjects.Pop ();
                        try {
                            obj.Exit (this); // Call __exit__
                        } catch (UnhandledIodineExceptionException) {
                            // Ignore this, we will throw one when we're done anyway
                        }
                    }
                    top = top.Parent;
                }
                throw new UnhandledIodineExceptionException (Top, ex);
            }

            ex.SetAttribute ("stacktrace", new IodineString (GetStackTrace ()));

            UnwindStack (frameCount - handler.Frame);
            lastException = ex;
            Top.InstructionPointer = handler.InstructionPointer;
        }

        /// <summary>
        /// Sets the trace callback function (For debugging).
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void SetTrace (TraceCallback callback)
        {
            traceCallback = callback;
        }

        private void Trace (TraceType type, StackFrame frame, SourceLocation location)
        {
            pauseVirtualMachine.WaitOne ();
            if (traceCallback != null && traceCallback (type, this, frame, location)) {
                pauseVirtualMachine.Reset ();
            }
        }

        /// <summary>
        /// Unwinds the stack n frames
        /// </summary>
        /// <param name="frames">Frames.</param>
        private void UnwindStack (int frames)
        {
            for (int i = 0; i < frames; i++) {
                StackFrame frame = this.frames.Pop ();
                frame.AbortExecution = true;
            }
            frameCount -= frames;
            Top = this.frames.Peek ();
        }

        private IodineExceptionHandler PopCurrentExceptionHandler ()
        {
            StackFrame current = Top;
            while (current != null) {
                if (current.ExceptionHandlers.Count > 0) {
                    return current.ExceptionHandlers.Pop ();
                }
                current = current.Parent;
            }
            return null;
        }

        #if DOTNET_45
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        #endif
        private void EvalInstruction ()
        {
            if (instruction.Location != null) {
                currentLocation = instruction.Location;
            }

            switch (instruction.OperationCode) {
            case Opcode.Pop:
                {
                    Pop ();
                    break;
                }
            case Opcode.Dup:
                {
                    IodineObject val = Pop ();
                    Push (val);
                    Push (val);
                    break;
                }
            case Opcode.LoadConst:
                {
                    Push (Top.Module.ConstantPool [instruction.Argument]);
                    break;
                }
            case Opcode.LoadNull:
                {
                    Push (IodineNull.Instance);
                    break;
                }
            case Opcode.LoadSelf:
                {
                    Push (Top.Self);
                    break;
                }
            case Opcode.LoadTrue:
                {
                    Push (IodineBool.True);
                    break;
                }
            case Opcode.LoadException:
                {
                    Push (lastException);
                    break;
                }
            case Opcode.LoadFalse:
                {
                    Push (IodineBool.False);
                    break;
                }
            case Opcode.StoreLocal:
                {
                    string name = ((IodineName)Top.Module.ConstantPool [instruction.Argument]).Value;
                    Top.StoreLocal (name, Pop ());
                    break;
                }
            case Opcode.LoadLocal:
                {
                    string name = ((IodineName)Top.Module.ConstantPool [instruction.Argument]).Value;
                    Push (Top.LoadLocal (name));
                    break;
                }
            case Opcode.StoreGlobal:
                {
                    string name = ((IodineName)Top.Module.ConstantPool [instruction.Argument]).Value;
                    Top.Module.SetAttribute (this, name, Pop ());
                    break;
                }
            case Opcode.LoadGlobal:
                {
                    string name = ((IodineName)Top.Module.ConstantPool [instruction.Argument]).Value;
                    if (name == "_") {
                        Push (Top.Module);
                    } else if (Top.Module.Attributes.ContainsKey (name)) {
                        Push (Top.Module.GetAttribute (this, name));
                    } else {
                        RaiseException (new IodineAttributeNotFoundException (name));
                    }
                    break;
                }
            case Opcode.StoreAttribute:
                {
                    IodineObject target = Pop ();
                    IodineObject value = Pop ();
                    string attribute = ((IodineName)Top.Module.ConstantPool [instruction.Argument]).Value;
                    if (target.Attributes.ContainsKey (attribute) &&
                        target.Attributes [attribute] is IIodineProperty) {
                        IIodineProperty property = (IIodineProperty)target.Attributes [attribute];
                        property.Set (this, value);
                        break;
                    }
                    target.SetAttribute (this, attribute, value);
                    break;
                }
            case Opcode.LoadAttribute:
                {
                    IodineObject target = Pop ();
                    string attribute = ((IodineName)Top.Module.ConstantPool [instruction.Argument]).Value;
                    if (target.Attributes.ContainsKey (attribute) &&
                        target.Attributes [attribute] is IIodineProperty) {
                        IIodineProperty property = (IIodineProperty)target.Attributes [attribute];
                        Push (property.Get (this));
                        break;
                    }
                    Push (target.GetAttribute (this, attribute));
                    break;
                }
            case Opcode.LoadAttributeOrNull:
                {
                    IodineObject target = Pop ();
                    string attribute = ((IodineName)Top.Module.ConstantPool [instruction.Argument]).Value;
                    if (target.Attributes.ContainsKey (attribute)) {
                        Push (target.GetAttribute (this, attribute));
                    } else {
                        Push (IodineNull.Instance);
                    }
                    break;
                }
            case Opcode.StoreIndex:
                {
                    IodineObject index = Pop ();
                    IodineObject target = Pop ();
                    IodineObject value = Pop ();
                    target.SetIndex (this, index, value);
                    break;
                }
            case Opcode.LoadIndex:
                {
                    IodineObject index = Pop ();
                    IodineObject target = Pop ();
                    Push (target.GetIndex (this, index));
                    break;
                }
            case Opcode.CastLocal:
                {
                    IodineTypeDefinition type = Pop () as IodineTypeDefinition;
                    string name = ((IodineName)Top.Module.ConstantPool [instruction.Argument]).Value;
                    IodineObject o = Top.LoadLocal (name);
                    if (type == null) {
                        RaiseException (new IodineTypeException ("TypeDef"));
                        break;
                    }
                    if (o.InstanceOf (type)) {
                        Push (o);
                    } else {
                        RaiseException (new IodineTypeException (type.Name));
                    }
                    break;
                }
            case Opcode.BinOp:
                {
                    IodineObject op2 = Pop ();
                    IodineObject op1 = Pop ();
                    Push (op1.PerformBinaryOperation (this,
                        (BinaryOperation)instruction.Argument,
                        op2
                    ));
                    break;
                }
            case Opcode.UnaryOp:
                {
                    Push (Pop ().PerformUnaryOperation (this, 
                        (UnaryOperation)instruction.Argument));
                    break;
                }
            case Opcode.Invoke:
                {
                    IodineObject target = Pop ();
                    IodineObject[] arguments = new IodineObject[instruction.Argument];
                    for (int i = 1; i <= instruction.Argument; i++) {
                        arguments [instruction.Argument - i] = Pop ();
                    }
                    Push (target.Invoke (this, arguments));
                    break;
                }
            case Opcode.InvokeVar:
                {
                    IodineObject target = Pop ();
                    List<IodineObject> arguments = new List<IodineObject> ();
                    IodineTuple tuple = Pop () as IodineTuple;
                    if (tuple == null) {
                        RaiseException (new IodineTypeException ("Tuple"));
                        break;
                    }
                    for (int i = 0; i < instruction.Argument; i++) {
                        arguments.Add (Pop ());
                    }
                    arguments.AddRange (tuple.Objects);
                    Push (target.Invoke (this, arguments.ToArray ()));
                    break;
                }
            case Opcode.InvokeSuper:
                {
                    IodineTypeDefinition target = Pop () as IodineTypeDefinition;
                    IodineObject[] arguments = new IodineObject[instruction.Argument];
                    for (int i = 1; i <= instruction.Argument; i++) {
                        arguments [instruction.Argument - i] = Pop ();
                    }

                    target.Inherit (this, Top.Self, arguments);
                    break;
                }
            case Opcode.Return:
                {
                    Top.InstructionPointer = int.MaxValue;
                    break;
                }
            case Opcode.Yield:
                {
                    Top.Yielded = true;
                    break;
                }
            case Opcode.JumpIfTrue:
                {
                    if (Pop ().IsTrue ()) {
                        Top.InstructionPointer = instruction.Argument;
                    }
                    break;
                }
            case Opcode.JumpIfFalse:
                {
                    if (!Pop ().IsTrue ()) {
                        Top.InstructionPointer = instruction.Argument;
                    }
                    break;
                }
            case Opcode.Jump:
                {
                    Top.InstructionPointer = instruction.Argument;
                    break;
                }
            case Opcode.BuildClass:
                {
                    IodineName name = Pop () as IodineName;
                    IodineString doc = Pop () as IodineString;
                    IodineMethod constructor = Pop () as IodineMethod;
                    //CodeObject initializer = Pop as CodeObject;
                    IodineTypeDefinition baseClass = Pop () as IodineTypeDefinition;
                    IodineTuple interfaces = Pop () as IodineTuple;
                    IodineClass clazz = new IodineClass (name.ToString (), new CodeObject (), constructor);

                    if (baseClass != null) {
                        clazz.BaseClass = baseClass;
                        baseClass.BindAttributes (clazz);
                    }

                    for (int i = 0; i < instruction.Argument; i++) {
                        IodineObject val = Pop ();
                        IodineObject key = Pop ();

                        clazz.Attributes [val.ToString ()] = key;
                    }

                    foreach (IodineObject obj in interfaces.Objects) {
                        IodineContract contract = obj as IodineContract;
                        if (!contract.InstanceOf (clazz)) {
                            //RaiseException (new IodineTypeException (contract.Name));
                            break;
                        }
                    }

                    clazz.SetAttribute ("__doc__", doc);

                    Push (clazz);
                    break;
                }
            case Opcode.BuildMixin:
                {
                    IodineName name = Pop () as IodineName;

                    IodineMixin mixin = new IodineMixin (name.ToString ());

                    for (int i = 0; i < instruction.Argument; i++) {
                        IodineObject val = Pop ();
                        IodineObject key = Pop ();

                        mixin.Attributes [val.ToString ()] = key;
                    }

                    Push (mixin);
                    break;
                }
            case Opcode.BuildEnum:
                {
                    IodineName name = Pop () as IodineName;

                    IodineEnum ienum = new IodineEnum (name.ToString ());
                    for (int i = 0; i < instruction.Argument; i++) {
                        IodineInteger val = Pop () as IodineInteger;
                        IodineName key = Pop () as IodineName;
                        ienum.AddItem (key.ToString (), (int)val.Value);
                    }

                    Push (ienum);
                    break;
                }
            case Opcode.BuildContract:
                {
                    IodineName name = Pop () as IodineName;

                    IodineContract contract = new IodineContract (name.ToString ());
                    for (int i = 0; i < instruction.Argument; i++) {
                        IodineMethod val = Pop () as IodineMethod;
                        contract.AddMethod (val);
                    }

                    Push (contract);
                    break;
                }
            case Opcode.BuildTrait:
                {
                    IodineName name = Pop () as IodineName;

                    IodineTrait trait = new IodineTrait (name.ToString ());
                    for (int i = 0; i < instruction.Argument; i++) {
                        IodineMethod val = Pop () as IodineMethod;
                        trait.AddMethod (val);
                    }

                    Push (trait);
                    break;
                }
            case Opcode.BuildHash:
                {
                    IodineDictionary hash = new IodineDictionary ();
                    for (int i = 0; i < instruction.Argument; i++) {
                        IodineObject val = Pop ();
                        IodineObject key = Pop ();
                        hash.Set (key, val);
                    }
                    Push (hash);
                    break;
                }
            case Opcode.BuildList:
                {
                    IodineObject[] items = new IodineObject[instruction.Argument];
                    for (int i = 1; i <= instruction.Argument; i++) {
                        items [instruction.Argument - i] = Pop ();
                    }
                    Push (new IodineList (items));
                    break;
                }
            case Opcode.BuildTuple:
                {
                    IodineObject[] items = new IodineObject[instruction.Argument];
                    for (int i = 1; i <= instruction.Argument; i++) {
                        items [instruction.Argument - i] = Pop ();
                    }
                    Push (new IodineTuple (items));
                    break;
                }
            case Opcode.BuildClosure:
                {
                    IodineObject obj = Pop ();
                    IodineMethod method = obj as IodineMethod;
                    Push (new IodineClosure (Top, method));
                    break;
                }

            case Opcode.BuildGenExpr:
                {
                    CodeObject method = Pop () as CodeObject;
                    Push (new IodineGeneratorExpr (Top, method));
                    break;
                }
            case Opcode.Slice:
                {
                    IodineObject target = Pop ();

                    IodineInteger[] arguments = new IodineInteger[3];

                    for (int i = 0; i < 3; i++) {
                        IodineObject obj = Pop ();
                        arguments [i] = obj as IodineInteger;

                        if (obj != IodineNull.Instance && arguments [i] == null) {
                            RaiseException (new IodineTypeException ("Int"));
                            break;
                        }
                    }

                    IodineSlice slice = new IodineSlice (arguments [0], arguments [1], arguments [2]);

                    Push (target.Slice (this, slice));

                    break;
                }
            case Opcode.MatchPattern:
                {
                    IodineObject collection = Pop ().GetIterator (this);

                    IodineObject[] items = new IodineObject[instruction.Argument];
                    for (int i = 1; i <= instruction.Argument; i++) {
                        items [instruction.Argument - i] = Pop ();
                    }


                    int index = 0;

                    collection.IterReset (this);

                    while (collection.IterMoveNext (this) && index < items.Length) {

                        IodineObject o = collection.IterGetCurrent (this);

                        if (items [index] is IodineTypeDefinition) {
                            if (!o.InstanceOf (items [index] as IodineTypeDefinition)) {
                                Push (IodineBool.False);
                                break;
                            }
                        } else {
                            if (!o.Equals (items [index])) {
                                Push (IodineBool.False);
                                break;
                            }
                        }

                        index++;
                    }
                        
                    Push (IodineBool.Create (index == items.Length));

                    break;
                }
            case Opcode.Unwrap:
                {
                    IodineObject container = Pop ();

                    IodineObject value = container.Unwrap (this);

                    if (instruction.Argument > 0) {
                        IodineInteger len = value.Len (this) as IodineInteger;

                        if (len == null || len.Value != instruction.Argument) {
                            Push (IodineBool.False);
                            break;
                        }
                    }

                    Push (value);
                    Push (IodineBool.True);

                    break;
                }
            case Opcode.Unpack:
                {
                    IodineTuple tuple = Pop () as IodineTuple;

                    if (tuple == null) {
                        RaiseException (new IodineTypeException ("Tuple"));
                        break;
                    }

                    if (tuple.Objects.Length != instruction.Argument) {
                        RaiseException (new IodineUnpackException (instruction.Argument));
                        break;
                    }
                    for (int i = tuple.Objects.Length - 1; i >= 0; i--) {
                        Push (tuple.Objects [i]);
                    }
                    break;
                }
            case Opcode.GetIter:
                {
                    Push (Pop ().GetIterator (this));
                    break;
                }
            case Opcode.IterGetNext:
                {
                    Push (Pop ().IterGetCurrent (this));
                    break;
                }
            case Opcode.IterMoveNext:
                {
                    Push (IodineBool.Create (Pop ().IterMoveNext (this)));
                    break;
                }
            case Opcode.IterReset:
                {
                    Pop ().IterReset (this);
                    break;
                }
            case Opcode.PushExceptionHandler:
                {
                    Top.ExceptionHandlers.Push (new IodineExceptionHandler (frameCount, instruction.Argument));
                    break;
                }
            case Opcode.PopExceptionHandler:
                {
                    Top.ExceptionHandlers.Pop ();
                    break;
                }
            case Opcode.InstanceOf:
                {
                    IodineObject o = Pop ();
                    IodineTypeDefinition type = Pop () as IodineTypeDefinition;
                    if (type == null) {
                        RaiseException (new IodineTypeException ("TypeDef"));
                        break;
                    }
                    Push (IodineBool.Create (o.InstanceOf (type)));
                    break;
                }
            case Opcode.DynamicCast:
                {
                    IodineObject o = Pop ();
                    IodineTypeDefinition type = Pop () as IodineTypeDefinition;
                    if (type == null) {
                        RaiseException (new IodineTypeException ("TypeDef"));
                        break;
                    }
                    if (o.InstanceOf (type)) {
                        Push (o);
                    } else {
                        Push (IodineNull.Instance);
                    }
                    break;
                }
            case Opcode.NullCoalesce:
                {
                    IodineObject o1 = Pop ();
                    IodineObject o2 = Pop ();
                    if (o1 is IodineNull) {
                        Push (o2);
                    } else {
                        Push (o1);
                    }
                    break;
                }
            case Opcode.BeginExcept:
                {
                    bool rethrow = true;
                    for (int i = 1; i <= instruction.Argument; i++) {
                        IodineTypeDefinition type = Pop () as IodineTypeDefinition;
                        if (type == null) {
                            RaiseException (new IodineTypeException ("TypeDef"));
                            break;
                        }

                        if (lastException.InstanceOf (type)) {
                            rethrow = false;
                            break;
                        }
                    }
                    if (rethrow) {
                        RaiseException (lastException);
                    }
                    break;
                }
            case Opcode.Raise:
                {
                    IodineObject e = Pop ();
                    if (e.InstanceOf (IodineException.TypeDefinition)) {
                        RaiseException (e);
                    } else {
                        RaiseException (new IodineTypeException ("Exception"));
                    }
                    break;
                }
            case Opcode.SwitchLookup:
                {
                    Dictionary<int, IodineObject> lookup = new Dictionary<int, IodineObject> ();
                    int needle = Pop ().GetHashCode ();
                    for (int i = 0; i < instruction.Argument; i++) {
                        IodineObject value = Pop ();
                        IodineObject key = Pop ();
                        lookup [key.GetHashCode ()] = value;
                    }
                    if (lookup.ContainsKey (needle)) {
                        lookup [needle].Invoke (this, new IodineObject[] { });
                        Push (IodineBool.True);
                    } else {
                        Push (IodineBool.False);
                    }
                    break;
                }
            case Opcode.BeginWith:
                {
                    IodineObject obj = Pop ();
                    obj.Enter (this);
                    Top.DisposableObjects.Push (obj);
                    break;
                }
            case Opcode.EndWith:
                {
                    Top.DisposableObjects.Pop ().Exit (this);
                    break;
                }
            case Opcode.IncludeMixin:
                {
                    IodineObject obj = Pop ();
                    IodineObject type = Pop ();

                    foreach (KeyValuePair<string, IodineObject> attr in obj.Attributes) {
                        type.SetAttribute (attr.Key, attr.Value);
                    }
                    break;
                }
            case Opcode.ApplyMixin:
                {
                    IodineObject type = Pop ();
                    IodineMixin mixin = Top.Module.ConstantPool [instruction.Argument] as IodineMixin;

                    foreach (KeyValuePair<string, IodineObject> attr in mixin.Attributes) {
                        type.SetAttribute (attr.Key, attr.Value);
                    }
                    break;
                }
            case Opcode.BuildFunction:
                {
                    MethodFlags flags = (MethodFlags)instruction.Argument;

                    IodineString name = Pop () as IodineString;
                    IodineString doc = Pop () as IodineString;
                    CodeObject bytecode = Pop () as CodeObject;
                    IodineTuple parameters = Pop () as IodineTuple;

                    IodineObject[] defaultValues = new IodineObject[] { };
                    int defaultValuesStart = 0;

                    if (flags.HasFlag (MethodFlags.HasDefaultParameters)) {
                        IodineTuple defaultValuesTuple = Pop () as IodineTuple;
                        IodineInteger startInt = Pop () as IodineInteger;
                        defaultValues = defaultValuesTuple.Objects;
                        defaultValuesStart = (int)startInt.Value;
                    }

                    IodineMethod method = new IodineMethod (
                        Top.Module,
                        name,
                        bytecode,
                        parameters,
                        flags,
                        defaultValues,
                        defaultValuesStart
                    );

                    method.SetAttribute ("__doc__", doc);

                    Push (method);

                    break;
                }
            }

        }

        #if DOTNET_45
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        #endif
        /// <summary>
        /// Pushes an item onto the evaluation stack
        /// </summary>
        /// <param name="obj">Object.</param>
        private void Push (IodineObject obj)
        {
            if (stackSize >= Context.Configuration.StackLimit) {
                RaiseException (new IodineStackOverflow ());
                return;
            }
            lastObject = obj;
            stackSize++;
            Top.Push (obj);
        }

        #if DOTNET_45
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        #endif
        private IodineObject Pop ()
        {
            stackSize--;
            return Top.Pop ();
        }

        #if DOTNET_45
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        #endif
        public void NewFrame (StackFrame frame)
        {
            frameCount++;
            stackSize++;
            Top = frame;
            frames.Push (frame);
        }

        #if DOTNET_45
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        #endif
        private void NewFrame (IodineMethod method, IodineObject[] args, IodineObject self)
        {
            frameCount++;
            stackSize++;
            Top = new StackFrame (method.Module, method, args, Top, self);
            frames.Push (Top);
        }

        #if DOTNET_45
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        #endif
        public StackFrame EndFrame ()
        {
            frameCount--;
            stackSize--;
            StackFrame ret = frames.Pop ();
            if (frames.Count != 0) {
                Top = frames.Peek ();
            } else {
                Top = null;
            }
            return ret;
        }

        public IodineModule LoadModule (string name, bool useCache = true)
        {
            IodineModule module = Context.LoadModule (name, useCache);
            if (module == null) {
                throw new ModuleNotFoundException (name, Context.SearchPath);
            }
            return module;
        }

        /// <summary>
        /// Sets a global variable 
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="val">Value.</param>
        public void SetGlobal (string name, IodineObject val)
        {
            Top.Module.SetAttribute (name, val);
        }
    }
}


