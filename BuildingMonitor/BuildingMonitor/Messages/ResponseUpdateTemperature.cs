namespace BuildingMonitor.Messages
{
	public class ResponseUpdateTemperature
	{
		public long RequestId { get; }

		public ResponseUpdateTemperature(long requestId)
		{
			RequestId = requestId;
		}
	}
}

