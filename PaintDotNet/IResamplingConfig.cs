namespace PaintDotNet
{
    using System;

    internal interface IResamplingConfig
    {
        event EventHandler ResamplingAlgorithmChanged;

        void PerformResamplingAlgorithmChanged();

        PaintDotNet.ResamplingAlgorithm ResamplingAlgorithm { get; set; }
    }
}

