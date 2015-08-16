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
using Iodine.Compiler.Ast;

namespace Iodine.Compiler
{
	public interface IAstVisitor
	{
		void Accept (AstRoot ast);
		void Accept (NodeExpr expr);
		void Accept (NodeStmt stmt);
		void Accept (NodeBinOp binop);
		void Accept (NodeUnaryOp unaryop);
		void Accept (NodeIdent ident);
		void Accept (NodeCall call);
		void Accept (NodeArgList arglist);
		void Accept (NodeGetAttr getAttr);
		void Accept (NodeInteger integer);
		void Accept (NodeIfStmt ifStmt);
		void Accept (NodeWhileStmt whileStmt);
		void Accept (NodeForStmt forStmt);
		void Accept (NodeForeach foreachStmt);
		void Accept (NodeFuncDecl funcDecl);
		void Accept (NodeScope scope);
		void Accept (NodeString stringConst);
		void Accept (NodeUseStatement useStmt);
		void Accept (NodeInterfaceDecl interfaceDecl);
		void Accept (NodeClassDecl classDecl);
		void Accept (NodeReturnStmt returnStmt);
		void Accept (NodeIndexer indexer);
		void Accept (NodeList list);
		void Accept (NodeSelf self);
		void Accept (NodeTrue ntrue);
		void Accept (NodeFalse nfalse);
		void Accept (NodeNull nil);
		void Accept (NodeLambda lambda);
		void Accept (NodeTryExcept tryCatch);
		void Accept (NodeBreak brk);
		void Accept (NodeContinue cont);
		void Accept (NodeTuple tuple);
		void Accept (NodeFloat dec);
		void Accept (NodeSuperCall super);
		void Accept (NodeEnumDecl enumDecl);
		void Accept (NodeRaiseStmt raise);
	}
}

