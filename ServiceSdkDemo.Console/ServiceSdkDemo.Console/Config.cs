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
        public string Device1ConnectionString {  get; set; }
        public string Device2ConnectionString {  get; set; }
    }
}
