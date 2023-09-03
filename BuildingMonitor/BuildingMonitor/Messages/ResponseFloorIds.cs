using System.Collections.Immutable;

namespace BuildingMonitor.Messages
{
    public sealed class ResponseFloorIds
    {
        public long RequestId { get; }
        public IImmutableSet<string> FloorIds { get; }

        public ResponseFloorIds(long requestId, IImmutableSet<string> floorIds)
        {
            RequestId = requestId;
            FloorIds = floorIds;
        }
    }
}