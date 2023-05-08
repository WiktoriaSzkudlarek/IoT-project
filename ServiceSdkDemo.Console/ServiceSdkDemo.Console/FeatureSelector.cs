using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace ServiceSdkDemo.Console
{
    internal static class FeatureSelector
    {

        public static void PrintMenu()
        {
            System.Console.ForegroundColor = ConsoleColor.DarkCyan;
            System.Console.WriteLine(@"
1 - C2D
2 - Direct Method
3 - Device Twin
4 - 'Business Logic'
0 - Exit");
            System.Console.ResetColor();
            System.Console.Write("$>");
        }

        public static async Task Execute(int feature, IoTHubManager manager)
        {

            switch (feature)
            {
                case 1:
                    {
                        System.Console.Write("\nType your message (confirm with enter):\n$>");
                        string messageText = System.Console.ReadLine() ?? string.Empty;

                        System.Console.Write("\nType your device id (confirm with enter):\n$>");
                        string deviceId = System.Console.ReadLine() ?? string.Empty;

                        await manager.SendMessage(messageText, deviceId);
                    }
                    break;
                case 2:
                    {
                        System.Console.Write("\nType your device id (confirm with enter):\n$>");
                        string deviceId = System.Console.ReadLine() ?? string.Empty;

                        System.Console.ForegroundColor = ConsoleColor.DarkCyan;
                        System.Console.WriteLine(@"Choose Method (Emergency Stop is default, hehe):
1 - Emergency Stop
2 - Reset Error Status
3 - Reduce Production Rate
4 - Send Telemetry");
                        System.Console.ResetColor();
                        System.Console.Write("$>");
                        string method;
                        switch (Convert.ToInt32(System.Console.ReadLine()))
                        {
                            case 1:
                            default:
                                method = "EmergencyStop";
                                break;
                            case 2:
                                method = "ResetErrorStatus";
                                break;
                            case 3:
                                method = "ReduceProductionRate";
                                break;
                            case 4:
                                method = "SendTelemetry";
                                break;

                        }

                        try
                        {
                            var result = await manager.ExecuteDeviceMethod(method, deviceId);
                            System.Console.WriteLine($"Method executed with status {result}");
                        }
                        catch (DeviceNotFoundException e)
                        {
                            System.Console.WriteLine($"Device not connected! \n{e.Message}");
                        }
                    }
                    break;
                case 3:
                    {
                        System.Console.Write("\nType your device id (confirm with enter):\n$>");
                        string deviceId = System.Console.ReadLine() ?? string.Empty;

                        System.Console.Write("\nType your property name (confirm with enter):\n$>");
                        string propertyName = System.Console.ReadLine() ?? string.Empty;

                        var random = new Random();
                        await manager.UpdateDesiredTwin(deviceId, propertyName, random.Next());
                    }
                    break;
                    case 4:
                    {
                        BusinessLogic(manager);
                    }
                    break;
                default:
                    break;
            }
        }

        internal static int ReadInput()
        {
            var keyPressed = System.Console.ReadKey();
            var isParsed = int.TryParse(keyPressed.KeyChar.ToString(), out var value);
            return isParsed ? value : -1;
        }

        static async Task BusinessLogic(IoTHubManager manager)
        {
            ProductionKPI? productionKpi = null;
            DeviceErrors? deviceErrors = null;
            Config config = JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json"));

            List<Blob> blobs = new List<Blob>();
            foreach(var blob in config.Blobs)
            {
                blobs.Add(blob);
            }

            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            try
            {
                

                string line = File.ReadLines($"{path}\\Blobs\\{blobs[0].BlobName}").First();
                productionKpi = JsonSerializer.Deserialize<ProductionKPI>(line);
                if( productionKpi.Kpi < 90.0)
                {
                    try
                    {
                        var result = await manager.ExecuteDeviceMethod(blobs[0].Method, productionKpi.Device);
                        System.Console.WriteLine($"Method {blobs[0].Method} executed with status {result}");
                    }
                    catch (DeviceNotFoundException e)
                    {
                        System.Console.WriteLine($"Device not connected! \n{e.Message}");
                    }
                }

                line = File.ReadLines($"{path}\\Blobs\\{blobs[1].BlobName}").First();
                deviceErrors = JsonSerializer.Deserialize<DeviceErrors>(line);
                if (deviceErrors.Count > 3)
                {
                    try
                    {
                        var result = await manager.ExecuteDeviceMethod(blobs[1].Method, deviceErrors.Device);
                        System.Console.WriteLine($"\nMethod: {blobs[1].Method} executed with status {result}");
                    }
                    catch (DeviceNotFoundException e)
                    {
                        System.Console.WriteLine($"Device not connected! \n{e.Message}");
                    }
                }
            }
            catch(Exception ex) 
            {
                System.Console.WriteLine(ex);
                System.Console.WriteLine("There was a problem while reading blob files.\nPress any key to close the program...");
            }
            PrintMenu();
        }
    }
}
