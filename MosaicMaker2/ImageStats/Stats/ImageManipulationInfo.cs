using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace ImageStats.Stats
{
    [Serializable]
    public struct ImageManipulationInfo
    {
        private Rectangle? _rect;

        public ImageManipulationInfo(int startX, int startY, int width, int height)
        {
            StartX = startX;
            StartY = startY;
            Width = width;
            Height = height;
            _rect = new Rectangle(StartX, StartY, Width, Height);
        }

        private Rectangle AsRectangle()
        {
            return new Rectangle(StartX, StartY, Width, Height);
        }

        public int StartX { get; set; }
        public int StartY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Rectangle Rectangle => _rect ?? (_rect = AsRectangle()).Value;

        public override string ToString()
        {
            return $"[{StartX},{StartY} -> {Width},{Height}]";
        }
    }
}