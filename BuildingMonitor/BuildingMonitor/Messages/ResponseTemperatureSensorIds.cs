namespace BuildingMonitor.Messages
{
    public sealed class ResponseTemperatureSensorIds
    {
        public long RequestId { get; }
        public ISet<string> SensorIds { get; }

        public ResponseTemperatureSensorIds(long requestId, ISet<string> sensorIds)
        {
            RequestId = requestId;
            SensorIds = sensorIds;
        }
    }
}