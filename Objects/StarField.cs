﻿using rpi_rgb_led_matrix_sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZED.Display;

namespace ZED.Objects
{
    internal class StarField
    {
        public List<Star> Stars;

        private IDisplay _display;

        public StarField(IDisplay display, int numStars = 50)
        {
            _display = display;

            Stars = new List<Star>();

            AddStars(numStars);
        }

        public void Draw(long lastFrameTicks)
        {
            foreach (Star star in Stars)
            {
                star.Draw(lastFrameTicks);
            }
        }

        public Star AddStar()
        {
            Star star = new Star(_display, ColorExtensions.ColorFromHSV(Program.Random.Next(360)));
            Stars.Add(star);
            return star;
        }

        public List<Star> AddStars(int numStars)
        {
            List<Star> rtn = new List<Star>();
            for (int i = 0; i < numStars; i++)
            {
                rtn.Add(AddStar());
            }
            return rtn;
        }
    }

    internal class Star
    {
        public int X;
        public int Y;

        public double MinVelocityY = 10;
        public double MaxVelocityY = 25;
               
        public double MinVelocityX = 1;
        public double MaxVelocityX = 5;

        private int _hue = 0;
        public int Hue
        {
            get { return _hue; }
            set { _hue = value; Color = ColorExtensions.ColorFromHSV(value); }
        }

        public Color Color;

        private double _yVelocity;
        private double _xVelocity;

        private IDisplay _display;

        public Star(IDisplay display, Color color)
        {
            Color = color;

            _display = display;

            InitPosition();
        }

        public void InitPosition()
        {
            X = Program.Random.Next(_display.Width);
            Y = -Program.Random.Next(30);
            _xVelocity = (MinVelocityX + Program.Random.NextDouble() * (MaxVelocityX - MinVelocityX)) * (Program.Random.Next() % 2 == 0 ? 1 : -1);
            _yVelocity = MinVelocityY + Program.Random.NextDouble() * (MaxVelocityY - MinVelocityY);
        }

        private double _msAccumulatorX = 0;
        private double _msAccumulatorY = 0;
        public void Draw(long lastFrameTicks)
        {
            _msAccumulatorX += lastFrameTicks / (double)TimeSpan.TicksPerMillisecond;
            _msAccumulatorY += lastFrameTicks / (double)TimeSpan.TicksPerMillisecond;

            if (_xVelocity != 0)
            {
                int msPerMove = (int)(1000 / Math.Abs(_xVelocity));

                if (_msAccumulatorX >= msPerMove)
                {
                    _msAccumulatorX -= msPerMove;
                    X += _xVelocity < 0 ? -1 : 1;
                }
            }
            else
            {
                _msAccumulatorX = 0;
            }

            if (_yVelocity != 0)
            {
                int msPerMove = (int)(1000 / Math.Abs(_yVelocity));

                if (_msAccumulatorY >= msPerMove)
                {
                    _msAccumulatorY -= msPerMove;
                    Y += _yVelocity < 0 ? -1 : 1;
                }
            }
            else
            {
                _msAccumulatorY = 0;
            }

            if (Y > _display.Height)
            {
                InitPosition();
            }
            else
            {
                _display.SetPixel(X, Y, Color);
            }
        }
    }
}
