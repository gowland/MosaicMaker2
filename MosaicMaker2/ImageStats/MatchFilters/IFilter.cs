namespace ImageStats.MatchFilters
{
    public interface IFilter
    {
        FilterResult Compare(Stats.BasicStats a, Stats.BasicStats b);
    }
}