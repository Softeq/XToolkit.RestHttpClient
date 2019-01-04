using System;

namespace Softeq.XToolkit.CrossCutting
{
    public class OncePerIntervalAction
    {
        public DateTime LastChangeOnTime { get; set; }

        public TimeSpan Interval { get; }
        public Action Operation { get; }

        public OncePerIntervalAction(TimeSpan interval, Action operation)
        {
            Interval = interval;
            Operation = operation;
        }
    }
}
