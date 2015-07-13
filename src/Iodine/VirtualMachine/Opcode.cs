using System;

namespace Iodine
{
	public enum Opcode
	{
		Nop,
		BinOp,
		UnaryOp,
		Pop,
		Dup,
		Dup3,
		LoadConst,
		LoadNull,
		LoadSelf,
		LoadTrue,
		LoadFalse,
		LoadLocal,
		StoreLocal,
		LoadGlobal,
		StoreGlobal,
		LoadAttribute,
		StoreAttribute,
		LoadIndex,
		StoreIndex,
		Invoke,
		InvokeSuper,
		Return,
		JumpIfTrue,
		JumpIfFalse,
		Jump,
		BuildList,
		BuildTuple,
		BuildClosure,
		IterGetNext,
		IterMoveNext,
		IterReset,
		Raise,
		PushExceptionHandler,
		PopExceptionHandler,
		LoadException,
		BeginExcept,
		InstanceOf,
		Import,
		ImportFrom,
		ImportAll
	}
}

