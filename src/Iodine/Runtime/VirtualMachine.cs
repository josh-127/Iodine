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
using System.Collections.Generic;
using Iodine.Compiler;

namespace Iodine.Runtime
{
	public class VirtualMachine
	{
		public static readonly Dictionary<string, IodineModule> ModuleCache = new Dictionary<string, IodineModule> ();

		private Dictionary<string, IodineObject> globalDict = new Dictionary<string, IodineObject> ();
		private Stack<IodineExceptionHandler> exceptionHandlers = new Stack<IodineExceptionHandler> ();
		private IodineException lastException = null;
		private Location currLoc;

		public IodineStack Stack {
			private set;
			get;
		}

		public Dictionary <string, IodineObject> Globals {
			get {
				return globalDict;
			}
		}

		public VirtualMachine ()
		{
			Stack = new IodineStack ();
			var modules = BuiltInModules.Modules.Values.Where (p => p.ExistsInGlobalNamespace);
			foreach (IodineModule module in modules) {
				foreach (KeyValuePair<string, IodineObject> val in module.Attributes) {
					Globals [val.Key] = val.Value;
				}
			}
		}

		public VirtualMachine (Dictionary<string, IodineObject> globals)
		{
			Stack = new IodineStack ();
			globalDict = globals;
		}

		public IodineObject InvokeMethod (IodineMethod method, IodineObject self, IodineObject[] arguments)
		{
			if ((method.Variadic && arguments.Length + 1 < method.ParameterCount) ||
			    (!method.Variadic && arguments.Length < method.ParameterCount)) {
				RaiseException (new IodineArgumentException (method.ParameterCount));
				return null;
			}

			Stack.NewFrame (method, self, method.LocalCount);

			return Invoke (method, arguments);
		}

		public IodineObject InvokeMethod (IodineMethod method, StackFrame frame, IodineObject self,
		                                  IodineObject[] arguments)
		{

			if ((method.Variadic && arguments.Length + 1 < method.ParameterCount) ||
			    (!method.Variadic && arguments.Length < method.ParameterCount)) {
				RaiseException (new IodineArgumentException (method.ParameterCount));
				return null;
			}

			Stack.NewFrame (frame);
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
				if (i == method.Parameters.Keys.Count - 1 && method.Variadic) {
					IodineObject[] tupleItems = new IodineObject[arguments.Length - i];
					Array.Copy (arguments, i, tupleItems, 0, arguments.Length - i);
					Stack.StoreLocal (method.Parameters [param], new IodineTuple (tupleItems));
				} else {
					Stack.StoreLocal (method.Parameters [param], arguments [i++]);
				}
			}

			StackFrame top = Stack.Top;
			while (top.InstructionPointer < insCount && !top.AbortExecution && !Stack.Top.Yielded) {
				Instruction currInstruction = method.Body [Stack.Top.InstructionPointer++];
				ExecuteInstruction (currInstruction);
				top.Location = currLoc;
			}

			if (top.AbortExecution) {
				return IodineNull.Instance;
			}

			IodineObject retVal = Stack.Last == null ? IodineNull.Instance : Stack.Last;
			Stack.EndFrame ();
			return retVal;
		}

		public void RaiseException (string message, params object[] args)
		{
			RaiseException (new IodineException (message, args));
		}

		public void RaiseException (IodineException ex)
		{
			if (exceptionHandlers.Count == 0) {
				throw new UnhandledIodineExceptionException (Stack.Top, ex);
			} else {
				IodineExceptionHandler handler = exceptionHandlers.Pop ();
				ex.SetAttribute ("stackTrace", new IodineString (Stack.Trace ()));
				Stack.Unwind (Stack.Frames - handler.Frame);
				lastException = ex;
				Stack.Top.InstructionPointer = handler.InstructionPointer;
			}
		}

		private void ExecuteInstruction (Instruction ins)
		{
			currLoc = ins.Location;
			switch (ins.OperationCode) {
			case Opcode.Pop:
				{
					Stack.Pop ();
					break;
				}
			case Opcode.Dup:
				{
					IodineObject val = Stack.Pop ();
					Stack.Push (val);
					Stack.Push (val);
					break;
				}
			case Opcode.LoadConst:
				{
					Stack.Push (Stack.Top.Module.ConstantPool [ins.Argument]);
					break;
				}
			case Opcode.LoadNull:
				{
					Stack.Push (IodineNull.Instance);
					break;
				}
			case Opcode.LoadSelf:
				{
					Stack.Push (Stack.Top.Self);
					break;
				}
			case Opcode.LoadTrue:
				{
					Stack.Push (IodineBool.True);
					break;
				}
			case Opcode.LoadException:
				{
					Stack.Push (lastException);
					break;
				}
			case Opcode.LoadFalse:
				{
					Stack.Push (IodineBool.False);
					break;
				}
			case Opcode.StoreLocal:
				{
					Stack.StoreLocal (ins.Argument, Stack.Pop ());
					break;
				}
			case Opcode.LoadLocal:
				{
					Stack.Push (Stack.LoadLocal (ins.Argument));
					break;
				}
			case Opcode.StoreGlobal:
				{
					string name = ((IodineName)Stack.Top.Module.ConstantPool [ins.Argument]).Value;
					if (globalDict.ContainsKey (name)) {
						globalDict [name] = Stack.Pop ();
					} else {
						Stack.Top.Module.SetAttribute (this, name, Stack.Pop ());
					}
					break;
				}
			case Opcode.LoadGlobal:
				{
					string name = ((IodineName)Stack.Top.Module.ConstantPool [ins.Argument]).Value;
					if (globalDict.ContainsKey (name)) {
						Stack.Push (globalDict [name]);
					} else {
						Stack.Push (Stack.Top.Module.GetAttribute (this, name));
					}
					break;
				}
			case Opcode.StoreAttribute:
				{
					IodineObject target = Stack.Pop ();
					IodineObject value = Stack.Pop ();
					string attribute = ((IodineName)Stack.Top.Module.ConstantPool [ins.Argument]).Value;
					target.SetAttribute (this, attribute, value);
					break;
				}
			case Opcode.LoadAttribute:
				{
					IodineObject target = Stack.Pop ();
					string attribute = ((IodineName)Stack.Top.Module.ConstantPool [ins.Argument]).Value;
					Stack.Push (target.GetAttribute (this, attribute));
					break;
				}
			case Opcode.StoreIndex:
				{
					IodineObject index = Stack.Pop ();
					IodineObject target = Stack.Pop ();
					IodineObject value = Stack.Pop ();
					target.SetIndex (this, index, value);
					break;
				}
			case Opcode.LoadIndex:
				{
					IodineObject index = Stack.Pop ();
					IodineObject target = Stack.Pop ();
					Stack.Push (target.GetIndex (this, index));
					break;
				}
			case Opcode.BinOp:
				{
					IodineObject op2 = Stack.Pop ();
					IodineObject op1 = Stack.Pop ();
					Stack.Push (op1.PerformBinaryOperation (this, (BinaryOperation)ins.Argument,
						op2));
					break;
				}
			case Opcode.UnaryOp:
				{
					Stack.Push (Stack.Pop ().PerformUnaryOperation (this, 
						(UnaryOperation)ins.Argument));
					break;
				}
			case Opcode.Invoke:
				{
					IodineObject target = Stack.Pop ();
					IodineObject[] arguments = new IodineObject[ins.Argument];
					for (int i = 1; i <= ins.Argument; i++) {
						arguments [ins.Argument - i] = Stack.Pop ();
					}
					Stack.Push (target.Invoke (this, arguments));
					break;
				}
			case Opcode.InvokeVar:
				{
					IodineObject target = Stack.Pop ();
					List<IodineObject> arguments = new List<IodineObject> ();
					IodineTuple tuple = Stack.Pop () as IodineTuple;
					if (tuple == null) {
						RaiseException (new IodineTypeException ("Tuple"));
						break;
					}
					for (int i = 0; i < ins.Argument; i++) {
						arguments.Add (Stack.Pop ());
					}
					arguments.AddRange (tuple.Objects);
					Stack.Push (target.Invoke (this, arguments.ToArray ()));
					break;
				}
			case Opcode.InvokeSuper:
				{
					IodineTypeDefinition target = (IodineTypeDefinition)Stack.Pop ();
					IodineObject[] arguments = new IodineObject[ins.Argument];
					for (int i = 1; i <= ins.Argument; i++) {
						arguments [ins.Argument - i] = Stack.Pop ();
					}
					target.Inherit (this, Stack.Top.Self, arguments);
					break;
				}
			case Opcode.Return:
				{
					this.Stack.Top.InstructionPointer = int.MaxValue;
					break;
				}
			case Opcode.Yield:
				{
					Stack.Top.Yielded = true;
					break;
				}
			case Opcode.JumpIfTrue:
				{
					if (Stack.Pop ().IsTrue ()) {
						Stack.Top.InstructionPointer = ins.Argument;
					}
					break;
				}
			case Opcode.JumpIfFalse:
				{
					if (!Stack.Pop ().IsTrue ()) {
						Stack.Top.InstructionPointer = ins.Argument;
					}
					break;
				}
			case Opcode.Jump:
				{
					Stack.Top.InstructionPointer = ins.Argument;
					break;
				}
			case Opcode.BuildList:
				{
					IodineObject[] items = new IodineObject[ins.Argument];
					for (int i = 1; i <= ins.Argument; i++) {
						items [ins.Argument - i] = Stack.Pop ();
					}
					Stack.Push (new IodineList (items));
					break;
				}
			case Opcode.BuildTuple:
				{
					IodineObject[] items = new IodineObject[ins.Argument];
					for (int i = 1; i <= ins.Argument; i++) {
						items [ins.Argument - i] = Stack.Pop ();
					}
					Stack.Push (new IodineTuple (items));
					break;
				}
			case Opcode.BuildClosure:
				{
					IodineMethod method = Stack.Pop () as IodineMethod;
					Stack.Push (new IodineClosure (Stack.Top, method));
					break;
				}
			case Opcode.IterGetNext:
				{
					Stack.Push (Stack.Pop ().IterGetNext (this));
					break;
				}
			case Opcode.IterMoveNext:
				{
					Stack.Push (new IodineBool (Stack.Pop ().IterMoveNext (this)));
					break;
				}
			case Opcode.IterReset:
				{
					Stack.Pop ().IterReset (this);
					break;
				}
			case Opcode.PushExceptionHandler:
				{
					exceptionHandlers.Push (new IodineExceptionHandler (Stack.Frames, ins.Argument));
					break;
				}
			case Opcode.PopExceptionHandler:
				{
					exceptionHandlers.Pop ();
					break;
				}
			case Opcode.InstanceOf:
				{
					IodineObject o = Stack.Pop ();
					IodineTypeDefinition type = Stack.Pop () as IodineTypeDefinition;
					if (type == null) {
						RaiseException (new IodineTypeException ("TypeDef"));
						break;
					}
					Stack.Push (new IodineBool (o.InstanceOf (type)));
					break;
				}
			case Opcode.BeginExcept:
				{
					bool rethrow = true;
					for (int i = 1; i <= ins.Argument; i++) {
						IodineTypeDefinition type = Stack.Pop () as IodineTypeDefinition;
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
					IodineException e = Stack.Pop () as IodineException;
					if (e != null) {
						RaiseException (e);
					} else {
						RaiseException (new IodineTypeException ("Exception"));
					}
					break;
				}
			case Opcode.Import:
				{
					string name = ((IodineName)Stack.Top.Module.ConstantPool [ins.Argument]).Value;
					string fullPath = Path.GetFullPath (name);
					if (ModuleCache.ContainsKey (fullPath)) {
						IodineModule module = ModuleCache [fullPath];
						Stack.Top.Module.SetAttribute (this, Path.GetFileNameWithoutExtension (fullPath),
							module);
					} else {
						ErrorLog errLog = new ErrorLog ();
						IodineModule module = IodineModule.CompileModule (errLog, name);
						if (errLog.ErrorCount == 0 && module != null) {
							Stack.Top.Module.SetAttribute (this, Path.GetFileNameWithoutExtension (
								fullPath), module);
							ModuleCache [fullPath] = module;
							module.Initializer.Invoke (this, new IodineObject[] { });
						}
					}
					break;
				}
			case Opcode.ImportFrom:
				{
					IodineTuple names = Stack.Pop () as IodineTuple;
					string name = ((IodineName)Stack.Top.Module.ConstantPool [ins.Argument]).Value;
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
						foreach (IodineObject item in names.Objects) {
							this.Stack.Top.Module.SetAttribute (this, item.ToString (),
								module.GetAttribute (item.ToString ()));
						}
					}
					break;
				}
			case Opcode.ImportAll:
				{
					string name = ((IodineName)Stack.Top.Module.ConstantPool [ins.Argument]).Value;
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
							this.Stack.Top.Module.SetAttribute (this, item, module.GetAttribute (
								item));
						}
					}
					break;
				}
			case Opcode.SwitchLookup:
				{
					Dictionary<int, IodineObject> lookup = new Dictionary<int, IodineObject> ();
					int needle = Stack.Pop ().GetHashCode ();
					for (int i = 0; i < ins.Argument; i++) {
						IodineObject value = Stack.Pop ();
						IodineObject key = Stack.Pop ();
						lookup [key.GetHashCode ()] = value;
					}
					if (lookup.ContainsKey (needle)) {
						lookup [needle].Invoke (this, new IodineObject[] { });
						Stack.Push (IodineBool.True);
					} else {
						Stack.Push (IodineBool.False);
					}
					break;
				}
			}

		}
	}
}

