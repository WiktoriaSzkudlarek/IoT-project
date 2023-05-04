using Microsoft.Azure.Devices.Common.Exceptions;

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
    }
}
