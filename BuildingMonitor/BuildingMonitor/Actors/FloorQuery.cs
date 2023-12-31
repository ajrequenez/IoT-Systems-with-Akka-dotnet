﻿using System.Collections.Immutable;
using Akka.Actor;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;

namespace BuildingMonitor.Actors
{
	public class FloorQuery : UntypedActor
	{
		public static readonly long TemperatureRequestCorrelationId = 42;
        private ICancelable _queryTimeoutTimer;
        private readonly IActorRef _requester;
        private readonly long _requestId;
        private readonly TimeSpan _timeout;
        private readonly Dictionary<IActorRef, string> _actorToSensorId;

        private Dictionary<string, ITemperatureQueryResult> _responsesReceived = new Dictionary<string, ITemperatureQueryResult>();
        private HashSet<IActorRef> _stillAwaitingResponse;

        public FloorQuery(Dictionary<IActorRef, string> actorToSensorId, 
                long requestId, 
                IActorRef requester, 
                TimeSpan timeout)
        {
            _actorToSensorId = actorToSensorId;
            _requestId = requestId;
            _requester = requester;
            _timeout = timeout;

            _stillAwaitingResponse = new HashSet<IActorRef>(_actorToSensorId.Keys);

            _queryTimeoutTimer = Context.System.Scheduler.ScheduleTellOnceCancelable(timeout, Self, QueryTimeout.Instance, Self);
        }

        protected override void PreStart() 
        {
            foreach(var sensor in _actorToSensorId.Keys)
            {
                Context.Watch(sensor);
                sensor.Tell(new RequestTemperature(TemperatureRequestCorrelationId), Self);
            }
        }

        protected override void PostStop()
        {
            _queryTimeoutTimer.Cancel();
        }
        protected override void OnReceive(object message)
        {
            switch(message) {
                case ResponseTemperature m when m.RequestId == TemperatureRequestCorrelationId:
                    ITemperatureQueryResult reading = NoTemperatureReadingRecordedYet.Instance;
                    if (m.Temperature.HasValue)
                    {
                        reading = new TemperatureAvailable(m.Temperature.Value);
                    }
                    RecordSensorResponse(Sender, reading);
                    break;
                case Terminated m:
                    RecordSensorResponse(m.ActorRef, TemperatureSensorNotAvailable.Instance);
                    break;
                case QueryTimeout m:
                    foreach(var sensor in _stillAwaitingResponse)
                    {
                        var sensorId = _actorToSensorId[sensor];
                        _responsesReceived.Add(sensorId, TemperatureSensorTimedOut.Instance);
                    }
                    _requester.Tell(new ResponseAllTemperatures(_requestId, _responsesReceived.ToImmutableDictionary()));
                    Context.Stop(Self);
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }

        public static Props Props(Dictionary<IActorRef, string> actorToSensorId, 
                long requestId, 
                IActorRef requester, 
                TimeSpan timeout) =>
            Akka.Actor.Props.Create(() => new FloorQuery(actorToSensorId, requestId, requester, timeout));

        private void RecordSensorResponse(IActorRef sensorActor, ITemperatureQueryResult reading)
        {
            Context.Unwatch(sensorActor);

            var sensorId = _actorToSensorId[sensorActor];
            _stillAwaitingResponse.Remove(sensorActor);
            _responsesReceived.Add(sensorId, reading);

            if(_stillAwaitingResponse.Count == 0)
            {
                _requester.Tell(new ResponseAllTemperatures(_requestId, _responsesReceived.ToImmutableDictionary()));
                Context.Stop(Self);
            }

        }
    }
}

