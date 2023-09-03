using System.Collections.Immutable;

namespace BuildingMonitor.Messages
{
    public sealed class ResponseTemperatureSensorIds
    {
        public long RequestId { get; }
        public IImmutableSet<string> SensorIds { get; }

        public ResponseTemperatureSensorIds(long requestId, IImmutableSet<string> sensorIds)
        {
            RequestId = requestId;
            SensorIds = sensorIds;
        }
    }
}