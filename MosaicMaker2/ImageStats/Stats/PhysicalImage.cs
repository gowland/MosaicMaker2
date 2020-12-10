using System;

namespace ImageStats.Stats
{
    [Serializable]
    public struct PhysicalImage
    {
        public PhysicalImage(string imagePath)
        {
            ImagePath = imagePath;
        }

        public string ImagePath { get; set; }
    }
}