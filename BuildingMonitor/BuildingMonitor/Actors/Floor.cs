using System;
using Akka.Actor;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;

namespace BuildingMonitor.Actors
{
	public class Floor : UntypedActor
	{
        private readonly string _floorId;
        private Dictionary<string, IActorRef> _sensorIdToActorRefMap = new Dictionary<string, IActorRef>();

		public Floor(string floorId)
		{
            _floorId= floorId;
		}

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RequestRegisterTemperatureSensor m when m.FloorId == _floorId:
                    if(_sensorIdToActorRefMap.TryGetValue(m.SensorId, out var existingActor))
                    {
                        // Return ref is existing
                        existingActor.Forward(m);
                    }
                    else
                    {
                        // Create new temperature sensor and fwd registreaion message
                        var newSensorActor = Context.ActorOf(
                            TemperatureSensor.Props(m.FloorId, m.SensorId),
                            $"temperature-sensor-{m.SensorId}");
                        _sensorIdToActorRefMap.Add(m.SensorId, newSensorActor);
                        newSensorActor.Forward(m);
                    }
                    break;

                default:
                    Unhandled(message);
                    break;
            }
        }

        public static Props Props(string floorId) => Akka.Actor.Props.Create(() => new Floor(floorId));
    }
}

