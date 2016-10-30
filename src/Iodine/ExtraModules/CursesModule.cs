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

#if COMPILE_EXTRAS

using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Net.Security;
using System.Collections.Generic;
using Mono.Posix;
using System.Security.Cryptography.X509Certificates;
using Iodine.Runtime;

namespace Iodine.Modules.Extras
{
    [IodineBuiltinModule ("curses")]
    internal class CursesModule : IodineModule
    {
        enum TerminalAttributes
        {
            FOREGROUND_BLACK = 0x01,
            FOREGROUND_RED = 0x02,
            FOREGROUND_GREEN = 0x03,
            FOREGROUND_YELLOW = 0x04,
            FOREGROUND_BLUE = 0x05,
            FOREGROUND_MAGENTA = 0x06,
            FOREGROUND_CYAN = 0x07,
            FOREGROUND_WHITE = 0x08,
            BACKGROUND_BLACK = 0x10,
            BACKGROUND_RED = 0x20,
            BACKGROUND_GREEN = 0x30,
            BACKGROUND_YELLOW = 0x40,
            BACKGROUND_BLUE = 0x50,
            BACKGROUND_MAGENTA = 0x60,
            BACKGROUND_CYAN = 0x70,
            BACKGROUND_WHITE = 0x80

        }

        class AttributeWrapper : IodineObject 
        {
            public readonly TerminalAttributes Value;

            public AttributeWrapper (TerminalAttributes value)
                : base (new IodineTypeDefinition ("TerminalAttribute"))
            {
                Value = value;
            }
        }

        static TerminalAttributes[] Palette = new TerminalAttributes[32];

        public CursesModule ()
            : base ("curses")
        {
            SetAttribute ("COLOR_BLACK", new IodineInteger (1));
            SetAttribute ("COLOR_RED", new IodineInteger (2));
            SetAttribute ("COLOR_GREEN", new IodineInteger (3));
            SetAttribute ("COLOR_YELLOW", new IodineInteger (4));
            SetAttribute ("COLOR_BLUE", new IodineInteger (5));
            SetAttribute ("COLOR_MAGENTA", new IodineInteger (6));
            SetAttribute ("COLOR_CYAN", new IodineInteger (7));
            SetAttribute ("COLOR_WHITE", new IodineInteger (8));
            SetAttribute ("COLOR_PAIR", new BuiltinMethodCallback (ColorPair, null));

            SetAttribute ("attron", new BuiltinMethodCallback (AttributeOn, null));
            SetAttribute ("attroff", new BuiltinMethodCallback (AttributeOff, null));
            SetAttribute ("move", new BuiltinMethodCallback (Move, null));
            SetAttribute ("mvprint", new BuiltinMethodCallback (Mvprint, null));
            SetAttribute ("echo", new BuiltinMethodCallback (EchoOn, null));
            SetAttribute ("noecho", new BuiltinMethodCallback (EchoOff, null));
            SetAttribute ("init_pair", new BuiltinMethodCallback (InitPair, null));
        }

        private static ConsoleColor AttributeToConsoleColor (int num)
        {
            switch (num) {
            case 0x01:
                return ConsoleColor.Black;
            case 0x02:
                return ConsoleColor.Red;
            case 0x03:
                return ConsoleColor.Green;
            case 0x04:
                return ConsoleColor.Yellow;
            case 0x05:
                return ConsoleColor.Blue;
            case 0x06:
                return ConsoleColor.Magenta;
            case 0x07:
                return ConsoleColor.Cyan;
            case 0x08:
                return ConsoleColor.Cyan;
            default:
                return ConsoleColor.Black;
            }
        }

        private static IodineObject ColorPair (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineInteger index = args [0] as IodineInteger;

            return new AttributeWrapper (Palette [(int)index.Value]);
        }

        private static IodineObject InitPair (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 3) {
                vm.RaiseException (new IodineArgumentException (3));
                return null;
            }

            IodineInteger index = args [0] as IodineInteger;
            IodineInteger fg = args [1] as IodineInteger;
            IodineInteger bg = args [2] as IodineInteger;

            TerminalAttributes attr = (TerminalAttributes)((int)fg.Value | ((int)bg.Value << 4));

            Palette [(int)index.Value] = attr;

            return null;
        }

        private static IodineObject Move (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 2) {
                vm.RaiseException (new IodineArgumentException (2));
                return null;
            }

            IodineInteger yPos = args [0] as IodineInteger;
            IodineInteger xPos = args [1] as IodineInteger;

            Console.CursorLeft = (int)xPos.Value;
            Console.CursorTop = (int)yPos.Value;
            return null;
        }

        private static IodineObject Mvprint (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length < 3) {
                vm.RaiseException (new IodineArgumentException (3));
                return null;
            }

            IodineInteger yPos = args [0] as IodineInteger;
            IodineInteger xPos = args [1] as IodineInteger;

            string message = ((IodineString)args [2]).Value;
            Console.CursorLeft = (int)xPos.Value;
            Console.CursorTop = (int)yPos.Value;
            Console.Write (message);
            Console.Out.Flush ();
            return null;
        }

        private static IodineObject AttributeOn (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            AttributeWrapper attrWrapper = args [0] as AttributeWrapper;

            TerminalAttributes attr = attrWrapper.Value;

            int fg = (int)attr & 0x0F;
            int bg = ((int)attr & 0xF0) >> 4;

            if (fg != 0) {
                Console.ForegroundColor = AttributeToConsoleColor (fg);
            }

            if (bg != 0) {
                Console.BackgroundColor = AttributeToConsoleColor (bg);
            }

            return null;
        }

        private static IodineObject AttributeOff (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            return null;
        }

        private static IodineObject EchoOn (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            Console.Write ("\x1B[22l");
            return null;
        }

        private static IodineObject EchoOff (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            Console.Write ("\x1B[22h");
            return null;
        }
    }
}


#endif
