namespace ImageStats.RegionCreation
{
    public class NonOverlappingRegionCreationStrategy : FixedSizeRegionCreationStrategy
    {
        public NonOverlappingRegionCreationStrategy(int holeWidth, int holeHeight)
            : base(holeWidth, holeHeight, holeWidth, holeHeight)
        {

        }
    }
}