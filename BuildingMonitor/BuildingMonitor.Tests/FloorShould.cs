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

		[Fact]
		public void NotRegisterWhenMismatchedFloor()
		{
			var probe = CreateTestProbe();
			var eventStreamProbe = CreateTestProbe();

			Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));

			var floor = Sys.ActorOf(Floor.Props("c"));

			floor.Tell(new RequestRegisterTemperatureSensor(30, "a", "1"), probe.Ref);
			probe.ExpectNoMsg();

			var unhandled = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();

			Assert.IsType<RequestRegisterTemperatureSensor>(unhandled.Message);
			Assert.Equal(floor, unhandled.Recipient);
		}

		[Fact]
		public void ReturnAllRegisteredTemperatureSensorIds(){
			var probe = CreateTestProbe();
			var floor = Sys.ActorOf(Floor.Props("d"));

			floor.Tell(new RequestRegisterTemperatureSensor(40, "d", "1"), probe.Ref);
			probe.ExpectMsg<ResponseRegisterTemperatureSensor>();

			floor.Tell(new RequestRegisterTemperatureSensor(41, "d", "2"), probe.Ref);
			probe.ExpectMsg<ResponseRegisterTemperatureSensor>();

			floor.Tell(new RequestTemperatureSensorIds(42), probe.Ref);
			var received = probe.ExpectMsg<ResponseTemperatureSensorIds>();

			Assert.Equal(42, received.RequestId);
			Assert.Equal(new HashSet<string> { "1", "2" }, received.SensorIds);

		}

		[Fact]
		public void ReturnEmptySetWhenNoTemperatureSensorsRegistered(){
			var probe = CreateTestProbe();
			var floor = Sys.ActorOf(Floor.Props("e"));

			floor.Tell(new RequestTemperatureSensorIds(50), probe.Ref);
			var received = probe.ExpectMsg<ResponseTemperatureSensorIds>();

			Assert.Equal(50, received.RequestId);
			Assert.Equal(new HashSet<string>(), received.SensorIds);
			Assert.Empty(received.SensorIds);
		}
    }
}
