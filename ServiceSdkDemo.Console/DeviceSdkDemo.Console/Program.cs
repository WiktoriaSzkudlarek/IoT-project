using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices;
using ServiceSdkDemo.Console;
using System;

namespace ServiceSdkDemo.Console
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length == 3)
            {
                string deviceConnectionString = args[0];
                string deviceId = args[1];
                string opcConnectionString = args[2];

                System.Console.Title = $"Device{deviceId}";

                using var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
                await deviceClient.OpenAsync();
                var device = new VirtualDevice(deviceClient, deviceId, opcConnectionString);
                System.Console.WriteLine("Connection success");
                await device.InitializeHandlers();

                System.Console.WriteLine("Finished! Press key to close...");
                System.Console.ReadLine();
            }
            else
            {
                System.Console.WriteLine("Number of arguments is not equal 3");
            }
        }
    }
}


