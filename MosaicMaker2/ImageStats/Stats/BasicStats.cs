using System;

namespace ImageStats.Stats
{
    [Serializable]
    public struct BasicStats
    {
        public ConvolutionResult LowResIntensity { get; set; }
        public ConvolutionResult LowResR { get; set; }
        public ConvolutionResult LowResG { get; set; }
        public ConvolutionResult LowResB { get; set; }
    }

    public struct AdvancedStats
    {
        public ConvolutionResult MidResHorizontal { get; set; }
        public ConvolutionResult MidResVertical { get; set; }
        public ConvolutionResult MidRes45 { get; set; }
        public ConvolutionResult MidRes135 { get; set; }
        public ConvolutionResult MidResEdge { get; set; }


        public ConvolutionResult MidResIntensity { get; set; }
        public ConvolutionResult MidResR { get; set; }
        public ConvolutionResult MidResG { get; set; }
        public ConvolutionResult MidResB { get; set; }
    }
}