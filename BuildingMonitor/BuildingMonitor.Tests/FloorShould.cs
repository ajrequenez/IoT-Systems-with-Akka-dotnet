using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;

namespace BuildingMonitor.Tests
{
	public class FloorShould : TestKit
	{
		[Fact]
		public void RegisterTemperatureSensorWhenDoesNotAlreadyExist()
		{
			var probe = CreateTestProbe();
			var floor = Sys.ActorOf(Floor.Props("a"));

			floor.Tell(new RequestRegisterTemperatureSensor(10, "a", "11"), probe.Ref);

			var received = probe.ExpectMsg<ResponseRegisterTemperatureSensor>();

			Assert.Equal(10, received.RequestId);

            // Could check if sensor is repsonding
			//var sensor = probe.LastSender;
        }

		[Fact]
		public void ReturnExistingSensorWhenRegisteringSameSensor()
		{
			var probe = CreateTestProbe();
			var floor = Sys.ActorOf(Floor.Props("b"));

            floor.Tell(new RequestRegisterTemperatureSensor(21, "b", "11"), probe.Ref);

            var received = probe.ExpectMsg<ResponseRegisterTemperatureSensor>();
			var firstSensor = probe.LastSender;

			// Try to register same sensor
            floor.Tell(new RequestRegisterTemperatureSensor(22, "b", "11"), probe.Ref);
            var receivedSecond = probe.ExpectMsg<ResponseRegisterTemperatureSensor>();
            var secondSensor = probe.LastSender;

			Assert.Equal(firstSensor, secondSensor);
        }
    }
}
