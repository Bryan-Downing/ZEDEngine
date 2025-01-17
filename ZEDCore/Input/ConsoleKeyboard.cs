﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ZED.Input
{
    internal class ConsoleKeyboard : InputDevice, IDisposable
    {
        private Dictionary<ConsoleKey, short> _keyAxisValues;

        private readonly CancellationTokenSource _cancellationTokenSource;

        public ConsoleKeyboard() : base("console")
        {
            _cancellationTokenSource = new CancellationTokenSource();

            AxisBindings = new Dictionary<object, Axis>()
            {
                { ConsoleKey.UpArrow, Axis.Vertical },
                { ConsoleKey.DownArrow, Axis.Vertical },
                { ConsoleKey.LeftArrow, Axis.Horizontal },
                { ConsoleKey.RightArrow, Axis.Horizontal },
                { ConsoleKey.W, Axis.Vertical },
                { ConsoleKey.S, Axis.Vertical },
                { ConsoleKey.A, Axis.Horizontal },
                { ConsoleKey.D, Axis.Horizontal },
            };

            _keyAxisValues = new Dictionary<ConsoleKey, short>()
            {
                { ConsoleKey.UpArrow, -short.MaxValue },
                { ConsoleKey.DownArrow, short.MaxValue },
                { ConsoleKey.LeftArrow, -short.MaxValue },
                { ConsoleKey.RightArrow, short.MaxValue },
                { ConsoleKey.W, -short.MaxValue },
                { ConsoleKey.S, short.MaxValue },
                { ConsoleKey.A, -short.MaxValue },
                { ConsoleKey.D, short.MaxValue },
            };

            ButtonBindings = new Dictionary<object, Button>()
            {
                { ConsoleKey.Spacebar, Button.A },
                { ConsoleKey.Enter, Button.A },
                { ConsoleKey.B, Button.B },
                { ConsoleKey.X, Button.X },
                { ConsoleKey.Y, Button.Y },
                { ConsoleKey.Escape, Button.Start },
                { ConsoleKey.Tab, Button.Select }
            };
        }

        public override void ProcessMessages()
        {
            try
            {
                if (!Console.KeyAvailable)
                {
                    return;
                }

                var input = Console.ReadKey(true);

                if (input.Key == ConsoleKey.Q && input.Modifiers.HasFlag(ConsoleModifiers.Shift))
                {
                    ZEDProgram.Instance.Logger.Log($"!!!!! User pressed Shift+Q - application closing. !!!!!");
                    ZEDProgram.Instance.Close();
                }
                else if (ButtonBindings.TryGetValue(input.Key, out Button button))
                {
                    // TODO: Un-press this button...
                    // NOTE: I believe detecting a press/un-press would require accessing /dev/input, which I don't wanna do. Also wouldn't work for ssh.
                    OnButtonChanged(this, new ButtonEventArgs() { Button = button, IsPressed = true });
                }
                else if (AxisBindings.TryGetValue(input.Key, out Axis axis) && _keyAxisValues.TryGetValue(input.Key, out short value))
                {
                    // TODO: Un-press this axis...
                    OnAxisChanged(this, new AxisEventArgs() { Axis = axis, Value = value });
                }

                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
                ZEDProgram.Instance.Logger.Log($"Caught exception: {ex}");
            }
        }

        public override void Dispose()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
