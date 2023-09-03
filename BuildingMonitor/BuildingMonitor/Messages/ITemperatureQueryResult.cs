namespace BuildingMonitor.Messages
{
	public interface ITemperatureQueryResult { }

	public sealed class TemperatureAvailable : ITemperatureQueryResult
	{
		public double Temperature { get; }

		public TemperatureAvailable(double temperature)
		{
			Temperature = temperature;
		}
	}

	public sealed class NoTemperatureReadingRecordedYet : ITemperatureQueryResult
	{
		public static NoTemperatureReadingRecordedYet Instance { get; } = new NoTemperatureReadingRecordedYet();

		private NoTemperatureReadingRecordedYet() { }

	}

	public sealed class TemperatureSensorNotAvailable : ITemperatureQueryResult
	{
		public static TemperatureSensorNotAvailable Instance { get; } = new TemperatureSensorNotAvailable();

		private TemperatureSensorNotAvailable() { }
	}

	public sealed class TemperatureSensorTimedOut : ITemperatureQueryResult
	{
		public static TemperatureSensorTimedOut Instance { get; } = new TemperatureSensorTimedOut();

		private TemperatureSensorTimedOut() { }
	}
}

