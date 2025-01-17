﻿using SkiaSharp;
using System;
using System.Diagnostics;
using ZED.Common;
using ZED.Input;
using ZED.Interfaces;
using ZED.Scenes;

namespace ZED
{
    public abstract class Scene
    {
        public string Name;

        protected bool IsPausable = true;
        protected Scene NextScene = null;

        protected IDisplay Display;

        protected bool SceneClosing = false;
        protected bool IsPaused = false;

        protected long FrameCount = 0;
        protected long TotalElapsedMS
        {
            get { return _timeStopwatch?.ElapsedMilliseconds ?? 0; }
        }
        protected long TotalElapsedTicks
        {
            get { return _timeStopwatch?.ElapsedTicks ?? 0; }
        }

        protected long LastFrameMS = 0;

        private Stopwatch _frameStopwatch = new Stopwatch();
        private Stopwatch _timeStopwatch = new Stopwatch();

        private Scene _nestedSceneToRun = null;

        private SKBitmap _errorImage = null; // Properties.Resources.Error.ToSKBitmap(); TODO

        public Scene(string name = "Unknown Scene")
        {
            Name = name;
        }

        protected abstract void Setup();

        protected abstract void PrimaryExecutionMethod();

        protected virtual void OnButtonChanged(object sender, ButtonEventArgs e)
        {
            if (e.IsPressed)
            {
                OnButtonDown(sender, e);
            }
            else
            {
                OnButtonUp(sender, e);
            }
        }

        protected virtual void OnButtonDown(object sender, ButtonEventArgs e)
        {
            if (e.Button == Button.Start)
            {
                Pause();
            }
        }

        protected virtual void OnButtonUp(object sender, ButtonEventArgs e)
        {

        }

        protected virtual void OnAxisChanged(object sender, AxisEventArgs e)
        {

        }

        protected virtual void OnNestedSceneClosed()
        {

        }

        public void Run(IDisplay display)
        {
            lock (SceneManager.SceneChangingLock)
            {
                ZEDProgram.Instance.Logger.Log($"Running scene [{Name}]...");

                SceneManager.CurrentScene = this;

                SceneClosing = ZEDProgram.Instance.IsClosing;

                Display = display;

                InputManager.Instance.ButtonChanged += OnButtonChanged;
                InputManager.Instance.AxisChanged += OnAxisChanged;

                _frameStopwatch.Restart();
                _timeStopwatch.Restart();

                Setup();
            }

            while (!SceneClosing && !SceneManager.ClosingToMainMenu && !ZEDProgram.Instance.IsClosing)
            {
                PrimaryExecutionMethod();

                Draw();

                InputManager.Instance.ProcessInput();
            }

            Close();

            if (SceneManager.ClosingToMainMenu)
            {
                NextScene = new MainMenu();
            }

            if (NextScene != null)
            {
                NextScene.Run(Display);
            }
        }

        protected virtual void Draw()
        {
            _frameStopwatch.Stop();
            LastFrameMS = _frameStopwatch.ElapsedMilliseconds;
            _frameStopwatch.Restart();

            if (SceneManager.DisplayFPS)
            {
                DrawFPSCounter();
            }

            if (ZEDProgram.Instance.ErrorOccurred)
            {
                DrawErrorSymbol();
            }

            Display.Draw();

            int frameDelayMS = 0;

            if (SceneManager.LockFPS)
            {
                frameDelayMS = (1000 / SceneManager.FrameRate) - (int)_frameStopwatch.ElapsedMilliseconds;
            }

            if (IsPaused && IsPausable)
            {
                RunNestedScene(new OptionsMenu(this));
            }

            if (_nestedSceneToRun != null)
            {
                RunNestedSceneInternal(_nestedSceneToRun);
            }

            if (frameDelayMS > 0)
            {
                System.Threading.Thread.Sleep(frameDelayMS);
            }

            FrameCount++;
        }

        private object _pauseLock = new object();
        protected virtual void Pause()
        {
            lock (_pauseLock)
            {
                if (!IsPausable || IsPaused)
                {
                    return;
                }

                IsPaused = true;
            }
        }

        protected void RunNestedScene(Scene scene)
        {
            _nestedSceneToRun = scene;
        }

        private void RunNestedSceneInternal(Scene scene)
        {
            _nestedSceneToRun = null;

            InputManager.Instance.ButtonChanged -= OnButtonChanged;
            InputManager.Instance.AxisChanged -= OnAxisChanged;

            _frameStopwatch.Stop();
            _timeStopwatch.Stop();

            scene.Run(Display);

            OnNestedSceneClosed();

            SceneManager.CurrentScene = this;

            IsPaused = false;

            _frameStopwatch.Start();
            _timeStopwatch.Start();

            InputManager.Instance.ButtonChanged += OnButtonChanged;
            InputManager.Instance.AxisChanged += OnAxisChanged;
        }

        private void DrawFPSCounter()
        {
            long fps = (long)(FrameCount / Math.Max(_timeStopwatch.ElapsedMilliseconds / 1000.0, 1.0));

            Display.DrawRect(1, 1, 13, 7, SKColors.Black);

            Display.DrawText(Common.Fonts.FourBySix, 2, 7, SKColors.White, $"{fps}");
        }

        private void DrawErrorSymbol()
        {
            int x = Display.Width - (_errorImage.Width + 1);
            int y = 1;

            Display.DrawImage(x, y, _errorImage);
        }

        public void Close()
        {
            lock (SceneManager.SceneChangingLock)
            {
                if (!SceneClosing)
                {
                    if (Settings.DebugMode)
                    {
                        ZEDProgram.Instance.Logger.Log($"Exiting scene [{Name}].");
                    }

                    SceneClosing = true;
                    InputManager.Instance.ButtonChanged -= OnButtonChanged;
                    InputManager.Instance.AxisChanged -= OnAxisChanged;
                    //ZEDProgram.Instance.Closing -= OnProgramClosing;
                }
            }
        }
    }
}
