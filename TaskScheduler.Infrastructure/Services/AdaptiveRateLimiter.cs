using System;
using System.Collections.Generic;
using System.Text;

namespace TaskScheduler.Infrastructure.Services
{
    public class AdaptiveRateLimiter
    {
        private readonly int _minDelayMs    = 100;
        private readonly int _maxDelayMs    = 5000;
        private readonly int _lowThreshold  = 10;
        private readonly int _highThreshold = 100;

        public async Task WaitAsync(int pendingJobCount, CancellationToken cancellationToken)
        {
            var delay = CalculateDelay(pendingJobCount);
            await Task.Delay(delay, cancellationToken);
        }
        private int CalculateDelay(int pendingJobCount)
        {
            if (pendingJobCount <= _lowThreshold)
                return _minDelayMs;
            if (pendingJobCount >= _highThreshold)
                return _maxDelayMs;

            //linear interpolation betweem min and max delay
            var ratio = (double)(pendingJobCount - _lowThreshold) / (_highThreshold - _lowThreshold);
            return (int)(_minDelayMs + ratio * (_maxDelayMs - _minDelayMs));
        }
    }
}
