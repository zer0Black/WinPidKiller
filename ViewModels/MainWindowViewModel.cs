using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Text;
using WinPidKiller.Models;
using WinPidKiller.Services;
using WinPidKiller.Services.Impl;

namespace WinPidKiller.ViewModels
{
    /**
     * 做双向绑定，port提供查询框用，processInfo列表提供dataGrid用
     */
    class MainWindowViewModel : BindableBase
    {
        private int port;
        public int Port
        {
            get { return port; }
            set { 
                port = value;
                SetProperty(ref port, value);
            }
        }

        /**
         * 如果这个DataList列表的内容需要同步刷新，
         * 则类型必须是ObservableCollection。
         * 否则就算控件与数据绑定成功，控件只在初始化时能够正确显示数据，
         * 之后数据发生改变时，控件不会自动刷新。
         */
        private ObservableCollection<ProcessItemViewModel> processItemList;
        public ObservableCollection<ProcessItemViewModel> ProcessItemList
        {
            get { return processItemList; }
            set {
                processItemList = value;
                SetProperty(ref processItemList, value);
            }
        }

        public MainWindowViewModel()
        {
            // 加载数据
            LoadProcessInfo();

            QueryPortCommand = new DelegateCommand(new Action(QueryPortCommandExec));
            KillCommand = new DelegateCommand(new Action(KillCommandExec));
            RefreshCommand = new DelegateCommand(new Action(RefreshCommandExec));
        }

        private void LoadProcessInfo()
        {
            IProcessInfoService processInfoService = new ProcessInfoService();
            processItemList = new ObservableCollection<ProcessItemViewModel>();
            processItemList.AddRange(GetProcessItemViewModel(processInfoService.GetAllProcessInfo())); 
        }

        // 绑定检索命令 和 kill命令
        public DelegateCommand QueryPortCommand { get; set; }
        public DelegateCommand KillCommand { get; set; }
        public DelegateCommand RefreshCommand { get; set; }

        private void QueryPortCommandExec()
        {
            IProcessInfoService processInfoService = new ProcessInfoService();
            processItemList.Clear();
            processItemList.AddRange(GetProcessItemViewModel(processInfoService.GetAllProcessInfo(port.ToString())));
        }

        private void RefreshCommandExec()
        {
            IProcessInfoService processInfoService = new ProcessInfoService();
            processItemList.Clear();
            processItemList.AddRange(GetProcessItemViewModel(processInfoService.GetAllProcessInfo()));
        }

        private void KillCommandExec()
        {
            List<String> pidList = new List<string>();
            foreach (var processItem in processItemList)
            {
                if (processItem.SelectItem) 
                {
                    pidList.Add(processItem.ProcessInfo.Pid);
                }
            }

            IProcessInfoService processInfoService = new ProcessInfoService();
            processInfoService.KillProcess(pidList);

            // 杀死进程后，重新加载列表
            this.QueryPortCommandExec();
        }

        /**
     * 将ProcessInfo列表转为ProcessItemViewModel列表
     */
        private List<ProcessItemViewModel> GetProcessItemViewModel(List<ProcessInfo> processInfos)
        {
            List<ProcessItemViewModel> itemList = new List<ProcessItemViewModel>();
            foreach(ProcessInfo processInfo in processInfos){
                ProcessItemViewModel item = new ProcessItemViewModel() { ProcessInfo = processInfo };
                itemList.Add(item);
            }
            return itemList;
        }

    }
    
}
