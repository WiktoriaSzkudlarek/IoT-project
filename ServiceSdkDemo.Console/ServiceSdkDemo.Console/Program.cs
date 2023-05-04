using Microsoft.Azure.Devices;
using ServiceSdkDemo.Console;
using ServiceSdkDemo.Lib;
using System;
using System.IO;
using System.Text.Json;
using Opc.UaFx;
using Opc.UaFx.Client;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Diagnostics;
using System.Security.Cryptography;

Config? config = null;

ServiceClient? serviceClient = null;
RegistryManager? registryManager = null;

IoTHubManager? manager = null;

string projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
try
{
    string text = File.ReadAllText("config.json");
    config = JsonSerializer.Deserialize<Config>(text);
}
catch
{
    Console.WriteLine("There was a problem while reading config file.");
}
try
{
    serviceClient = ServiceClient.CreateFromConnectionString(config.ServiceConnectionString);
    registryManager = RegistryManager.CreateFromConnectionString(config.ServiceConnectionString);

    manager = new IoTHubManager(serviceClient, registryManager);
}
catch
{
    Console.WriteLine("There was a problem while connecting to the Azure platform.\nPress any key to close the program...");
    Console.ReadLine();
    Environment.Exit(1);
}

string opcClientConnectionString = config.OpcClientConnectionString;

List<VirtualDevice> deviceClients = new List<VirtualDevice>();
List<int> deviceProcesses = new List<int>();
foreach (var device in config.Devices)
{
    VirtualDevice? virtualDevice = null;
    try
    {
        string deviceExePath = $"{projectPath}\\DeviceSdkDemo.Console\\bin\\Debug\\net6.0\\DeviceSdkDemo.Console.exe";
        string procArgs = $"{device.ConnectionString} {device.DeviceId} {opcClientConnectionString}";

        using Process process = new();
        process.StartInfo.FileName = deviceExePath;
        process.StartInfo.Arguments = procArgs;
        process.StartInfo.UseShellExecute = true;
        process.Start();

        deviceProcesses.Add(process.Id);
    }
    catch
    {
        Console.WriteLine($"Could not connect to: Device{device.DeviceId}\nPress any key to close the program...");
        Console.ReadLine();
        Environment.Exit(1);
    }
    finally
    {
        if (virtualDevice != null)
            deviceClients.Add(virtualDevice);
    }
}

int input;
do
{
    FeatureSelector.PrintMenu();
    input = FeatureSelector.ReadInput();
    await FeatureSelector.Execute(input, manager);
} while (input != 0);

foreach (var pid in deviceProcesses)
{
    Process proc = Process.GetProcessById(pid);
    proc.Kill();
}
Console.WriteLine("\nHub and Devices are terminated now. Press enter to close the program...");


Console.ReadLine();