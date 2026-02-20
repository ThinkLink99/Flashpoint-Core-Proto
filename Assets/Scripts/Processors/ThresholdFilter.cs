using System;

namespace Assets.Scripts
{
    public class ThresholdFilter : IProcessor<float, bool>
    {
        readonly Func<float> getThreshold;

        public ThresholdFilter(Func<float> getThreshold)
        {
            this.getThreshold = getThreshold;
        }

        public bool Process(float score) => score >= getThreshold();
    }
}
