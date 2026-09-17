using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MiniFactory.Analytics
{
    public class ConsoleAnalyticsProvider : IAnalyticsProvider
    {
        public void SendEvent(
            string eventName,
            Dictionary<string, object> parameters = null)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append($"[Analytics] {eventName}");

            if (parameters != null &&
                parameters.Count > 0)
            {
                builder.Append(" | ");

                bool first = true;

                foreach (var parameter in parameters)
                {
                    if (!first)
                        builder.Append(", ");

                    builder.Append(
                        $"{parameter.Key}: {parameter.Value}"
                    );

                    first = false;
                }
            }

            Debug.Log(builder.ToString());
        }
    }
}