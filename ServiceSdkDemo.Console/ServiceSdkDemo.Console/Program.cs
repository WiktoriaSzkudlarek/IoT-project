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

List<VirtualDevice> deviceClients = new List<VirtualDevice>();

string projectPath = "";
string opcClientConnectionString = "";

try
{
    string text = File.ReadAllText("config.json");
    config = JsonSerializer.Deserialize<Config>(text);
    opcClientConnectionString = config.OpcClientConnectionString;
}
catch
{
    Console.WriteLine("There was a problem while reading config file.\nPress any key to close the program...");
    CloseStartedProcesses();
    Console.ReadLine();
    Environment.Exit(1);
}
try
{
    projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
}
catch
{
    Console.WriteLine("The device.exe probably doesn't exist.\nPress any key to close the program...");
    CloseStartedProcesses();
    Console.ReadLine();
    Environment.Exit(1);
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
    CloseStartedProcesses();
    Console.ReadLine();
    Environment.Exit(1);
}

foreach (var device in config.Devices)
{
    VirtualDevice? virtualDevice = null;
    try
    {
        string deviceExePath = $"{projectPath}\\DeviceSdkDemo.Console\\bin\\Debug\\net6.0\\DeviceSdkDemo.Console.exe";
        string procArgs = $"{device.ConnectionString} {device.DeviceId} {opcClientConnectionString} {Process.GetCurrentProcess().ProcessName}";

        using Process process = new();
        process.StartInfo.FileName = deviceExePath;
        process.StartInfo.Arguments = procArgs;
        process.StartInfo.UseShellExecute = true;
        process.Start();

    }
    catch
    {
        Console.WriteLine($"Could not connect to: Device{device.DeviceId}\nPress any key to close the program...");
        CloseStartedProcesses();
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

CloseStartedProcesses();
Console.WriteLine("\nHub and Devices are terminated now. Press enter to close the program...");
Console.ReadLine();

///
void CloseStartedProcesses()
{
    Process[] workers = Process.GetProcessesByName("DeviceSdkDemo.Console");
    foreach (Process worker in workers)
    {
        worker.Kill();
        worker.WaitForExit();
        worker.Dispose();
    }
}

