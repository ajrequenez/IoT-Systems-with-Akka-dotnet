using System;
using System.Collections.Immutable;
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
                        Context.Watch(newSensorActor);
                        _sensorIdToActorRefMap.Add(m.SensorId, newSensorActor);
                        newSensorActor.Forward(m);
                    }
                    break;
                case RequestTemperatureSensorIds m:
                    Sender.Tell(new ResponseTemperatureSensorIds(m.RequestId, _sensorIdToActorRefMap.Keys.ToImmutableHashSet()));
                    break;
                case Terminated m:
                    var sensorId = _sensorIdToActorRefMap.First(x => x.Value == m.ActorRef).Key;
                    _sensorIdToActorRefMap.Remove(sensorId);
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }

        public static Props Props(string floorId) => Akka.Actor.Props.Create(() => new Floor(floorId));
    }
}

