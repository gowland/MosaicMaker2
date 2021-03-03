using System;
using System.Collections.Generic;
using System.Drawing;
using ImageStats.Stats;

namespace ImageStats
{
    public class ReconstructedImageBuilder
    {
        private readonly FastBitmap.FastBitmap _target;
        private readonly double _newImagePercentage;
        private readonly double _oldImagePercentage;
        private readonly Bitmap _newImage;

        public ReconstructedImageBuilder(FastBitmap.FastBitmap target, double newImagePercentage)
        {
            _target = target;
            _newImagePercentage = newImagePercentage;
            _oldImagePercentage = (1 - _newImagePercentage);
            _newImage = new Bitmap(target.Width, target.Height);
        }

        public Bitmap NewImage => _newImage;

        public void SaveAs(string path)
        {
            _newImage.Save(path);
        }

        public void ApplyMatches(IEnumerable<SourceAndMatch> matches)
        {
            _target.Lock();
            foreach (var imageMatch in matches)
            {
                WriteSourceAndFill(imageMatch);
            }
            _target.Unlock();
        }

        private void WriteSourceAndFill(SourceAndMatch match)
        {
            FastBitmap.FastBitmap fillImage = new FastBitmap.FastBitmap(match.ReplacementImage);

            fillImage.Lock();

            ImageManipulationInfo hole = match.SourceSegment;
            for (int xOffset = 0; xOffset < hole.Width; xOffset++)
            {
                for (int yOffset = 0; yOffset < hole.Height; yOffset++)
                {
                    Color holeColor = _target.GetPixel(hole.StartX + xOffset, hole.StartY + yOffset);
                    Color fillColor = fillImage.GetPixel(xOffset, yOffset);
                    _newImage.SetPixel(hole.StartX + xOffset, hole.StartY + yOffset, GetNewColor(holeColor, fillColor));
                }
            }

            fillImage.Unlock();
        }

        private Color GetNewColor(Color source, Color fill)
        {
            return Color.FromArgb(
                255,
                GetNewImageValue(source.R, fill.R),
                GetNewImageValue(source.G, fill.G),
                GetNewImageValue(source.B, fill.B));
        }

        private int GetNewImageValue(int source, int fill)
        {
            return (int)Math.Floor((source*_oldImagePercentage) + (fill*_newImagePercentage));
        }
    }
}