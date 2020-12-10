namespace ImageStats.MatchFilters
{
    public interface IFilter
    {
        FilterResult Compare(Stats.ImageStats a, Stats.ImageStats b);
    }
}