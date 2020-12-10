namespace ImageStats.MatchFilters
{
    public struct FilterResult
    {
        public FilterResult(bool passed, string filterName)
        {
            Passed = passed;
            FilterName = filterName;
        }
        public bool Passed { get; }
        public string FilterName { get; }
    }
}