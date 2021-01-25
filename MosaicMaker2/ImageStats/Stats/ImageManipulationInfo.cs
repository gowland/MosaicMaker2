using System;
using System.Drawing;

namespace ImageStats.Stats
{
    [Serializable]
    public struct ImageManipulationInfo
    {
        public ImageManipulationInfo(int startX, int startY, int width, int height)
        {
            Rectangle = new Rectangle(startX, startY, width, height);
        }

        public int StartX => Rectangle.X;
        public int StartY => Rectangle.Y;
        public int Width => Rectangle.Width;
        public int Height => Rectangle.Height;
        public Rectangle Rectangle { get; }

        public override string ToString()
        {
            return $"[{StartX},{StartY} -> {Width},{Height}]";
        }
    }
}