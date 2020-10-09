using System;
using System.Collections.Generic;
using System.Text;
using WinPidKiller.Models;

namespace WinPidKiller.Services
{
    interface IProcessInfoService
    {
        List<ProcessInfo> GetAllProcessInfo();

        List<ProcessInfo> GetAllProcessInfo(String port);

        void KillProcess(List<string> pidList);
    }
}
