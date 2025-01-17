﻿using SkiaSharp;
using ZED.Common;
using ZED.GUI;
using ZED.Input;
using ZED.Objects;

namespace ZED.Scenes
{
    internal class ControllerAssignment : Scene
    {
        private Text _headerText;
        private Text _instructionText;
        private Text _finishText;
        private StarField _starField;

        private SKBitmap _controllerImage = null; //SKImage.FromEncodedData(""); //Properties.Resources.Controller.ToSKBitmap(); TODO

        private object _assignmentLock = new object();

        protected override void OnButtonDown(object sender, ButtonEventArgs e)
        {
            lock (_assignmentLock)
            {
                if (e.Button == Button.Select)
                {
                    Close();
                    return;
                }

                if (sender is InputDevice inputDevice)
                {
                    PlayerID nextID = InputManager.Instance.GetNextUnassignedPlayerID();

                    if (nextID != PlayerID.None)
                    {
                        if (InputManager.Instance.GetPlayerIDFromDevice(inputDevice) == PlayerID.None)
                        {
                            InputManager.Instance.SetInputDeviceForPlayer(nextID, inputDevice);
                        }
                    }
                }
            }
        }

        protected override void Setup()
        {
            _starField = new StarField(Display);
            _headerText = new Text(0, 10, "- assign controllers -", SKColors.White, Common.Fonts.FiveBySeven);
            _instructionText = new Text(0, 20, "press any button to join", SKColors.White, Common.Fonts.FiveByEight);
            _finishText = new Text(0, 30, "press select to finish", SKColors.White, Common.Fonts.FiveByEight);

            for (PlayerID id = PlayerID.One; id < (PlayerID)InputManager.Instance.MaxPlayers; id++)
            {
                InputManager.Instance.SetInputDeviceForPlayer(id, null);
            }
        }

        protected override void PrimaryExecutionMethod()
        {
            Display.Clear();

            _starField.Draw(LastFrameMS);

            _headerText.TextColor = ColorExtensions.ColorFromHSV(180 + FrameCount / 10);
            _instructionText.TextColor = ColorExtensions.ColorFromHSV(FrameCount / 10);
            _finishText.TextColor = ColorExtensions.ColorFromHSV(FrameCount / 10);

            _headerText.Draw(Display, true);
            _instructionText.Draw(Display, true);
            _finishText.Draw(Display, true);

            int numPlayers = 0;

            for (PlayerID id = PlayerID.One; id < (PlayerID)InputManager.Instance.MaxPlayers; id++)
            {
                if (InputManager.Instance.GetDeviceFromPlayerID(id) != null)
                {
                    numPlayers++;
                }
            }

            int controllersWidth = (_controllerImage.Width + 2) * numPlayers;

            int curX = (Display.Width / 2) - (controllersWidth / 2);
            for (int i = 0; i < numPlayers; i++)
            {
                Display.DrawImage(curX, Display.Height - (_controllerImage.Height + 2), _controllerImage);
                curX += _controllerImage.Width + 2;
            }
        }
    }
}
