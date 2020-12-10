using System;
using ImageStats.Stats;

namespace ImageStats.MatchFilters
{
    public class ConvolutionResultFilter : IFilter
    {
        private readonly Func<int, bool> _isWithinThresholdFunc;
        private readonly Func<Stats.ImageStats, ConvolutionResult> _getConvolutionResult;
        private readonly string _name;

        public ConvolutionResultFilter(Func<int, bool> isWithinThresholdFunc, Func<Stats.ImageStats, ConvolutionResult> getConvolutionResult, string name)
        {
            _isWithinThresholdFunc = isWithinThresholdFunc;
            _getConvolutionResult = getConvolutionResult;
            _name = name;
        }

        public FilterResult Compare(Stats.ImageStats a, Stats.ImageStats b)
        {
            return new FilterResult(_isWithinThresholdFunc(_getConvolutionResult(a).Difference(_getConvolutionResult(b))), _name);
        }
    }
}