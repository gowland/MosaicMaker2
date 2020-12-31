using System;
using System.Collections.Generic;
using ImageStats.Stats;

namespace ImageStats.MatchFilters
{
    public class CompoundFilterBuilder
    {
        private readonly string _name;
        readonly IList<IFilter> _filters = new List<IFilter>();

        public CompoundFilterBuilder(string name = null)
        {
            _name = name ?? Guid.NewGuid().ToString();
        }

        public CompoundFilterBuilder WithConvolutionResultFilter(Func<int, bool> isWithinThresholdFunc, Func<Stats.BasicStats, ConvolutionResult> getConvolutionResult, string name)
        {
            _filters.Add(new ConvolutionResultFilter(isWithinThresholdFunc, getConvolutionResult, name));
            return this;
        }

        public CompoundFilter Build()
        {
            return new CompoundFilter(_filters, _name);
        }
    }
}