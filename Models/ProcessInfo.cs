using System;
using System.Collections.Generic;
using System.Text;

namespace WinPidKiller.Models
{
    class ProcessInfo
    {
        public string Name { get; set; }
        public string Pid { get; set; }
        public string AgreeMent { get; set; }
        public string LocalIp { get; set; }
        public string RemoteIp { get; set; }
    }
}
