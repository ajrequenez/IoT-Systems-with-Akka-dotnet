using Akka.Actor;

namespace BuildingMonitor.Actors
{
    public class TemperatureSensor : UntypedActor
    {
        public TemperatureSensor(string floorId, string sensorId)
        {

        }

        protected override void OnReceive(object message)
        {
            throw new NotImplementedException();
        }

        public static Props Props(string floorId, string sensorId) =>
            Akka.Actor.Props.Create(() => new TemperatureSensor(floorId, sensorId));
    }
}

