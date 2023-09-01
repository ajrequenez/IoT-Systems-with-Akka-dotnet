using Akka.Actor;

namespace BuildingMonitor.Messages
{
	public sealed class ResponseRegisterTemperatureSensor
	{
		public long RequestId { get; }
		public IActorRef SensorRef { get; }

		public ResponseRegisterTemperatureSensor(long request, IActorRef sensorRef)
		{
			RequestId = request;
			SensorRef = sensorRef;
		}
	}
}

