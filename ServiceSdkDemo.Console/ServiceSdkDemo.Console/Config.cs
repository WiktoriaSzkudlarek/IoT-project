using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceSdkDemo.Console
{
    public class Config
    {
        public string ServiceConnectionString { get; set; }
        public string OpcClientConnectionString {  get; set; }
        public string StorageConnectionString {  get; set; }
        
        public Device[] Devices { get; set; }

        public Blob[] Blobs { get; set; }
    }
    public class Device
    {
        public string DeviceId { get; set; }
        public string ConnectionString { get; set; }
    }
    public class Blob
    {
        public string BlobName { get; set; }
        public string Method { get; set; }
    }

    //
    public class ProductionKPI
    {
        public string Device { get; set; }
        public double Kpi { get; set;}
    }

    public class DeviceErrors
    {
        public int Count { get; set; }
        public string Device { get; set; }
    }
}
