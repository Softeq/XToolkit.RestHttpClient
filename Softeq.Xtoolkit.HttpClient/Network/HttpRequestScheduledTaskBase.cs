// Developed for LilBytes by Softeq Development Corporation
//

using System;
using System.Threading;
using Softeq.HttpClient.Common;

namespace Softeq.XToolkit.HttpClient.Network
{
    internal abstract class HttpRequestScheduledTaskBase : IEquatable<HttpRequestScheduledTaskBase>
    {
        private readonly Guid _id;

        public Guid Id => _id;

        public int Timeout { get; private set; }
        public bool DisableSystemCache { get; set; }
        public ForegroundTaskDeferral Deferral { get; private set; }
        public DateTime ExecutionStartedTime { get; set; }
        public HttpRequestPriority Priority { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; private set; }

        protected HttpRequestScheduledTaskBase(int executionTimeoutInMilliseconds)
        {
            _id = Guid.NewGuid();

            Timeout = executionTimeoutInMilliseconds;
            Deferral = new ForegroundTaskDeferral();
            CancellationTokenSource = new CancellationTokenSource();
        }

        public bool Equals(HttpRequestScheduledTaskBase other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((HttpRequestScheduledTaskBase)obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}
