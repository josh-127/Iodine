using System;

namespace Iodine
{
	public enum Opcode
	{
		Print,
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
		Return,
		JumpIfTrue,
		JumpIfFalse,
		Jump,
		BuildList,
		BuildClosure,
		IterGetNext,
		IterMoveNext,
		IterReset,
		PushExceptionHandler,
		PopExceptionHandler,
		LoadException
	}
}

