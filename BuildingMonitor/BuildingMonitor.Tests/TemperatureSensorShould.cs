using Xunit;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;

namespace BuildingMonitor.Tests
{
    public class TemperatureSensorShould : TestKit
    {
        [Fact]
        public void InitializeSensorMetaData()
        {
            var probe = CreateTestProbe();

            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));

            sensor.Tell(new RequestMetadata(10), probe.Ref);

            var received = probe.ExpectMsg<ResponseMetadata>();

            Assert.Equal(10, received.RequestId);
            Assert.Equal("a", received.FloorId);
            Assert.Equal("1", received.SensorId);
        }

        [Fact]
        public void StartWithNoTemperatureReading()
        {
            var probe = CreateTestProbe();

            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));

            sensor.Tell(new RequestTemperature(20), probe.Ref);

            var received = probe.ExpectMsg<ResponseTemperature>();

            Assert.Equal(20, received.RequestId);
            Assert.Null(received.Temperature);
        }

        [Fact]
        public void ConfirmTemperatureUpdate()
        {
            var probe = CreateTestProbe();

            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));

            sensor.Tell(new RequestUpdateTemperature(30, 97.5), probe.Ref);
            var received = probe.ExpectMsg<ResponseUpdateTemperature>();

            Assert.Equal(30, received.RequestId);
        }

        [Fact]
        public void UpdateNewTemperatureReading()
        {
            var probe = CreateTestProbe();

            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));

            sensor.Tell(new RequestUpdateTemperature(40, 97.5));
            sensor.Tell(new RequestTemperature(41), probe.Ref);
            var receivedTemperature = probe.ExpectMsg<ResponseTemperature>();

            Assert.Equal(41, receivedTemperature.RequestId);
            Assert.Equal(97.5, receivedTemperature.Temperature);
        }

        [Fact]
        public void RegisterSensor()
        {
            var probe = CreateTestProbe();

            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));

            sensor.Tell(new RequestRegisterTemperatureSensor(50, "a", "1"), probe.Ref);

            var received = probe.ExpectMsg<ResponseRegisterTemperatureSensor>();

            Assert.Equal(50, received.RequestId);
            Assert.Equal(sensor, received.SensorRef);
        }

        [Fact]
        public void NotRegisterSensorWhenIncorrectFloorId()
        {
            var probe = CreateTestProbe();
            var eventStreamProbe = CreateTestProbe();

            Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));

            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));

            sensor.Tell(new RequestRegisterTemperatureSensor(50, "b", "1"), probe.Ref);

            probe.ExpectNoMsg();

            var unhandled = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();

            Assert.IsType<RequestRegisterTemperatureSensor>(unhandled.Message);
        }

        [Fact]
        public void NotRegisterSensorWhenIncorrectSensorId()
        {
            var probe = CreateTestProbe();
            var eventStreamProbe = CreateTestProbe();

            Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));

            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));

            sensor.Tell(new RequestRegisterTemperatureSensor(50, "a", "2"), probe.Ref);

            probe.ExpectNoMsg();

            var unhandled = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();

            Assert.IsType<RequestRegisterTemperatureSensor>(unhandled.Message);
        }
    }
}

