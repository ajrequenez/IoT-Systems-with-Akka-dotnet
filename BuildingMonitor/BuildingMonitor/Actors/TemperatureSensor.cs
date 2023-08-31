using Akka.Actor;
using BuildingMonitor.Messages;

namespace BuildingMonitor.Actors
{
    public class TemperatureSensor : UntypedActor
    {
        private string _floorId;
        private string _sensorId;
        private double? _lastRecordedTemperature;

        public TemperatureSensor(string floorId, string sensorId)
        {
            _floorId = floorId;
            _sensorId = sensorId;
        }

        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case RequestMetadata m:
                    Sender.Tell(new ResponseMetadata(m.RequestId, _floorId, _sensorId));
                    break;
                case RequestTemperature t:
                    Sender.Tell(new ResponseTemperature(t.RequestId, _lastRecordedTemperature));
                    break;
                default:
                    break;

            }
        }

        public static Props Props(string floorId, string sensorId) =>
            Akka.Actor.Props.Create(() => new TemperatureSensor(floorId, sensorId));
    }
}

