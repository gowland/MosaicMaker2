using System;

namespace ImageStats.Stats
{
    [Serializable]
    public struct ImageStats
    {
        public ConvolutionResult LowResIntensity { get; set; }
        public ConvolutionResult LowResR { get; set; }
        public ConvolutionResult LowResG { get; set; }
        public ConvolutionResult LowResB { get; set; }
        public ConvolutionResult MidResHorizontal { get; set; }
        public ConvolutionResult MidResVertical { get; set; }
        public ConvolutionResult MidRes45 { get; set; }
        public ConvolutionResult MidRes135 { get; set; }
    }
}