using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageStats.Stats
{
    [Serializable]
    public struct ConvolutionResult
    {
        public ConvolutionResult(IEnumerable<int> values)
        {
            Values = values.ToArray();
        }
        public int[] Values { get; set; }
    }
}