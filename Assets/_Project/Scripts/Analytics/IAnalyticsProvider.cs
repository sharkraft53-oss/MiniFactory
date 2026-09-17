using System.Collections.Generic;

namespace MiniFactory.Analytics
{
    public interface IAnalyticsProvider
    {
        void SendEvent(
            string eventName,
            Dictionary<string, object> parameters = null
        );
    }
}