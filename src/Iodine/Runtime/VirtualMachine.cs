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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Iodine.Compiler;

namespace Iodine.Runtime
{
	public sealed class VirtualMachine
	{
		public static readonly Dictionary<string, IodineModule> ModuleCache = new Dictionary<string, IodineModule> ();

		private LinkedStack<StackFrame> frames = new LinkedStack<StackFrame> ();
		private int frameCount = 0;
		private IodineObject last;

		private Dictionary<string, IodineObject> globalDict = new Dictionary<string, IodineObject> ();
		private Stack<IodineExceptionHandler> exceptionHandlers = new Stack<IodineExceptionHandler> ();
		private IodineObject lastException = null;
		private Location currLoc;
		private Instruction instruction;

		public StackFrame Top;

		public Dictionary <string, IodineObject> Globals {
			get {
				return globalDict;
			}
		}
			
		public VirtualMachine ()
		{
			var modules = BuiltInModules.Modules.Values.Where (p => p.ExistsInGlobalNamespace);
			foreach (IodineModule module in modules) {
				foreach (KeyValuePair<string, IodineObject> val in module.Attributes) {
					Globals [val.Key] = val.Value;
				}
			}
		}

		public VirtualMachine (Dictionary<string, IodineObject> globals)
		{
			globalDict = globals;
		}

		public string Trace ()
		{
			StringBuilder accum = new StringBuilder ();
			StackFrame top = Top;
			while (top != null) {
				if (top is NativeStackFrame) {
					NativeStackFrame frame = top as NativeStackFrame;

					accum.AppendFormat (" at {0} <internal method>\n",
						frame.NativeMethod.Callback.Method.Name);
				} else {
					accum.AppendFormat (" at {0} (Module: {1}, Line: {2})\n",
						top.Method.Name,
						top.Module.Name,
						top.Location.Line + 1);
				}
				top = top.Parent;
			}

			return accum.ToString ();
		}

		public void Unwind (int frames)
		{
			for (int i = 0; i < frames; i++) {
				StackFrame frame = this.frames.Pop ();
				frame.AbortExecution = true;
			}
			frameCount -= frames;
			Top = this.frames.Peek ();
		}

		public IodineObject InvokeMethod (IodineMethod method, IodineObject self, IodineObject[] arguments)
		{
			int requiredArgs = method.AcceptsKeywordArgs ? method.ParameterCount - 1 : method.ParameterCount;
			if ((method.Variadic && arguments.Length + 1 < requiredArgs) ||
				(!method.Variadic && arguments.Length < requiredArgs)) {
				RaiseException (new IodineArgumentException (method.ParameterCount));
				return null;
			}

			NewFrame (method, self, method.LocalCount);

			return Invoke (method, arguments);
		}

		public IodineObject InvokeMethod (IodineMethod method, StackFrame frame, IodineObject self,
		                                  IodineObject[] arguments)
		{
			int requiredArgs = method.AcceptsKeywordArgs ? method.ParameterCount - 1 :
				method.ParameterCount;
			if ((method.Variadic && arguments.Length + 1 < requiredArgs) ||
				(!method.Variadic && arguments.Length < requiredArgs)) {
				RaiseException (new IodineArgumentException (method.ParameterCount));
				return null;
			}

			NewFrame (frame);
			return Invoke (method, arguments);
		}

		private IodineObject Invoke (IodineMethod method, IodineObject[] arguments)
		{
			if (method.Body.Count > 0) {
				currLoc = method.Body [0].Location;
			}

			int insCount = method.Body.Count;
			int i = 0;
			foreach (string param in method.Parameters.Keys) {
				if (method.Variadic && (method.AcceptsKeywordArgs ? i == method.Parameters.Keys.Count - 2 :
					i == method.Parameters.Keys.Count - 1)) {
					IodineObject[] tupleItems = new IodineObject[arguments.Length - i];
					Array.Copy (arguments, i, tupleItems, 0, arguments.Length - i);
					Top.StoreLocal (method.Parameters [param], new IodineTuple (tupleItems));
				} else if (i == method.Parameters.Keys.Count - 1 && method.AcceptsKeywordArgs) {
					if (i < arguments.Length && arguments [i] is IodineHashMap) {
						Top.StoreLocal (method.Parameters [param], arguments [i]);
					} else {
						Top.StoreLocal (method.Parameters [param], new IodineHashMap ());
					}
				} else {
					Top.StoreLocal (method.Parameters [param], arguments [i++]);
				}
			}

			StackFrame top = Top;
			while (top.InstructionPointer < insCount && !top.AbortExecution && !Top.Yielded) {
				instruction = method.Body [Top.InstructionPointer++];
				ExecuteInstruction ();
				top.Location = currLoc;
			}

			if (top.AbortExecution) {
				while (top.DisposableObjects.Count > 0) {
					top.DisposableObjects.Pop ().Exit (this);
				}
				return IodineNull.Instance;
			}

			IodineObject retVal = last ?? IodineNull.Instance;
			EndFrame ();

			while (top.DisposableObjects.Count > 0) {
				top.DisposableObjects.Pop ().Exit (this);
			}

			return retVal;
		}

		public void RaiseException (string message, params object[] args)
		{
			RaiseException (new IodineException (message, args));
		}

		public void RaiseException (IodineObject ex)
		{
			if (exceptionHandlers.Count == 0) {
				throw new UnhandledIodineExceptionException (Top, ex);
			} else {
				IodineExceptionHandler handler = exceptionHandlers.Pop ();
				ex.SetAttribute ("stackTrace", new IodineString (Trace ()));
				Unwind (frameCount - handler.Frame);
				lastException = ex;
				Top.InstructionPointer = handler.InstructionPointer;
			}
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private void ExecuteInstruction ()
		{
			currLoc = instruction.Location;
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
					Top.StoreLocal (instruction.Argument, Pop ());
					break;
				}
			case Opcode.LoadLocal:
				{
					Push (Top.LoadLocal (instruction.Argument));
					break;
				}
			case Opcode.StoreGlobal:
				{
					string name = ((IodineName)Top.Module.ConstantPool [instruction.Argument]).Value;
					if (globalDict.ContainsKey (name)) {
						globalDict [name] = Pop ();
					} else {
						Top.Module.SetAttribute (this, name, Pop ());
					}
					break;
				}
			case Opcode.LoadGlobal:
				{
					string name = ((IodineName)Top.Module.ConstantPool [instruction.Argument]).Value;
					if (Top.Module.Attributes.ContainsKey (name)) {
						Push (Top.Module.GetAttribute (this, name));
					} else if (globalDict.ContainsKey (name)) {
						Push (globalDict [name]);
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
			case Opcode.BinOp:
				{
					IodineObject op2 = Pop ();
					IodineObject op1 = Pop ();
					Push (op1.PerformBinaryOperation (this,
						(BinaryOperation)instruction.Argument,
						op2));
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
					IodineTypeDefinition target = (IodineTypeDefinition)Pop ();
					IodineObject[] arguments = new IodineObject[instruction.Argument];
					for (int i = 1; i <= instruction.Argument; i++) {
						arguments [instruction.Argument - i] = Pop ();
					}
					target.Inherit (this, Top.Self, arguments);
					break;
				}
			case Opcode.Return:
				{
					this.Top.InstructionPointer = int.MaxValue;
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
			case Opcode.BuildHash:
				{
					IodineHashMap hash = new IodineHashMap ();
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
					IodineMethod method = Pop () as IodineMethod;
					Push (new IodineClosure (Top, method));
					break;
				}
			case Opcode.IterGetNext:
				{
					Push (Pop ().IterGetNext (this));
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
					exceptionHandlers.Push (new IodineExceptionHandler (frameCount, instruction.Argument));
					break;
				}
			case Opcode.PopExceptionHandler:
				{
					exceptionHandlers.Pop ();
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
			case Opcode.Import:
				{
					string name = ((IodineName)Top.Module.ConstantPool [instruction.Argument]).Value;
					string fullPath = Path.GetFullPath (name);
					if (ModuleCache.ContainsKey (fullPath)) {
						IodineModule module = ModuleCache [fullPath];
						Top.Module.SetAttribute (this, Path.GetFileNameWithoutExtension (fullPath),
							module);
					} else {
						ErrorLog errLog = new ErrorLog ();
						IodineModule module = IodineModule.CompileModule (errLog, name);
						if (errLog.ErrorCount == 0 && module != null) {
							Top.Module.SetAttribute (this, Path.GetFileNameWithoutExtension (
								fullPath), module);
							ModuleCache [fullPath] = module;
							module.Initializer.Invoke (this, new IodineObject[] { });
						} else {
							throw new SyntaxException (errLog);
						}
					}
					break;
				}
			case Opcode.ImportFrom:
				{
					IodineTuple names = Pop () as IodineTuple;
					string name = ((IodineName)Top.Module.ConstantPool [instruction.Argument]).Value;
					string fullPath = Path.GetFullPath (name);
					IodineModule module = null;
					if (ModuleCache.ContainsKey (fullPath)) {
						module = ModuleCache [fullPath];
					} else {
						ErrorLog errLog = new ErrorLog ();
						module = IodineModule.LoadModule (errLog, name);
					
						if (module == null) {
							throw new SyntaxException (errLog);
						}
						ModuleCache [fullPath] = module;
						module.Initializer.Invoke (this, new IodineObject[] { });
					}
					foreach (IodineObject item in names.Objects) {
						this.Top.Module.SetAttribute (this, item.ToString (),
							module.GetAttribute (item.ToString ()));
					}
					break;
				}
			case Opcode.ImportAll:
				{
					string name = ((IodineName)Top.Module.ConstantPool [instruction.Argument]).Value;
					string fullPath = Path.GetFullPath (name);
					IodineModule module = null;
					if (ModuleCache.ContainsKey (fullPath)) {
						module = ModuleCache [fullPath];
					} else {
						ErrorLog errLog = new ErrorLog ();
						module = IodineModule.LoadModule (errLog, name);
						ModuleCache [fullPath] = module;
						module.Initializer.Invoke (this, new IodineObject[] { });
					}
					if (module != null) {
						foreach (string item in module.Attributes.Keys) {
							this.Top.Module.SetAttribute (this, item, module.GetAttribute (
								item));
						}
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
			}

		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private void Push (IodineObject obj)
		{
			last = obj;
			Top.Push (obj);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private IodineObject Pop ()
		{
			return Top.Pop ();
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private void NewFrame (StackFrame frame)
		{
			frameCount++;
			Top = frame;
			frames.Push (frame);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private void NewFrame (IodineMethod method, IodineObject self, int localCount)
		{
			frameCount++;
			Top = new StackFrame (method, Top, self, localCount);
			frames.Push (Top);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private void EndFrame ()
		{
			frameCount--;
			frames.Pop ();
			if (frames.Count != 0) {
				Top = frames.Peek ();
			} else {
				Top = null;
			}
		}

	}
}

