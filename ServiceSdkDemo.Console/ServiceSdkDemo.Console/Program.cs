using Microsoft.Azure.Devices;
using ServiceSdkDemo.Console;
using ServiceSdkDemo.Lib;
using System;
using System.IO;
using System.Text.Json;

string text = File.ReadAllText("config.json");
var config = JsonSerializer.Deserialize<Config>(text);

using var serviceClient = ServiceClient.CreateFromConnectionString(config.ServiceConnectionString);
using var registryManager = RegistryManager.CreateFromConnectionString(config.ServiceConnectionString);

var manager = new IoTHubManager(serviceClient, registryManager);

int input;
do
{
    FeatureSelector.PrintMenu();
    input = FeatureSelector.ReadInput();
    await FeatureSelector.Execute(input, manager);
} while (input != 0);

