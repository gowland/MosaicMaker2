using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageStats.RegionCreation
{
    public class FixedSizeRegionCreationStrategy : IRegionCreationStrategy
    {
        private readonly int _regionHeight;
        private readonly int _regionWidth;
        private readonly int _horizontalStep;
        private readonly int _verticalStep;

        public FixedSizeRegionCreationStrategy(int regionWidth, int regionHeight, int horizontalStep, int verticalStep)
        {
            _regionHeight = regionHeight;
            _regionWidth = regionWidth;
            _horizontalStep = horizontalStep;
            _verticalStep = verticalStep;
        }

        public IEnumerable<Rectangle> GetRegions(Rectangle sourceRegion)
        {
            if (sourceRegion.Width < _regionWidth || sourceRegion.Height < _regionHeight)
                throw new Exception($"{sourceRegion.Width}x{sourceRegion.Height} < {_regionWidth}x{_regionHeight}");

            for (int xOffset = 0; xOffset + _regionWidth <= sourceRegion.Width; xOffset += _horizontalStep)
            {
                for (int yOffset = 0; yOffset + _regionHeight <= sourceRegion.Height; yOffset += _verticalStep)
                {
                    yield return new Rectangle(sourceRegion.X + xOffset, sourceRegion.Y + yOffset, _regionWidth, _regionHeight);
                }
            }
        }
    }
}