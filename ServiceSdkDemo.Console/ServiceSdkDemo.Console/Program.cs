using Microsoft.Azure.Devices;
using ServiceSdkDemo.Console;
using ServiceSdkDemo.Lib;
using System;
using System.IO;
using System.Text.Json;
using Opc.UaFx;
using Opc.UaFx.Client;
using Microsoft.Azure.Devices.Client;

string text = File.ReadAllText("config.json");
var config = JsonSerializer.Deserialize<Config>(text);

using var serviceClient = ServiceClient.CreateFromConnectionString(config.ServiceConnectionString);
using var registryManager = RegistryManager.CreateFromConnectionString(config.ServiceConnectionString);

var manager = new IoTHubManager(serviceClient, registryManager);

string opcClientConnectionString = config.OpcClientConnectionString;

using var device1Client = DeviceClient.CreateFromConnectionString(config.Device1ConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
using var device2Client = DeviceClient.CreateFromConnectionString(config.Device2ConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
await device1Client.OpenAsync();
await device2Client.OpenAsync();
var device1 = new VirtualDevice(device1Client, 1, opcClientConnectionString);
var device2 = new VirtualDevice(device2Client, 2, opcClientConnectionString);

int input;

/*
do
{
System.Console.WriteLine("Select device");
input = FeatureSelector.ReadInput();

FeatureSelector.PrintMenu();
input = FeatureSelector.ReadInput();
await FeatureSelector.Execute(input, manager);
*/
    for (int i = 0; i < 3; i++)
    {
        await device1.SendTelemetry();
    }

    await device1.EmergencyStop();

//} while (input != 0);

