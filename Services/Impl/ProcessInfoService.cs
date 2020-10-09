using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using WinPidKiller.Models;

namespace WinPidKiller.Services.Impl
{
    class ProcessInfoService : IProcessInfoService
    {
        /**
         * 若port为空则获取所有进程信息
         * 若port不为空则获取占用port的线程
         */
        public List<ProcessInfo> GetAllProcessInfo(String port)
        {
            List<ProcessInfo> processInfoList = new List<ProcessInfo>();

            // 拿到所有进程
            Dictionary<int, Process> processMap = GetAllProcess();

            List<NetworkInfo> networkInfos = null;
            if (!(string.IsNullOrEmpty(port)))
            {
                // 根据port查询出对应的端口信息，展示对应进程信息
                networkInfos = GetPortInfo(port);
            } else
            {
                networkInfos = GetPortInfo(); 
            }

            foreach (NetworkInfo networkInfo in networkInfos)
            {
                ProcessInfo processInfo = new ProcessInfo();

                int.TryParse(networkInfo.Pid, out int pid);
                Process process = processMap[pid];

                processInfo.Name = process.ProcessName;
                processInfo.Pid = process.Id.ToString();
                processInfo.AgreeMent = networkInfo.AgreeMent;
                processInfo.LocalIp = networkInfo.LocalIp;
                processInfo.RemoteIp = networkInfo.RemoteIp;

                processInfoList.Add(processInfo);
            }

            return processInfoList;
        }

        public List<ProcessInfo> GetAllProcessInfo()
        {
            return GetAllProcessInfo(null);
        }

        /**
         * 根据pid列表杀死所有进程
         */
        public void KillProcess(List<string> pidList)
        {
            if (pidList == null || pidList.Count == 0)
            {
                MessageBox.Show("请选择正确的进程号");
                return;
            }

            Dictionary<int, Process> processMap = GetAllProcess();

            StringBuilder sb = new StringBuilder();
            foreach (var pidStr in pidList)
            {
                int.TryParse(pidStr, out int pid);
                Process process = processMap[pid];
                try
                {
                    process.Kill();
                    sb.Append("已杀掉");
                    sb.Append(process.ProcessName);
                    sb.Append("进程！！！");
                }
                catch (Win32Exception e)
                {
                    sb.Append(process.ProcessName);
                    sb.Append(e.Message.ToString());
                }
                catch (InvalidOperationException e)
                {
                    sb.Append(process.ProcessName);
                    sb.Append(e.Message.ToString());
                }
            }

            MessageBox.Show(sb.ToString());
        }

        private Dictionary<int, Process> GetAllProcess()
        {
            Process[] processes = Process.GetProcesses();
            return processes.ToDictionary(key => key.Id, process => process);
        }

        private List<NetworkInfo> GetPortInfo()
        {
            return GetPortInfo(null);
        }

        /**
         * 通过端口取出所有相关的数据
         */
        private List<NetworkInfo> GetPortInfo(string port)
        {
            List<NetworkInfo> networkInfoList = new List<NetworkInfo>();
            Process process = CreateCmd();
            process.Start();

            if (string.IsNullOrEmpty(port))
            {
                process.StandardInput.WriteLine(string.Format("netstat -ano"));
            } else
            {
                process.StandardInput.WriteLine(string.Format("netstat -ano|find \"{0}\"", port));
            }
           
            process.StandardInput.WriteLine("exit");
            StreamReader reader = process.StandardOutput;
            string strLine = reader.ReadLine();
            while (!reader.EndOfStream)
            {
                strLine = strLine.Trim();
                if (strLine.Length > 0 && ((strLine.Contains("TCP") || strLine.Contains("UDP"))))
                {
                    Regex r = new Regex(@"\s+");
                    string[] strArr = r.Split(strLine);
                    // 解析数据格式为 TCP   0.0.0.0:135    0.0.0.0:0   LISTENING   692
                    int defaultResultLength = 5;
                    if (strArr.Length == defaultResultLength)
                    {
                        NetworkInfo networkInfo = new NetworkInfo();
                        // 只拿第一行数据，拿完就撤（每个PID展示一个port就行）
                        networkInfo.AgreeMent = strArr[0];
                        networkInfo.LocalIp = strArr[1];
                        networkInfo.RemoteIp = strArr[2];
                        networkInfo.Pid = strArr[4];

                        networkInfoList.Add(networkInfo);
                    }
                }
                strLine = reader.ReadLine();
            }
            reader.Close();
            process.Close();
            return networkInfoList;
        }

        /**
         * 创建cmd控件
         */
        private Process CreateCmd()
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            return process;
        }

    }

}
