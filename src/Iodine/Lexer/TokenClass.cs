using System;

namespace Iodine.Compiler
{
	public enum TokenClass
	{
		Identifier,
		StringLiteral,
		InterpolatedStringLiteral,
		IntLiteral,
		FloatLiteral,
		Keyword,
		OpenParan,
		CloseParan,
		OpenBrace,
		CloseBrace,
		OpenBracket,
		CloseBracket,
		SemiColon,
		Colon,
		Operator,
		Comma,
		Dot
	}
}

