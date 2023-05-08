using Azure.Storage.Blobs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ServiceSdkDemo;
using System.IO;
using System.Text.Json;

namespace BlobStorageDemo.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string connectionString;
        private readonly BlobServiceClient blobServiceClient;
        private readonly Dictionary<string, string[]> blobDict = new();

        public MainWindow()
        {
            InitializeComponent();
            ServiceSdkDemo.Console.Config? config = null;
            try
            {
                string text = File.ReadAllText("config.json");
                config = JsonSerializer.Deserialize<ServiceSdkDemo.Console.Config>(text);
                connectionString = config.StorageConnectionString;
                
            }
            catch
            {
                Console.WriteLine("There was a problem while reading config file.\nPress any key to close the program...");
                Console.ReadLine();
                Environment.Exit(1);
            }

            blobServiceClient = new BlobServiceClient(connectionString);
        }

        private async void onCreateContainerButtonClickAsync(object sender, RoutedEventArgs e)
        {
            string container = Guid.NewGuid().ToString();
            await blobServiceClient.CreateBlobContainerAsync(container);
        }

        private async void onRefreshListButtonClickAsync(object sender, RoutedEventArgs e)
        {
            containerList.Items.Clear();
            blobList.Items.Clear();
            blobDict.Clear();

            var containers = blobServiceClient.GetBlobContainersAsync();
            await foreach (var container in containers)
            {
                containerList.Items.Add(container.Name);

                var containerClient = blobServiceClient.GetBlobContainerClient(container.Name);
                var blobs = containerClient.GetBlobs().Select(i => i.Name).ToArray();
                blobDict.Add(container.Name, blobs);
            }
        }

        private async void onUploadButtonClickAsync(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            var fileName = openFileDialog.SafeFileName;
            var filePath = openFileDialog.FileName;

            var selectedContainer = containerList.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedContainer))
            {
                return;
            }
            var containerClient = blobServiceClient.GetBlobContainerClient(selectedContainer);
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(filePath, true);
        }

        private void onContainerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            blobList.Items.Clear();

            var selectedContainer = containerList.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedContainer))
            {
                return;
            }

            foreach (var blob in blobDict[selectedContainer])
            {
                blobList.Items.Add(blob);
            }
        }

        private async void onDownloadButtonClickAsync(object sender, RoutedEventArgs e)
        {
            var selectedContainer = containerList.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedContainer))
            {
                return;
            }

            var containerClient = blobServiceClient.GetBlobContainerClient(selectedContainer);

            var selectedBlob = blobList.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedBlob))
            {
                return;
            }

            var blobClient = containerClient.GetBlobClient(selectedBlob);

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = $"{selectedContainer}.json";
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            saveFileDialog.InitialDirectory = $"{path}\\Blobs";
            saveFileDialog.RestoreDirectory = true;
            Console.WriteLine(path);
            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            await blobClient.DownloadToAsync(saveFileDialog.FileName);
        }
    }
}