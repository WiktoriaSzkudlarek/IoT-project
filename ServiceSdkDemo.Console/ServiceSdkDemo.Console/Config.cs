using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceSdkDemo.Lib
{
    public class Config
    {
        public string ServiceConnectionString { get; set; }
        public string OpcClientConnectionString {  get; set; }
        public Device[] Devices { get; set; }
    }
    public class Device
    {
        public string DeviceId { get; set; }
        public string ConnectionString { get; set; }
    }
}
