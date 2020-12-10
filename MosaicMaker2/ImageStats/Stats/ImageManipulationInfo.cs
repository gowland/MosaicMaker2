using System;

namespace ImageStats.Stats
{
    [Serializable]
    public struct ImageManipulationInfo
    {
        public ImageManipulationInfo(int startX, int startY, int width, int height)
        {
            StartX = startX;
            StartY = startY;
            Width = width;
            Height = height;
        }

        public int StartX { get; set; }
        public int StartY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public override string ToString()
        {
            return $"[{StartX},{StartY} -> {Width},{Height}]";
        }
    }
}