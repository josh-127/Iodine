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
using Iodine.Runtime;

namespace Iodine.Modules.Extras
{
    /*
     * Note: This is NOT intended to be a full implementation of the curses API, rather
     * a minimalistic API modelled after curses
     */
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

        abstract class TerminalAction
        {
            public abstract void Visit (Terminal terminal);
        }

        class MoveCursorAction : TerminalAction
        {
            public readonly int X;
            public readonly int Y;

            public MoveCursorAction (int x, int y)
            {
                X = x;
                Y = y;
            }

            public override void Visit (Terminal terminal)
            {
                terminal.AcceptAction (this);
            }
        }

        class PrintStringAction : TerminalAction
        {
            public readonly string Message;

            public PrintStringAction (string message)
            {
                Message = message;
            }

            public override void Visit (Terminal terminal)
            {
                terminal.AcceptAction (this);
            }
        }

        abstract class Terminal
        {
            private TerminalAttributes activeAttributes = TerminalAttributes.BACKGROUND_BLACK | TerminalAttributes.FOREGROUND_WHITE;
            private Dictionary<TerminalAttributes, TerminalAttributes> previousAttributes = new Dictionary<TerminalAttributes, TerminalAttributes> ();

            protected TerminalAttributes[] palette = new TerminalAttributes[16];
            protected Queue<TerminalAction> actionQueue = new Queue<TerminalAction> ();

            public abstract void Init ();
            public abstract void Destroy ();
            public abstract void Clear ();
            public abstract void Echo ();
            public abstract void NoEcho ();
            public abstract int GetChar ();
            public abstract void Refresh ();
            public abstract void CurseSet (int amount);
            public abstract void Move (int y, int x);
            public abstract void Print (string message);
            public abstract void SetTerminalAttributes (TerminalAttributes attributes);

            public abstract void AcceptAction (MoveCursorAction action);
            public abstract void AcceptAction (PrintStringAction action);

            public TerminalAttributes ColorPair (int index)
            {
                return palette [index & 0x0F];
            }

            public void InitPair (int index, int forecolor, int backcolor)
            {
                TerminalAttributes attr = (TerminalAttributes)(forecolor | (backcolor << 4));

                palette [index & 0x0F] = attr;
            }


            public void AttributesOn (TerminalAttributes attributes)
            {
                previousAttributes [attributes] = activeAttributes;
                SetTerminalAttributes (attributes);
            }

            public void AttributesOff (TerminalAttributes attributes)
            {
                SetTerminalAttributes (previousAttributes [activeAttributes]);
            }
        }

        class UnixTerminal : Terminal
        {
            private bool echo = true;

            public override void Init ()
            {
                Console.Write ("\x1B[?47h");
                Console.Out.Flush ();
                Clear ();
            }

            public override void Destroy ()
            {
                Console.Write ("\x1B[?47l");
                Console.Out.Flush ();
            }

            public override void CurseSet (int amount)
            {
                switch (amount) {
                case 0x00:
                    Console.Write ("\x1B[?25l");
                    break;
                default:
                    Console.Write ("\x1B[?25h");
                    break;
                }
            }

            public override void Clear ()
            {
                Console.Write ("\x1B" + "c");
                Console.CursorTop = 0;
                Console.CursorLeft = 0;
                Console.Out.Flush ();
            }

            public override void Echo ()
            {
                echo = true;
                Console.Write ("\x1B[22l");
            }

            public override void NoEcho ()
            {
                echo = false;
                Console.Write ("\x1B[22h");
            }

            public override int GetChar ()
            {
                ConsoleKeyInfo info = Console.ReadKey (!echo);

                if (info.KeyChar == '\0') {
                    switch (info.Key) {
                    case ConsoleKey.LeftArrow:
                        return 260;
                    case ConsoleKey.RightArrow:
                        return 261;
                    case ConsoleKey.UpArrow:
                        return 259;
                    case ConsoleKey.DownArrow:
                        return 258;
                    }
                }

                return (int)info.KeyChar;
            }

            public override void Refresh ()
            {
                foreach (TerminalAction action in actionQueue) {
                    action.Visit (this);
                }
                actionQueue.Clear ();
            }

            public override void Move (int y, int x)
            {
                actionQueue.Enqueue (new MoveCursorAction (x, y));
            }

            public override void Print (string message)
            {
                actionQueue.Enqueue (new PrintStringAction (message));
            }

            public override void SetTerminalAttributes (TerminalAttributes attributes)
            {
                int fg = (int)attributes & 0x0F;
                int bg = ((int)attributes & 0xF0) >> 4;

                if (fg != 0) {
                    Console.ForegroundColor = AttributeToConsoleColor (fg);
                }

                if (bg != 0) {
                    Console.BackgroundColor = AttributeToConsoleColor (bg);
                }
            }

            public override void AcceptAction (MoveCursorAction action)
            {
                Console.CursorLeft = action.X;
                Console.CursorTop = action.Y;
            }

            public override void AcceptAction (PrintStringAction action)
            {
                Console.Write (action.Message);
            }
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

        static Terminal activeTerminal = new UnixTerminal ();

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

            SetAttribute ("initscr", new BuiltinMethodCallback (InitScreen, null));
            SetAttribute ("endwin", new BuiltinMethodCallback (EndWin, null));
            SetAttribute ("cur_set", new BuiltinMethodCallback (CurseSet, null));
            SetAttribute ("getch", new BuiltinMethodCallback (Getch, null));
            SetAttribute ("attron", new BuiltinMethodCallback (AttributeOn, null));
            SetAttribute ("attroff", new BuiltinMethodCallback (AttributeOff, null));
            SetAttribute ("move", new BuiltinMethodCallback (Move, null));
            SetAttribute ("print", new BuiltinMethodCallback (Print, null));
            SetAttribute ("mvprint", new BuiltinMethodCallback (Mvprint, null));
            SetAttribute ("echo", new BuiltinMethodCallback (EchoOn, null));
            SetAttribute ("noecho", new BuiltinMethodCallback (EchoOff, null));
            SetAttribute ("init_pair", new BuiltinMethodCallback (InitPair, null));
            SetAttribute ("refresh", new BuiltinMethodCallback (Refresh, null));
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
                return ConsoleColor.White;
            default:
                return ConsoleColor.Black;
            }
        }

        private static IodineObject InitScreen (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            activeTerminal.Init ();
            return null;
        }

        private static IodineObject EndWin (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            activeTerminal.Destroy ();
            return null;
        }

        private static IodineObject CurseSet (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineInteger index = args [0] as IodineInteger;

            activeTerminal.CurseSet ((int)index.Value);

            return null;
        }

        private static IodineObject Getch (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            return new IodineInteger (activeTerminal.GetChar ());
        }

        private static IodineObject ColorPair (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            IodineInteger index = args [0] as IodineInteger;

            return new AttributeWrapper (activeTerminal.ColorPair ((int)index.Value));
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

            activeTerminal.InitPair ((int)index.Value, (int)fg.Value, (int)bg.Value);

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

            activeTerminal.Move ((int)yPos.Value, (int)xPos.Value);

            return null;
        }

        private static IodineObject Print (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            string message = args [0].ToString ();

            activeTerminal.Print (message);

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

            string message = args [2].ToString ();

            activeTerminal.Move ((int)yPos.Value, (int)xPos.Value);
            activeTerminal.Print (message);

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

            activeTerminal.AttributesOn (attr);


            return null;
        }

        private static IodineObject AttributeOff (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            if (args.Length == 0) {
                vm.RaiseException (new IodineArgumentException (1));
                return null;
            }

            AttributeWrapper attrWrapper = args [0] as AttributeWrapper;

            TerminalAttributes attr = attrWrapper.Value;

            activeTerminal.AttributesOff (attr);

            return null;
        }

        private static IodineObject EchoOn (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            activeTerminal.Echo ();
            return null;
        }

        private static IodineObject EchoOff (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            activeTerminal.NoEcho ();
            return null;
        }

        private static IodineObject Refresh (VirtualMachine vm, IodineObject self, IodineObject[] args)
        {
            activeTerminal.Refresh ();
            return null;
        }
    }
}

#endif
