using Akka.Actor;
using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;

namespace BuildingMonitor.Tests
{
    public class FloorManagerShould : TestKit
    {
        [Fact]
        public void ReturnNoFloorIdsWhenNewlyCreatd()
        {
            var probe = CreateTestProbe();
            var floorManager = Sys.ActorOf(FloorManager.Props());

            floorManager.Tell(new RequestFloorIds(10), probe.Ref);

            var received = probe.ExpectMsg<ResponseFloorIds>();

            Assert.Equal(10, received.RequestId);
            Assert.Empty(received.FloorIds);
        }

        [Fact]
        public void RegisterNewFloorWhenDoesNotExist()
        {
            var probe = CreateTestProbe();
            var floorManager = Sys.ActorOf(FloorManager.Props());

            floorManager.Tell(new RequestRegisterTemperatureSensor(20, "a", "1"), probe.Ref);
            var received = probe.ExpectMsg<ResponseRegisterTemperatureSensor>();

            Assert.Equal(20, received.RequestId);

            floorManager.Tell(new RequestFloorIds(21), probe.Ref);
            var receivedFloorIds = probe.ExpectMsg<ResponseFloorIds>();

            Assert.Equal(21, receivedFloorIds.RequestId);
            Assert.Equal(new[] { "a" }, receivedFloorIds.FloorIds);
            Assert.Single(receivedFloorIds.FloorIds);
        }

        [Fact]
        public void UseExistingFloorWhenAlreadyExists()
        {
            var probe = CreateTestProbe();
            var floorManager = Sys.ActorOf(FloorManager.Props());

            floorManager.Tell(new RequestRegisterTemperatureSensor(30, "a", "1"), probe.Ref);
            probe.ExpectMsg<ResponseRegisterTemperatureSensor>(x => x.RequestId == 30);

            floorManager.Tell(new RequestRegisterTemperatureSensor(31, "a", "2"), probe.Ref);
            probe.ExpectMsg<ResponseRegisterTemperatureSensor>(x => x.RequestId == 31);
 
            floorManager.Tell(new RequestFloorIds(32), probe.Ref);
            var receivedFloorIds = probe.ExpectMsg<ResponseFloorIds>();

            Assert.Equal(32, receivedFloorIds.RequestId);
            Assert.Equal(new[] { "a" }, receivedFloorIds.FloorIds);
            Assert.Single(receivedFloorIds.FloorIds);
        }
    }
}