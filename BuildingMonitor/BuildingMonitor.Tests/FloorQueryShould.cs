using Akka.Actor;
using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;

namespace BuildingMonitor.Tests
{
    public class FloorQueryShould : TestKit
    {
        [Fact]
        public void ReturnTemperatures(){
            var queryRequester = CreateTestProbe();
            
            var temperatureSensor1 = CreateTestProbe();
            var temperatureSensor2 = CreateTestProbe();

            var floorQuery = Sys.ActorOf(FloorQuery.Props(
                actorToSensorId: new Dictionary<IActorRef, string>
                {
                    [temperatureSensor1.Ref] = "sensor1",
                    [temperatureSensor2.Ref] = "sensor2" 
                },
                requestId: 10,
                requester: queryRequester.Ref,
                timeout: TimeSpan.FromSeconds(3)
            ));

            temperatureSensor1.ExpectMsg<RequestTemperature>((m, sender) => 
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(floorQuery, sender);
            });
            
            temperatureSensor2.ExpectMsg<RequestTemperature>((m, sender) => 
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(floorQuery, sender);
            });

            floorQuery.Tell(new ResponseTemperature(FloorQuery.TemperatureRequestCorrelationId, 74.0), temperatureSensor1.Ref);
            floorQuery.Tell(new ResponseTemperature(FloorQuery.TemperatureRequestCorrelationId, 71.0), temperatureSensor2.Ref);

            var response = queryRequester.ExpectMsg<ResponseAllTemperatures>();
            
            Assert.Equal(10, response.RequestId);
            Assert.Equal(2, response.Temperatures.Count);

            var temperatureReading1 = Assert.IsAssignableFrom<TemperatureAvailable>(response.Temperatures["sensor1"]);
            Assert.Equal(74.0, temperatureReading1.Temperature);

            var temperatureReading2 = Assert.IsAssignableFrom<TemperatureAvailable>(response.Temperatures["sensor2"]);
            Assert.Equal(71.0, temperatureReading2.Temperature);
        }

        [Fact]
        public void ReturnNoTemperatureAvailableResult()
        {
            var queryRequester = CreateTestProbe();

            var temperatureSensor1 = CreateTestProbe();
            var temperatureSensor2 = CreateTestProbe();

            var floorQuery = Sys.ActorOf(FloorQuery.Props(
                actorToSensorId: new Dictionary<IActorRef, string>
                {
                    [temperatureSensor1.Ref] = "sensor1",
                    [temperatureSensor2.Ref] = "sensor2" 
                },
				requestId: 20,
				requester: queryRequester.Ref,
				timeout: TimeSpan.FromSeconds(3)
            ));

			temperatureSensor1.ExpectMsg<RequestTemperature>((m, sender) => 
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(floorQuery, sender);
            });
            
            temperatureSensor2.ExpectMsg<RequestTemperature>((m, sender) => 
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(floorQuery, sender);
            });

            floorQuery.Tell(new ResponseTemperature(FloorQuery.TemperatureRequestCorrelationId, null), temperatureSensor1.Ref);
            floorQuery.Tell(new ResponseTemperature(FloorQuery.TemperatureRequestCorrelationId, 71.0), temperatureSensor2.Ref);

            var response = queryRequester.ExpectMsg<ResponseAllTemperatures>();
            
            Assert.Equal(20, response.RequestId);
            Assert.Equal(2, response.Temperatures.Count);

            Assert.IsAssignableFrom<NoTemperatureReadingRecordedYet>(response.Temperatures["sensor1"]);

            var temperatureReading2 = Assert.IsAssignableFrom<TemperatureAvailable>(response.Temperatures["sensor2"]);
            Assert.Equal(71.0, temperatureReading2.Temperature);
        }

		[Fact]
		public void RecognizeSensorsThatStoppedDuringQuery(){
			var queryRequester = CreateTestProbe();

			var temperatureSensor1 = CreateTestProbe();
			var temperatureSensor2 = CreateTestProbe();

            var floorQuery = Sys.ActorOf(FloorQuery.Props(
                actorToSensorId: new Dictionary<IActorRef, string>
                {
                    [temperatureSensor1.Ref] = "sensor1",
                    [temperatureSensor2.Ref] = "sensor2" 
                },
				requestId: 30,
				requester: queryRequester.Ref,
				timeout: TimeSpan.FromSeconds(1)
            ));

			temperatureSensor1.ExpectMsg<RequestTemperature>((m, sender) => 
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(floorQuery, sender);
            });
            
            temperatureSensor2.ExpectMsg<RequestTemperature>((m, sender) => 
            {
                Assert.Equal(FloorQuery.TemperatureRequestCorrelationId, m.RequestId);
                Assert.Equal(floorQuery, sender);
            });

            floorQuery.Tell(new ResponseTemperature(FloorQuery.TemperatureRequestCorrelationId, 74.0), temperatureSensor1.Ref);
            temperatureSensor2.Tell(PoisonPill.Instance);

            var response = queryRequester.ExpectMsg<ResponseAllTemperatures>();
            
            Assert.Equal(30, response.RequestId);
            Assert.Equal(2, response.Temperatures.Count);

            var temperatureReading1 = Assert.IsAssignableFrom<TemperatureAvailable>(response.Temperatures["sensor1"]);
            Assert.Equal(74.0, temperatureReading1.Temperature);

			Assert.IsAssignableFrom<TemperatureSensorNotAvailable>(response.Temperatures["sensor2"]);
		}
    }
}

