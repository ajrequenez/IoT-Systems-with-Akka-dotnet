using System;
using Akka.Actor;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;

namespace ConsoleAppHost
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting actor system...");

            using (var system = ActorSystem.Create("building-iot-system"))
            {
                Console.WriteLine("     Floor Manager: Creating....");
                IActorRef floorManager = system.ActorOf(Props.Create<FloorManager>(), "floor-manager");
                Console.WriteLine("     Floor Manager: Created....");

                await CreateSimulatedSensors(floorManager);

                while (true)
                {
                    Console.WriteLine("Press enter to query, Q to quit");
                    var cmd = Console.ReadLine();

                    if (cmd?.ToUpperInvariant() == "Q")
                    {
                        Console.WriteLine("Shutting Down...");
                        Environment.Exit(0);
                    }
                    await DisplayTemperatures(system);
                }
            }
        }


        private static async Task CreateSimulatedSensors(IActorRef floorManager)
        {
            for (int simulatedSensorId = 0; simulatedSensorId < 10; simulatedSensorId++)
            {
                Console.WriteLine($"     Sensor{simulatedSensorId}: Creating....");
                var newSensor = new SimulatedSensor("basement", $"{simulatedSensorId}", floorManager);
                await newSensor.Connect();
                Console.WriteLine($"     Sensor{simulatedSensorId}: Connected....");

                var simulateNoReadingYet = simulatedSensorId == 3;

                if (!simulateNoReadingYet)
                {
                    newSensor.StartSendingSimulatedReadings();
                }
            }
        }

        private static async Task DisplayTemperatures(ActorSystem actorSystem)
        {
            var temps = await actorSystem.ActorSelection("akka://building-iot-system/user/floor-manager/floor-basement")
                                    .Ask<ResponseAllTemperatures>(new RequestAllTemperatures(0));
            Console.Clear();

            foreach (var temp in temps.Temperatures)
            {
                Console.Write($"Sensor {temp.Key} {temp.Value.GetType().Name}");
                if (temp.Value is TemperatureAvailable)
                {
                    Console.Write($" {((TemperatureAvailable)temp.Value).Temperature:00.00}");
                }
                Console.WriteLine("                   ");
            }
        }
    }
}
