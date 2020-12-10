using System.Collections.Generic;
using System.Drawing;

namespace ImageStats.RegionCreation
{
    public interface IRegionCreationStrategy
    {
        IEnumerable<Rectangle> GetRegions(Rectangle sourceRegion);
    }
}