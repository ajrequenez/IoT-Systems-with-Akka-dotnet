using System;
using System.Collections.Immutable;
using Akka.Actor;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;

namespace BuildingMonitor.Actors
{
    public class FloorManager : UntypedActor
    {
        private Dictionary<string, IActorRef> _floorIdToActorRefMap = new Dictionary<string, IActorRef>();

        public FloorManager()
        {
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RequestRegisterTemperatureSensor m:
                   if (_floorIdToActorRefMap.TryGetValue(m.FloorId, out var existingActor))
                   {
                       // Return ref if existing
                       existingActor.Forward(m);
                   }
                   else
                   {
                       // Create new floor actor and fwd registreaion message
                       var newFloorActor = Context.ActorOf(
                           Floor.Props(m.FloorId),
                           $"floor-{m.FloorId}");
                       Context.Watch(newFloorActor);
                       _floorIdToActorRefMap.Add(m.FloorId, newFloorActor);
                       newFloorActor.Forward(m);
                   }
                   break;
                case RequestFloorIds m:
                    Sender.Tell(new ResponseFloorIds(m.RequestId, _floorIdToActorRefMap.Keys.ToImmutableHashSet()));
                    break;
                case Terminated m:
                    var floorId = _floorIdToActorRefMap.First(x => x.Value == m.ActorRef).Key;
                    _floorIdToActorRefMap.Remove(floorId);
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }

        public static Props Props() => Akka.Actor.Props.Create(() => new FloorManager());
    }
}