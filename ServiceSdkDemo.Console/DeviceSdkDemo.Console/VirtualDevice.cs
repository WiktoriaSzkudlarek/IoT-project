using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;
using Opc.UaFx;
using Opc.UaFx.Client;
using Microsoft.Azure.Amqp.Framing;

namespace ServiceSdkDemo.Console
{
    public class VirtualDevice
    {
        private readonly DeviceClient deviceClient;
        private readonly string deviceNumber;
        private readonly string opcClient;

        public VirtualDevice(DeviceClient deviceClient, string deviceNumber, string opcClient)
        {
            this.deviceClient = deviceClient;
            this.deviceNumber = deviceNumber;
            this.opcClient = opcClient;
        }

        #region Sending Messages
        public async Task SendMessages(int nrOfMessages, int delay)
        {
            var rnd = new Random();

            System.Console.WriteLine($"Device sending {nrOfMessages} messages to IoTHub...\n");
            for (int i = 0; i < nrOfMessages; i++)
            {
                var data = new
                {
                    temperature = rnd.Next(20, 35),
                    humidity = rnd.Next(60, 80),
                    msgCount = i,
                };

                var dataString = JsonConvert.SerializeObject(data);

                Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataString));
                eventMessage.ContentType = MediaTypeNames.Application.Json;
                eventMessage.ContentEncoding = "utf-8";
                eventMessage.Properties.Add("temperatureAlert", (data.temperature > 30) ? "true" : "false");
                System.Console.WriteLine($"\t{DateTime.Now.ToLocalTime()}> Sending message: {i}, Data: [{dataString}]");


                await deviceClient.SendEventAsync(eventMessage);

                if (i < nrOfMessages - 1)
                    await Task.Delay(delay);
            }
            System.Console.WriteLine();
        }

        // Send Telemetry
        public async Task SendTelemetry()
        {
            var client = new OpcClient(opcClient);
            client.Connect();

            var productionStatus = client.ReadNode($"ns=2;s=Device {deviceNumber}/ProductionStatus").Value;
            //System.Console.WriteLine(productionStatus);
            var workorderId = client.ReadNode($"ns=2;s=Device {deviceNumber}/WorkorderId").Value;
            //System.Console.WriteLine(workorderId);
            var goodCount = client.ReadNode($"ns=2;s=Device {deviceNumber}/GoodCount").Value;
            //System.Console.WriteLine(goodCount);
            var badCount = client.ReadNode($"ns=2;s=Device {deviceNumber}/BadCount").Value;
            //System.Console.WriteLine(badCount);
            var temperature = client.ReadNode($"ns=2;s=Device {deviceNumber}/Temperature").Value;
            //System.Console.WriteLine(temperature);

            var deviceErrors = client.ReadNode($"ns=2;s=Device {deviceNumber}/DeviceErrors").Value;
            //System.Console.WriteLine(deviceErrors);
            var productionRate = client.ReadNode($"ns=2;s=Device {deviceNumber}/ProductionRate").Value;
            //System.Console.WriteLine(productionRate);

            var data = new
            {
                device = $"Device{deviceNumber}",
                productionStatus = productionStatus,
                workorderId = workorderId,
                goodCount = goodCount,
                badCount = badCount,
                temperature = temperature,
            };

            var dataString = JsonConvert.SerializeObject(data);

            Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataString));
            eventMessage.ContentType = MediaTypeNames.Application.Json;
            eventMessage.ContentEncoding = "utf-8";

            System.Console.WriteLine($"\t{DateTime.Now.ToLocalTime()}> Sending data: [{dataString}]");

            try
            {
                if (deviceClient != null)
                {
                    await deviceClient.SendEventAsync(eventMessage);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                System.Console.WriteLine($"Exception while sending event: {ex.Message}");
            }


            await UpdateTwinAsync("deviceErrors", DateTime.Now);
            await UpdateTwinAsync("productionRate", DateTime.Now);

            client.Disconnect();
            await Task.Delay(1000);
        }
        #endregion
        #region Receive Messages
        private async Task OnC2dMessageReceivedAsync(Message receivedMessage, object _)
        {
            System.Console.WriteLine($"\t{DateTime.Now}> C2D message callback - message received with Id={receivedMessage.MessageId}");
            PrintMessage(receivedMessage);
            await deviceClient.CompleteAsync(receivedMessage);
            System.Console.WriteLine($"\t{DateTime.Now}> Completed C2D message with Id={receivedMessage.MessageId}.");

            receivedMessage.Dispose();
        }

        private void PrintMessage(Message receivedMessage)
        {
            string messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
            System.Console.WriteLine($"\t\tReceived message: {messageData}");

            int propCount = 0;
            foreach (var prop in receivedMessage.Properties)
            {
                System.Console.WriteLine($"\t\tProperty[{propCount++}> Key={prop.Key} : Value={prop.Value}]");
            }
        }
        #endregion
        #region Direct Methods

        private async Task<MethodResponse> DefaultServiceHandler(MethodRequest methodRequest, object userContext)
        {
            System.Console.WriteLine($"\tMETHOD EXECUTED: {methodRequest.Name}");

            await Task.Delay(1000);

            return new MethodResponse(0);
        }
        private async Task<MethodResponse> SendMessagesHandler(MethodRequest methodRequest, object userContext)
        {
            System.Console.WriteLine($"\tMETHOD EXECUTED: {methodRequest.Name}");

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, new { nrOfMessages = default(int), delay = default(int) });
            await SendMessages(payload.nrOfMessages, payload.delay);

            return new MethodResponse(0);
        }
        // Send Telemetry
        private async Task<MethodResponse> SendTelemetryHandler(MethodRequest methodRequest, object userContext)
        {
            System.Console.WriteLine($"\tMETHOD EXECUTED: {methodRequest.Name}");

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, new { machineId = default(string) });
            await SendTelemetry();

            return new MethodResponse(0);
        }

        // Emergency Stop
        public async Task EmergencyStop()
        {
            var client = new OpcClient(opcClient);
            client.Connect();

            System.Console.WriteLine($"\tShutting down Device {deviceNumber}\n");
            client.CallMethod($"ns=2;s=Device {deviceNumber}", $"ns=2;s=Device {deviceNumber}/EmergencyStop");
            client.WriteNode($"ns=2;s=Device {deviceNumber}/ProductionRate", OpcAttribute.Value, 0);

            client.Disconnect();
            await Task.Delay(1000);

        }
        private async Task<MethodResponse> EmergencyStopHandler(MethodRequest methodRequest, object userContext)
        {
            System.Console.WriteLine($"\tEXECUTED METHOD: {methodRequest.Name}");

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, new { machineId = default(string) });

            await EmergencyStop();

            return new MethodResponse(0);
        }

        // Reset Error Status
        public async Task ResetErrorStatus()
        {
            var client = new OpcClient(opcClient);
            client.Connect();
            client.CallMethod($"ns=2;s=Device {deviceNumber}", $"ns=2;s=Device {deviceNumber}/ResetErrorStatus");
            
            client.Disconnect();
            await Task.Delay(1000);
        }
        private async Task<MethodResponse> ResetErrorStatusHandler(MethodRequest methodRequest, object userContext)
        {
            System.Console.WriteLine($"\tEXECUTED METHOD: {methodRequest.Name}");

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, new { machineId = default(string) });

            await ResetErrorStatus();

            return new MethodResponse(0);
        }

        #endregion
        #region Device Twin
        public async Task UpdateTwinAsync(string value, DateTime time)
        {
            var twin = await deviceClient.GetTwinAsync();

            var reportedProperties = new TwinCollection();
            reportedProperties[value] = time;

            await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
        }

        private async Task onDesiredPropertyChanged(TwinCollection desiredProperties, object _)
        {
            System.Console.WriteLine($"\tDesired property change:\n\t {JsonConvert.SerializeObject(desiredProperties)}");
            System.Console.WriteLine("\tSending current time as reported property");
            var reportedProperties = new TwinCollection();
            reportedProperties["ProductionRate"] = desiredProperties["ProductionRate"];

            var client = new OpcClient(opcClient);
            client.Connect();

            int ProductionRateChanged = desiredProperties["ProductionRate"];
            System.Console.WriteLine(ProductionRateChanged);
            client.WriteNode($"ns=2;s=Device {deviceNumber}/ProductionRate", ProductionRateChanged);

            client.Disconnect();

            await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
        }
        //
        #endregion

        public async Task InitializeHandlers()
        {
            await deviceClient.SetReceiveMessageHandlerAsync(OnC2dMessageReceivedAsync, deviceClient);

            await deviceClient.SetMethodDefaultHandlerAsync(DefaultServiceHandler, deviceClient);
            await deviceClient.SetMethodHandlerAsync("SendMessages", SendMessagesHandler, deviceClient);
            await deviceClient.SetMethodHandlerAsync("SendTelemetry", SendTelemetryHandler, deviceClient);
            await deviceClient.SetMethodHandlerAsync("EmergencyStop", EmergencyStopHandler, deviceClient);
            await deviceClient.SetMethodHandlerAsync("ResetErrorStatus", ResetErrorStatusHandler, deviceClient);

            await deviceClient.SetDesiredPropertyUpdateCallbackAsync(onDesiredPropertyChanged, deviceClient);
        }

    }
}
