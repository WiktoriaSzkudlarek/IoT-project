using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices;
using ServiceSdkDemo.Console;
using System;
using System.Threading;
using System.Diagnostics;

namespace ServiceSdkDemo.Console
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length == 4)
            {
                string deviceConnectionString = args[0];
                string deviceId = args[1];
                string opcConnectionString = args[2];
                string mainProcessName = args[3];

                System.Console.Title = $"Device{deviceId}";


                using var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
                await deviceClient.OpenAsync();

                var device = new VirtualDevice(deviceClient, deviceId, opcConnectionString);
                System.Console.WriteLine("Connection success");
                await device.InitializeHandlers();


                var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                while (await timer.WaitForNextTickAsync())
                {
                    if (Process.GetProcessesByName(mainProcessName).Length == 0)
                        Environment.Exit(1);

                    try
                    {
                        System.Console.WriteLine(DateTime.Now);
                        await device.SendTelemetry();
                    }
                    catch
                    {
                        System.Console.WriteLine("Cannot send Telemetry data.\nPress enter to close the program...");
                        System.Console.ReadLine();
                        Environment.Exit(1);
                    }
                }


                System.Console.WriteLine("Finished! Press key to close...");
                System.Console.ReadLine();

            }
            else
            {
                System.Console.WriteLine("Number of arguments is not equal 4");
                System.Console.ReadLine();
            }
        }
    }
}


