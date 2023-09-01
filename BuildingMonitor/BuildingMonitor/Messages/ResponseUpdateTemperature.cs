namespace BuildingMonitor.Messages
{
	public sealed class ResponseUpdateTemperature
	{
        public long RequestId { get; }

		public ResponseUpdateTemperature(long requestId)
		{
			RequestId = requestId;
		}
	}
}

