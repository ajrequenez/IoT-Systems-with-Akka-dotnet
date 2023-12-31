﻿using Akka.Actor;
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
                case RequestTemperature m:
                    Sender.Tell(new ResponseTemperature(m.RequestId, _lastRecordedTemperature));
                    break;
                case RequestUpdateTemperature m:
                    _lastRecordedTemperature = m.Temperature;
                    Sender.Tell(new ResponseUpdateTemperature(m.RequestId));
                    break;
                case RequestRegisterTemperatureSensor m when
                        m.FloorId == _floorId && m.SensorId == _sensorId:
                    Sender.Tell(new ResponseRegisterTemperatureSensor(m.RequestId, Context.Self));
                    break;
                default:
                    Unhandled(message);
                    break;

            }
        }

        public static Props Props(string floorId, string sensorId) =>
            Akka.Actor.Props.Create(() => new TemperatureSensor(floorId, sensorId));
    }
}

