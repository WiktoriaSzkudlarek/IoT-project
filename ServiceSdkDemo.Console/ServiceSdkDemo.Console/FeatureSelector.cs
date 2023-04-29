using Microsoft.Azure.Devices.Common.Exceptions;

namespace ServiceSdkDemo.Console
{
    internal static class FeatureSelector
    {

        public static void PrintMenu()
        {
            System.Console.WriteLine(@"
1 - C2D
2 - Direct Method
3 - Device Twin
0 - Exit");
        }

        public static async Task Execute(int feature, IoTHubManager manager) 
        { 
            switch(feature)
            {
                case 1:
                    {
                        System.Console.WriteLine("\nType your message (confirm with enter):");
                        string messageText = System.Console.ReadLine() ?? string.Empty;

                        System.Console.WriteLine("\nType your device id (confirm with enter):");
                        string deviceId = System.Console.ReadLine() ?? string.Empty;

                        await manager.SendMessage(messageText, deviceId);
                    }
                    break;
                case 2:
                    {
                        System.Console.WriteLine("\nType your device id (confirm with enter):");
                        string deviceId = System.Console.ReadLine() ?? string.Empty;
                        try
                        {
                            var result = await manager.ExecuteDeviceMethod("SendMessages", deviceId);
                            System.Console.WriteLine($"Method executed with status {result}");
                        }
                        catch(DeviceNotFoundException e)
                        {
                            System.Console.WriteLine($"Device not connected! \n{e.Message}");
                        }
                    }
                    break;
                case 3:
                    {
                        System.Console.WriteLine("\nType your device id (confirm with enter):");
                        string deviceId = System.Console.ReadLine() ?? string.Empty;

                        System.Console.WriteLine("\nType your property name (confirm with enter):");
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
