using System.Collections.Immutable; 

namespace BuildingMonitor.Messages {

    public sealed class ResponseAllTemperatures
    {
        public long RequestId { get; }
        public ImmutableDictionary<string, ITemperatureQueryResult> Temperatures { get; }

        public ResponseAllTemperatures(long requestId, ImmutableDictionary<string, ITemperatureQueryResult> temperatures)
        {
            RequestId = requestId;
            Temperatures = temperatures;
        }
    }
}