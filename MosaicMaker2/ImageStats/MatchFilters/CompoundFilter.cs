using System.Collections.Generic;
using System.Linq;

namespace ImageStats.MatchFilters
{
    public class CompoundFilter : IFilter
    {
        readonly IEnumerable<IFilter> _filters;
        private readonly string _name;

        public CompoundFilter(IEnumerable<IFilter> filters, string name)
        {
            _filters = filters;
            _name = name;
        }
        public FilterResult Compare(Stats.ImageStats a, Stats.ImageStats b)
        {
            var failedFilters = _filters
                .Select(filter => filter.Compare(a, b))
                .Where(result => !result.Passed);
            return failedFilters.Any() ? failedFilters.First() : new FilterResult(true, _name);
        }
    }
}