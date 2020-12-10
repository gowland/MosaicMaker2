using System;

namespace ImageStats.Stats
{
    [Serializable]
    public struct Alphabet
    {
        public Alphabet(ImageAndStats[] imagesAndStats)
        {
            ImagesAndStats = imagesAndStats;
        }

        public ImageAndStats[] ImagesAndStats { get; set; }
    }
}