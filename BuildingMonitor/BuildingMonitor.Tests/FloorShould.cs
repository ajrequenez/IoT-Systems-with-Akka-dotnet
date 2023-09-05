using Akka.Actor;
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

		[Fact]
		public void ReturnTemperatureSensorsOnlyForActiveActors(){
			var probe = CreateTestProbe();
			var floor = Sys.ActorOf(Floor.Props("f"));

			floor.Tell(new RequestRegisterTemperatureSensor(60, "f", "1"), probe.Ref);
			var received = probe.ExpectMsg<ResponseRegisterTemperatureSensor>();
			var sensor1 = probe.LastSender;

			floor.Tell(new RequestRegisterTemperatureSensor(61, "f", "2"), probe.Ref);
			var received2 = probe.ExpectMsg<ResponseRegisterTemperatureSensor>();
			var sensor2 = probe.LastSender;

			probe.Watch(sensor1);
			sensor1.Tell(PoisonPill.Instance);
			probe.ExpectTerminated(sensor1);

			floor.Tell(new RequestTemperatureSensorIds(63), probe.Ref);
			var received3 = probe.ExpectMsg<ResponseTemperatureSensorIds>();

			Assert.Equal(63, received3.RequestId);
			Assert.Equal(new HashSet<string> { "2" }, received3.SensorIds);
			Assert.Single(received3.SensorIds);

			probe.Watch(sensor2);
			sensor2.Tell(PoisonPill.Instance);
			probe.ExpectTerminated(sensor2);

			floor.Tell(new RequestTemperatureSensorIds(64), probe.Ref);
			var received4 = probe.ExpectMsg<ResponseTemperatureSensorIds>();

			Assert.Equal(64, received4.RequestId);
			Assert.Equal(new HashSet<string>(), received4.SensorIds);
			Assert.Empty(received4.SensorIds);
		}

		[Fact]
		public void InitiateQueryWhenRequested()
		{
			var probe = CreateTestProbe();
			var floor = Sys.ActorOf(Floor.Props("g"));

			floor.Tell(new RequestRegisterTemperatureSensor(70, "g", "1"), probe.Ref);
			probe.ExpectMsg<ResponseRegisterTemperatureSensor>();
			var sensor1 = probe.LastSender;

			floor.Tell(new RequestRegisterTemperatureSensor(71, "g", "2"), probe.Ref);
			probe.ExpectMsg<ResponseRegisterTemperatureSensor>();
			var sensor2 = probe.LastSender;

			sensor1.Tell(new RequestUpdateTemperature(0, 100.0));
			sensor2.Tell(new RequestUpdateTemperature(0, 80.0));

			floor.Tell(new RequestAllTemperatures(77), probe.Ref);
			var response = probe.ExpectMsg<ResponseAllTemperatures>(x => x.RequestId == 77);

			Assert.Equal(2, response.Temperatures.Count);

			var reading1 = Assert.IsAssignableFrom<TemperatureAvailable>(response.Temperatures["1"]);
			Assert.Equal(100.0, reading1.Temperature);

			var reading2 = Assert.IsAssignableFrom<TemperatureAvailable>(response.Temperatures["2"]);
			Assert.Equal(80.0, reading2.Temperature);
		}
    }
}
