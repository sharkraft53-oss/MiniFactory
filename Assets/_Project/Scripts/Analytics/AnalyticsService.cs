using System.Collections.Generic;

namespace MiniFactory.Analytics
{
    public class AnalyticsService
    {
        private readonly List<IAnalyticsProvider>
            _providers = new();

        public void AddProvider(
            IAnalyticsProvider provider)
        {
            if (provider == null)
                return;

            if (!_providers.Contains(provider))
            {
                _providers.Add(provider);
            }
        }

        public void SendEvent(
            string eventName,
            Dictionary<string, object> parameters = null)
        {
            foreach (IAnalyticsProvider provider
                     in _providers)
            {
                provider.SendEvent(
                    eventName,
                    parameters
                );
            }
        }
    }
}