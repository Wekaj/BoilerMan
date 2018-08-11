using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace LD42.Graphics {
    public sealed class Animation {
        private readonly List<Point> _frames = new List<Point>();

        public Animation(int frameWidth, int frameHeight) {
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
        }

        public int FrameWidth { get; }
        public int FrameHeight { get; }

        public Animation AddFrame(int x, int y) {
            _frames.Add(new Point(x, y));
            return this;
        }

        public Rectangle GetFrame(float p) {
            int frameNumber = Math.Min((int)(p * _frames.Count), _frames.Count - 1);
            Point frame = _frames[frameNumber];
            return new Rectangle(frame.X * FrameWidth, frame.Y * FrameHeight, 
                FrameWidth, FrameHeight);
        }
    }
}
